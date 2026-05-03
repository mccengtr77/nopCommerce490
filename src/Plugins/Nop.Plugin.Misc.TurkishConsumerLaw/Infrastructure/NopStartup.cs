using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.DataSubjectRequests;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Withdrawal;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Infrastructure;

/// <summary>
/// TurkishConsumerLaw plugin servis kayıt sınıfı.
/// </summary>
public class NopStartup : INopStartup
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // ETBİS modülü
        services.AddScoped<IEtbisService, EtbisService>();

        // Çerez consent modülü
        services.AddScoped<ICookieDefinitionService, CookieDefinitionService>();
        services.AddScoped<ICookieConsentService, CookieConsentService>();

        // KVKK modülü (aydınlatma + açık rıza + onay kayıtları)
        services.AddScoped<IDisclosureTextService, DisclosureTextService>();
        services.AddScoped<IExplicitConsentService, ExplicitConsentService>();
        services.AddScoped<IConsentRecordService, ConsentRecordService>();

        // KVKK m.11 veri sahibi başvuruları
        services.AddScoped<IDataSubjectRequestService, DataSubjectRequestService>();

        // Cayma hakkı (14 gün, 2026 yeni kurallar)
        services.AddScoped<IWithdrawalEligibilityChecker, WithdrawalEligibilityChecker>();
        services.AddScoped<IWithdrawalService, WithdrawalService>();

        // İYS, Contracts, Warranty, Complaints modülleri sırayla eklenir
    }

    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// TurkeyCore'dan (3000) sonra çalışsın
    /// </summary>
    public int Order => 3100;
}
