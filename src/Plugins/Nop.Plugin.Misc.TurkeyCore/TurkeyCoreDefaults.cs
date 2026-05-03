using Nop.Core.Caching;

namespace Nop.Plugin.Misc.TurkeyCore;

/// <summary>
/// TurkeyCore plugin sabitleri
/// </summary>
public static class TurkeyCoreDefaults
{
    /// <summary>
    /// Plugin system name
    /// </summary>
    public const string SystemName = "Misc.TurkeyCore";

    /// <summary>
    /// Plugin yapılandırma sayfası route adı
    /// </summary>
    public const string ConfigurationRouteName = "Plugin.Misc.TurkeyCore.Configure";

    /// <summary>
    /// Lokalizasyon kaynak prefix'i
    /// </summary>
    public const string LocaleStringResourcesPrefix = "Plugins.Misc.TurkeyCore";

    /// <summary>
    /// CacheKey tanımları — nopCommerce <see cref="IStaticCacheManager"/> ve
    /// <see cref="IRepository{TEntity}"/> callback pattern'i ile kullanılır.
    ///
    /// CacheTime, <see cref="CacheKey"/>'in default'unu (AppSettings/CacheConfig) kullanır.
    /// Plugin lokasyon verisi gibi seyrek değişen kayıtlar için CacheConfig globalde
    /// yeterince uzundur; özel ihtiyaç olursa <see cref="CacheKey.CacheTime"/>
    /// kullanım anında set edilir.
    /// </summary>
    public static class Cache
    {
        /// <summary>Tüm aktif iller (parametresiz)</summary>
        public static CacheKey ProvincesAll => new("Nop.Plugin.TurkeyCore.Provinces.All");

        /// <summary>Bir ile bağlı ilçeler — {0} = ProvinceId</summary>
        public static CacheKey DistrictsByProvince => new("Nop.Plugin.TurkeyCore.Districts.ByProvince.{0}");

        /// <summary>Bir ilçeye bağlı mahalleler — {0} = DistrictId</summary>
        public static CacheKey NeighborhoodsByDistrict => new("Nop.Plugin.TurkeyCore.Neighborhoods.ByDistrict.{0}");

        /// <summary>Posta kodu ile mahalle araması — {0} = PostalCode</summary>
        public static CacheKey NeighborhoodsByPostalCode => new("Nop.Plugin.TurkeyCore.Neighborhoods.ByPostalCode.{0}");

        /// <summary>Tüm vergi daireleri</summary>
        public static CacheKey TaxOfficesAll => new("Nop.Plugin.TurkeyCore.TaxOffices.All");

        /// <summary>Bir ile bağlı vergi daireleri — {0} = ProvinceId</summary>
        public static CacheKey TaxOfficesByProvince => new("Nop.Plugin.TurkeyCore.TaxOffices.ByProvince.{0}");

        /// <summary>TCMB güncel döviz kurları — tüm para birimleri tek dict olarak cache'lenir</summary>
        public static CacheKey LatestExchangeRates => new("Nop.Plugin.TurkeyCore.ExchangeRates.Latest");

        /// <summary>GİB e-fatura mükellef sorgu sonucu — {0} = VKN/TCKN</summary>
        public static CacheKey GibMukellef => new("Nop.Plugin.TurkeyCore.GibMukellef.{0}");
    }

    /// <summary>
    /// TCMB döviz kuru servisi sabitleri
    /// </summary>
    public static class Tcmb
    {
        public const string ExchangeRateFeedUrl = "https://www.tcmb.gov.tr/kurlar/today.xml";
        public const string ScheduleTaskType = "Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate.ExchangeRateBackgroundTask, Nop.Plugin.Misc.TurkeyCore";
        public const string ScheduleTaskName = "TCMB Döviz Kuru Güncelleme";
        public const int DefaultScheduleIntervalSeconds = 86400; // 24 saat
    }

    /// <summary>
    /// Türkiye'ye özgü validasyon sabitleri
    /// </summary>
    public static class Validation
    {
        public const int TcKimlikNoLength = 11;
        public const int VergiNoLength = 10;
        public const int IbanTrLength = 26;
        public const string IbanTrPrefix = "TR";
        public const string GsmCountryCode = "+90";
    }

    /// <summary>
    /// Türkiye ülke kimliği — ISO 3166-1 alpha-2 (nopCommerce Country.TwoLetterIsoCode ile eşleşir)
    /// </summary>
    public static class Country
    {
        public const string TurkeyTwoLetterIsoCode = "TR";
        public const string TurkeyThreeLetterIsoCode = "TUR";
    }

    /// <summary>
    /// Storefront widget zone hook'ları — adres formuna ViewComponent inject etmek için
    /// </summary>
    public static class WidgetZones
    {
        // Address book (My account → Add new address) ve checkout adres formlarının altı.
        // nopCommerce 4.90 native template (Views/Shared/_CreateOrUpdateAddress.cshtml)
        // bu widget zone'u her yerde çağırıyor (address book + register + checkout).
        public const string AddressBottom = "address_bottom";
    }

    /// <summary>
    /// Admin menü ve route sabitleri
    /// </summary>
    public static class Admin
    {
        public const string MenuSystemName = "TurkeyCore";
        public const string SettingsController = "TurkeyCoreSettings";
        public const string ProvinceController = "Province";
        public const string TaxOfficeController = "TaxOffice";
    }
}
