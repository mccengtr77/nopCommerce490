using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using Nop.Services.Events;
using Nop.Services.Logging;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Events;

/// <summary>
/// Müşteri kaydı tamamlandığında storefront kayıt formundaki KVKK ve ETK
/// onaylarını otomatik olarak <see cref="ConsentRecord"/> tablosuna kaydeder.
///
/// Form alanları (storefront ViewComponent'lerden):
/// - <c>tcl-disclosure-read</c> → checkbox (KVKK aydınlatma okundu beyanı)
/// - <c>tcl-consent</c> → multi-value (her seçilen scope int değeri)
///
/// İşaretlenmemiş scope'lar için "denied" kayıt eklenmez —
/// "rıza vermedi" durumunu açıkça loglamak istiyorsak ayrı checkbox listesi gerekir.
/// </summary>
public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    #region Fields

    protected readonly IConsentRecordService _consentRecordService;
    protected readonly IExplicitConsentService _explicitConsentService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;
    protected readonly ILogger _logger;

    #endregion

    #region Ctor

    public CustomerRegisteredConsumer(
        IConsentRecordService consentRecordService,
        IExplicitConsentService explicitConsentService,
        IHttpContextAccessor httpContextAccessor,
        IStoreContext storeContext,
        IWebHelper webHelper,
        ILogger logger)
    {
        _consentRecordService = consentRecordService;
        _explicitConsentService = explicitConsentService;
        _httpContextAccessor = httpContextAccessor;
        _storeContext = storeContext;
        _webHelper = webHelper;
        _logger = logger;
    }

    #endregion

    #region Methods

    public async Task HandleEventAsync(CustomerRegisteredEvent eventMessage)
    {
        var customer = eventMessage?.Customer;
        if (customer is null || customer.Id <= 0)
            return;

        var form = _httpContextAccessor.HttpContext?.Request?.Form;
        if (form is null)
            return; // Form context yok (örn. API kayıt) — sessizce çık

        // Seçilmiş scope'ları çek (bilinmeyenler ParseScopes'da atılır)
        var grantedScopes = ParseScopes(form);
        if (grantedScopes.Count == 0)
            return;

        try
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var activeTexts = await _explicitConsentService.GetActiveAsync(store.Id);
            var versionByScope = activeTexts.ToDictionary(t => t.Scope, t => t.Version);

            var batch = grantedScopes
                .Select(scope =>
                {
                    versionByScope.TryGetValue(scope, out var version);
                    return (Scope: scope, Granted: true, TextVersion: version ?? "1.0");
                })
                .ToList();

            var ip = _webHelper.GetCurrentIpAddress();
            var ua = _httpContextAccessor.HttpContext!.Request.Headers.UserAgent.ToString();

            await _consentRecordService.RecordBatchAsync(
                customer.Id, batch, ConsentSource.Registration, ip, ua);
        }
        catch (Exception ex)
        {
            // Kayıt akışını bloklamamak için yutuyoruz; ama loglu kalsın
            await _logger.ErrorAsync(
                $"KVKK consent kayıt başarısız (CustomerId={customer.Id}): {ex.Message}",
                ex, customer);
        }
    }

    /// <summary>
    /// Form'dan <c>tcl-consent</c> alanlarını okur ve <see cref="ConsentScope"/>
    /// enum değerlerine çevirir. Bilinmeyen değerleri atılır.
    /// Public static — test edilebilirlik için doğrudan çağrılabilir.
    /// </summary>
    public static IList<ConsentScope> ParseScopes(IFormCollection form)
    {
        var raw = form["tcl-consent"];
        if (raw.Count == 0)
            return Array.Empty<ConsentScope>();

        var scopes = new List<ConsentScope>(raw.Count);
        foreach (var value in raw)
        {
            if (int.TryParse(value, out var scopeInt)
                && Enum.IsDefined(typeof(ConsentScope), scopeInt))
            {
                scopes.Add((ConsentScope)scopeInt);
            }
        }
        return scopes;
    }

    #endregion
}
