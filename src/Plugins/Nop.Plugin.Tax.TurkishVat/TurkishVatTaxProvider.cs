using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Tax;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Tax;

namespace Nop.Plugin.Tax.TurkishVat;

/// <summary>
/// Türkiye KDV (Katma Değer Vergisi) tax provider.
///
/// Her tax category için ilgili oranı <see cref="ISettingService"/> üzerinden okur
/// (key formatı: <see cref="TurkishVatDefaults.RateSettingKey"/>).
/// Install sırasında üç standart kategori (%1/%10/%20) seed'lenir ve plugin
/// <see cref="TaxSettings.ActiveTaxProviderSystemName"/> olarak set edilir.
///
/// 2026 yürürlükteki oranlar <see cref="TurkishVatDefaults.Kdv"/> sabitlerinde tutulur;
/// admin yıllık güncellemelerde tek noktadan değer değiştirebilir.
/// </summary>
public class TurkishVatTaxProvider : BasePlugin, ITaxProvider
{
    #region Fields

    protected readonly IGenericAttributeService _genericAttributeService;
    protected readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly ILocalizationService _localizationService;
    protected readonly IOrderTotalCalculationService _orderTotalCalculationService;
    protected readonly IPaymentService _paymentService;
    protected readonly ISettingService _settingService;
    protected readonly ITaxCategoryService _taxCategoryService;
    protected readonly ITaxService _taxService;
    protected readonly IWebHelper _webHelper;
    protected readonly TaxSettings _taxSettings;

    #endregion

    #region Ctor

    public TurkishVatTaxProvider(
        IGenericAttributeService genericAttributeService,
        IHttpContextAccessor httpContextAccessor,
        ILocalizationService localizationService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentService paymentService,
        ISettingService settingService,
        ITaxCategoryService taxCategoryService,
        ITaxService taxService,
        IWebHelper webHelper,
        TaxSettings taxSettings)
    {
        _genericAttributeService = genericAttributeService;
        _httpContextAccessor = httpContextAccessor;
        _localizationService = localizationService;
        _orderTotalCalculationService = orderTotalCalculationService;
        _paymentService = paymentService;
        _settingService = settingService;
        _taxCategoryService = taxCategoryService;
        _taxService = taxService;
        _webHelper = webHelper;
        _taxSettings = taxSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Bir TaxCategory için yapılandırılmış KDV oranını döndürür.
    /// Setting bulunamazsa 0 döner — admin Configure ekranından oran girilmemişse "vergisiz" anlamı taşır.
    /// </summary>
    public async Task<TaxRateResult> GetTaxRateAsync(TaxRateRequest taxRateRequest)
    {
        var result = new TaxRateResult();

        if (taxRateRequest.TaxCategoryId == 0)
            return result;

        var key = string.Format(TurkishVatDefaults.RateSettingKey, taxRateRequest.TaxCategoryId);
        result.TaxRate = await _settingService.GetSettingByKeyAsync<decimal>(key);
        return result;
    }

    /// <summary>
    /// Sepet ve kargo üzerinden toplam vergi hesaplar.
    /// FixedOrByCountryStateZipTaxProvider implementasyonuyla aynı pattern — nopCommerce'in
    /// IOrderTotalCalculationService'ini sorgulayarak istek başına tek seferde hesaplar.
    /// </summary>
    public async Task<TaxTotalResult> GetTaxTotalAsync(TaxTotalRequest taxTotalRequest)
    {
        var ctx = _httpContextAccessor.HttpContext;
        if (ctx is not null
            && ctx.Items.TryGetValue("nop.TaxTotal", out var cached)
            && cached is (TaxTotalResult cachedResult, decimal paymentTax))
        {
            // Payment additional fee hesabı sırasında dolaylı tekrar çağrıyı önlemek için kısa devre
            if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
                return new TaxTotalResult { TaxTotal = cachedResult.TaxTotal - paymentTax };
            return cachedResult;
        }

        var taxRates = new SortedDictionary<decimal, decimal>();
        var taxTotal = decimal.Zero;

        // Sepet alt toplamı
        var (_, _, _, _, subTotalRates) = await _orderTotalCalculationService
            .GetShoppingCartSubTotalAsync(taxTotalRequest.ShoppingCart, false);

        var subTotalTax = decimal.Zero;
        foreach (var (rate, value) in subTotalRates)
        {
            subTotalTax += value;
            if (rate > 0 && value > 0)
                taxRates[rate] = taxRates.TryGetValue(rate, out var v) ? v + value : value;
        }
        taxTotal += subTotalTax;

        // Kargo
        var shippingTax = decimal.Zero;
        if (_taxSettings.ShippingIsTaxable)
        {
            var (shipExcl, _, _) = await _orderTotalCalculationService
                .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, false);
            var (shipIncl, shipRate, _) = await _orderTotalCalculationService
                .GetShoppingCartShippingTotalAsync(taxTotalRequest.ShoppingCart, true);
            if (shipExcl.HasValue && shipIncl.HasValue)
            {
                shippingTax = shipIncl.Value - shipExcl.Value;
                if (shippingTax < 0) shippingTax = 0;

                if (shipRate > 0 && shippingTax > 0)
                    taxRates[shipRate] = taxRates.TryGetValue(shipRate, out var v) ? v + shippingTax : shippingTax;
            }
        }
        taxTotal += shippingTax;

        if (!taxTotalRequest.UsePaymentMethodAdditionalFee)
            return new TaxTotalResult { TaxTotal = taxTotal };

        // Ödeme yöntemi ek ücreti vergisi
        var paymentFeeTax = decimal.Zero;
        if (_taxSettings.PaymentMethodAdditionalFeeIsTaxable)
        {
            var pmName = taxTotalRequest.Customer != null
                ? await _genericAttributeService.GetAttributeAsync<string>(
                    taxTotalRequest.Customer,
                    NopCustomerDefaults.SelectedPaymentMethodAttribute,
                    taxTotalRequest.StoreId)
                : string.Empty;

            var pmFee = await _paymentService.GetAdditionalHandlingFeeAsync(taxTotalRequest.ShoppingCart, pmName);
            var (pmExcl, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(pmFee, false, taxTotalRequest.Customer);
            var (pmIncl, pmRate) = await _taxService.GetPaymentMethodAdditionalFeeAsync(pmFee, true, taxTotalRequest.Customer);

            paymentFeeTax = pmIncl - pmExcl;
            if (paymentFeeTax < 0) paymentFeeTax = 0;

            if (pmRate > 0 && paymentFeeTax > 0)
                taxRates[pmRate] = taxRates.TryGetValue(pmRate, out var v) ? v + paymentFeeTax : paymentFeeTax;
        }
        taxTotal += paymentFeeTax;

        if (!taxRates.Any())
            taxRates.Add(0, 0);

        if (taxTotal < 0) taxTotal = 0;

        var totalResult = new TaxTotalResult { TaxTotal = taxTotal, TaxRates = taxRates };

        ctx?.Items.TryAdd("nop.TaxTotal", (totalResult, paymentFeeTax));
        return totalResult;
    }

    /// <summary>
    /// Plugin'in admin Configure sayfası — şu an basit settings (oranları düzenleme) için reserved.
    /// Faz 1B'de UI eklendiğinde bu URL doluacak; şimdilik default plugin sayfasına yönlenir.
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return $"{_webHelper.GetStoreLocation()}Admin/Setting/Tax";
    }

