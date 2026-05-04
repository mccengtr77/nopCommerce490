using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.TurkeyCore.Services.Address;
using Nop.Plugin.Misc.TurkeyCore.Services.CartLock;
using Nop.Plugin.Misc.TurkeyCore.Services.Customer;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Plugin.Misc.TurkeyCore.Services.GibLookup;
using Nop.Plugin.Misc.TurkeyCore.Services.Location;
using Nop.Plugin.Misc.TurkeyCore.Services.Product;
using Nop.Plugin.Misc.TurkeyCore.Services.TaxOffice;
using Nop.Plugin.Misc.TurkeyCore.Services.Validation;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Misc.TurkeyCore.Infrastructure;

/// <summary>
/// TurkeyCore plugin servis kayıt sınıfı
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Servisleri DI container'a kaydet
    /// </summary>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Validasyon servisleri (state'siz, singleton)
        services.AddSingleton<ITurkishValidationService, TurkishValidationService>();

        // Lokasyon servisleri — IRepository scoped olduğu için scoped
        services.AddScoped<ITurkishLocationService, TurkishLocationService>();
        services.AddScoped<ITurkishTaxOfficeService, TurkishTaxOfficeService>();

        // Customer/Address köprü servisleri (encrypt/decrypt + extension lookup)
        services.AddScoped<ITurkishCustomerService, TurkishCustomerService>();
        services.AddScoped<ITurkishAddressService, TurkishAddressService>();

        // Product extension — döviz bazlı ürün fiyatı (persisted recalc).
        // Admin Save veya TCMB scheduled task → Product.Price/OldPrice/ProductCost DB'ye yazılır,
        // nopCommerce'in tüm fiyat akışı (search, filter, discount, marketplace, tier price) tek doğru fiyatı görür.
        services.AddScoped<ITurkishProductExtensionService, TurkishProductExtensionService>();

        // Sepet kalemi fiyat snapshot — sepete eklenince TL kur lock'lanır
        services.AddScoped<ITurkishCartItemPriceLockService, TurkishCartItemPriceLockService>();

        // TCMB döviz kuru — typed HttpClient ile (proxy desteği için WithProxy)
        services.AddHttpClient<ITcmbExchangeRateService, TcmbExchangeRateService>().WithProxy();

        // GİB mükellef sorgu — default mock; gerçek implementasyon EFatura plugin'inde
        services.AddScoped<IGibMukellefService, MockGibMukellefService>();

        // Plugin override view path'leri — _CreateOrUpdateAddress.cshtml gibi shared partial'ları
        // override edebilmek için Razor view engine'a ek search path ekle.
        services.Configure<RazorViewEngineOptions>(options =>
        {
            options.ViewLocationExpanders.Add(new PluginViewLocationExpander());
        });
    }

    /// <summary>
    /// Middleware yapılandırması
    /// </summary>
    public void Configure(IApplicationBuilder application)
    {
    }

    /// <summary>
    /// Startup sırası — plugin'ler için yüksek değer
    /// </summary>
    public int Order => 3000;
}