    /// <summary>
    /// Install — kategorileri seed'le, oranları setting'e yaz, plugin'i aktif tax provider yap.
    /// </summary>
    public override async Task InstallAsync()
    {
        // 1) Kategorileri seed (zaten varsa atla — system koruyucu)
        var existingCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();

        foreach (var (name, rate, displayOrder) in TurkishVatDefaults.DefaultCategories)
        {
            var existing = existingCategories.FirstOrDefault(c =>
                string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));

            int categoryId;
            if (existing is null)
            {
                var category = new TaxCategory
                {
                    Name = name,
                    DisplayOrder = displayOrder
                };
                await _taxCategoryService.InsertTaxCategoryAsync(category);
                categoryId = category.Id;
            }
            else
            {
                categoryId = existing.Id;
            }

            // 2) Bu kategori için oran setting'i yaz
            var settingKey = string.Format(TurkishVatDefaults.RateSettingKey, categoryId);
            await _settingService.SetSettingAsync(settingKey, rate);
        }

        // 3) Plugin'i aktif tax provider yap (mevcut ayarı override eder — kasıtlı:
        //    Türkiye lokalizasyon plugin'inin tüm KDV akışını yönetmesi beklenir).
        _taxSettings.ActiveTaxProviderSystemName = TurkishVatDefaults.SystemName;
        await _settingService.SaveSettingAsync(_taxSettings);

        // 4) Lokalizasyon
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            [$"{TurkishVatDefaults.LocaleStringResourcesPrefix}.Description"] = "Türkiye KDV Tax Provider",
            [$"{TurkishVatDefaults.LocaleStringResourcesPrefix}.Categories.Standart"] = "KDV %20 (Standart)",
            [$"{TurkishVatDefaults.LocaleStringResourcesPrefix}.Categories.Indirimli10"] = "KDV %10 (İndirimli)",
            [$"{TurkishVatDefaults.LocaleStringResourcesPrefix}.Categories.Indirimli1"] = "KDV %1 (Temel Gıda/Kitap)"
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall — kategori-bazlı oran setting'lerini sil, lokalizasyon'u kaldır.
    /// Tax category'lerin kendisi korunur (ürünlere atanmış olabilir, manuel delete edilmesi
    /// admin'in tercihine bırakılır).
    /// </summary>
    public override async Task UninstallAsync()
    {
        var categories = await _taxCategoryService.GetAllTaxCategoriesAsync();
        foreach (var c in categories)
        {
            var key = string.Format(TurkishVatDefaults.RateSettingKey, c.Id);
            var setting = await _settingService.GetSettingAsync(key);
            if (setting is not null)
                await _settingService.DeleteSettingAsync(setting);
        }

        // Active tax provider biz isek temizle — başkasıysa dokunma
        if (string.Equals(_taxSettings.ActiveTaxProviderSystemName, TurkishVatDefaults.SystemName, StringComparison.OrdinalIgnoreCase))
        {
            _taxSettings.ActiveTaxProviderSystemName = string.Empty;
            await _settingService.SaveSettingAsync(_taxSettings);
        }

        await _localizationService.DeleteLocaleResourcesAsync(TurkishVatDefaults.LocaleStringResourcesPrefix);

        await base.UninstallAsync();
    }

    #endregion
}
