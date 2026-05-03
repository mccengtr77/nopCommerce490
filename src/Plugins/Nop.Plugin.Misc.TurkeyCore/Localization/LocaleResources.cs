namespace Nop.Plugin.Misc.TurkeyCore.Localization;

/// <summary>
/// Plugin lokalizasyon kaynakları — Türkçe (varsayılan) ve İngilizce.
///
/// nopCommerce plugin convention'ı: <c>tr-TR.xml</c> gibi ayrı XML dosyaları yerine
/// <see cref="ILocalizationService.AddOrUpdateLocaleResourceAsync"/> ile in-code Dictionary
/// olarak install sırasında kayıt edilir. Admin → Languages → Resources üzerinden
/// düzenlenebilir, multi-language standart şekilde çalışır.
///
/// Yeni dil eklemek için: yeni bir static dictionary tanımla ve plugin
/// InstallAsync içinde languageId parametresiyle kayıt et.
/// </summary>
public static class LocaleResources
{
    public const string Prefix = "Plugins.Misc.TurkeyCore";

    /// <summary>
    /// Türkçe (varsayılan dil — languageId = null geçilirse tüm dillere yazılır,
    /// İngilizce için ayrı kayıt yapılır).
    /// </summary>
    public static readonly IDictionary<string, string> Turkish = new Dictionary<string, string>
    {
        // Plugin tanıtımı
        [$"{Prefix}.FriendlyName"] = "Türkiye Core",
        [$"{Prefix}.Description"] = "Türkiye e-ticaret çekirdek altyapı modülü",

        // Admin menü
        [$"{Prefix}.Menu.TurkeyCore"] = "Türkiye Ayarları",
        [$"{Prefix}.Menu.Settings"] = "Genel Ayarlar",
        [$"{Prefix}.Menu.Provinces"] = "İl / İlçe / Mahalle",
        [$"{Prefix}.Menu.TaxOffices"] = "Vergi Daireleri",
        [$"{Prefix}.Menu.ExchangeRates"] = "Döviz Kurları",

        // Ayarlar
        [$"{Prefix}.Settings.TcmbAutoUpdateEnabled"] = "TCMB Otomatik Güncelleme",
        [$"{Prefix}.Settings.TcmbAutoUpdateEnabled.Hint"] = "TCMB döviz kurları otomatik güncellensin mi?",
        [$"{Prefix}.Settings.GibMukellefServiceUrl"] = "GİB Mükellef Servis URL",
        [$"{Prefix}.Settings.GibMukellefServiceUrl.Hint"] = "GİB e-fatura mükellef sorgu servisi endpoint adresi",
        [$"{Prefix}.Settings.GibCacheTimeMinutes"] = "GİB Cache Süresi (dk)",
        [$"{Prefix}.Settings.GibCacheTimeMinutes.Hint"] = "GİB mükellef sorgu sonuçlarının cache'de tutulma süresi (dakika)",
        [$"{Prefix}.Settings.TcknRequiredOnRegistration"] = "Kayıtta TCKN Zorunlu",
        [$"{Prefix}.Settings.TcknRequiredOnRegistration.Hint"] = "Müşteri kaydında TC Kimlik No zorunlu olsun mu?",
        [$"{Prefix}.Settings.VknRequiredForCorporate"] = "Kurumsal VKN Zorunlu",
        [$"{Prefix}.Settings.VknRequiredForCorporate.Hint"] = "Kurumsal müşteriler için Vergi Kimlik No zorunlu olsun mu?",
        [$"{Prefix}.Settings.LocationSelectionRequired"] = "Lokasyon Seçimi Zorunlu",
        [$"{Prefix}.Settings.LocationSelectionRequired.Hint"] = "Checkout'ta il/ilçe/mahalle seçimi zorunlu olsun mu?",
        [$"{Prefix}.Settings.DefaultVatRate"] = "Varsayılan KDV Oranı",
        [$"{Prefix}.Settings.DefaultVatRate.Hint"] = "Varsayılan KDV oranı (%)",

        // Validasyon mesajları
        [$"{Prefix}.Validation.TcKimlikNo.Invalid"] = "Geçersiz TC Kimlik No",
        [$"{Prefix}.Validation.TcKimlikNo.Required"] = "TC Kimlik No zorunludur",
        [$"{Prefix}.Validation.VergiNo.Invalid"] = "Geçersiz Vergi Kimlik No",
        [$"{Prefix}.Validation.VergiNo.Required"] = "Vergi Kimlik No zorunludur",
        [$"{Prefix}.Validation.Iban.Invalid"] = "Geçersiz IBAN",
        [$"{Prefix}.Validation.Gsm.Invalid"] = "Geçersiz GSM numarası",
        [$"{Prefix}.Validation.Gsm.InvalidFormat"] = "GSM numarası +90 5XX XXX XX XX formatında olmalıdır",

        // Lokasyon
        [$"{Prefix}.Location.Province"] = "İl",
        [$"{Prefix}.Location.Province.Select"] = "İl seçiniz",
        [$"{Prefix}.Location.District"] = "İlçe",
        [$"{Prefix}.Location.District.Select"] = "İlçe seçiniz",
        [$"{Prefix}.Location.Neighborhood"] = "Mahalle",
        [$"{Prefix}.Location.Neighborhood.Select"] = "Mahalle seçiniz",
        [$"{Prefix}.Location.PostalCode"] = "Posta Kodu",
        [$"{Prefix}.Location.BuildingNo"] = "Bina No",
        [$"{Prefix}.Location.ApartmentNo"] = "Daire No",

        // Müşteri tipi
        [$"{Prefix}.CustomerType"] = "Müşteri Tipi",
        [$"{Prefix}.CustomerType.Individual"] = "Bireysel",
        [$"{Prefix}.CustomerType.Corporate"] = "Kurumsal",

        // Vergi dairesi
        [$"{Prefix}.TaxOffice"] = "Vergi Dairesi",
        [$"{Prefix}.TaxOffice.Select"] = "Vergi dairesi seçiniz",

        // TCMB
        [$"{Prefix}.Tcmb.RatesUpdated"] = "TCMB döviz kurları güncellendi",
        [$"{Prefix}.Tcmb.UpdateFailed"] = "TCMB döviz kuru güncellemesi başarısız",
        [$"{Prefix}.Tcmb.RefreshNow"] = "Şimdi Güncelle",
        [$"{Prefix}.Tcmb.LatestRates"] = "En Güncel Kurlar",
        [$"{Prefix}.Tcmb.NoRatesYet"] = "Henüz TCMB kuru çekilmedi",

        // GİB
        [$"{Prefix}.Gib.EFaturaMukellefi"] = "E-Fatura Mükellefi",
        [$"{Prefix}.Gib.EFaturaMukellefiDegil"] = "E-Fatura Mükellefi Değil",
        [$"{Prefix}.Gib.SorguBasarisiz"] = "GİB mükellef sorgusu başarısız",

        // KEP
        [$"{Prefix}.Kep.Address"] = "KEP Adresi",
        [$"{Prefix}.Kep.Address.Hint"] = "Kayıtlı Elektronik Posta adresi (kurumsal müşteriler için)",

        // Kurumsal alanlar
        [$"{Prefix}.Corporate.Mersis"] = "MERSİS No",
        [$"{Prefix}.Corporate.TicaretSicilNo"] = "Ticaret Sicil No",

        // Admin Configure sayfası
        [$"{Prefix}.Admin.DataStatus"] = "Veri Durumu",
        [$"{Prefix}.Admin.DataStatus.Empty"] = "Boş",
        [$"{Prefix}.Admin.DataStatus.Incomplete"] = "Eksik",
        [$"{Prefix}.Admin.DataStatus.NoLocationData"] =
            "İlçe, mahalle veya vergi dairesi verisi henüz yüklenmemiş. Storefront cascading dropdown'larının çalışması için bu verileri admin import aracıyla veya elle eklemelisiniz."
    };

    /// <summary>
    /// İngilizce — sadece kullanıcıya yönelik kritik mesajlar (admin TR'de çalışacak).
    /// İngilizce-only mağazalar için fallback.
    /// </summary>
    public static readonly IDictionary<string, string> English = new Dictionary<string, string>
    {
        [$"{Prefix}.FriendlyName"] = "Turkey Core",
        [$"{Prefix}.Description"] = "Turkey e-commerce core infrastructure module",

        [$"{Prefix}.Validation.TcKimlikNo.Invalid"] = "Invalid Turkish ID Number",
        [$"{Prefix}.Validation.TcKimlikNo.Required"] = "Turkish ID Number is required",
        [$"{Prefix}.Validation.VergiNo.Invalid"] = "Invalid Tax ID Number",
        [$"{Prefix}.Validation.VergiNo.Required"] = "Tax ID Number is required",
        [$"{Prefix}.Validation.Iban.Invalid"] = "Invalid IBAN",
        [$"{Prefix}.Validation.Gsm.Invalid"] = "Invalid mobile number",
        [$"{Prefix}.Validation.Gsm.InvalidFormat"] = "Mobile number must be in +90 5XX XXX XX XX format",

        [$"{Prefix}.Location.Province"] = "Province",
        [$"{Prefix}.Location.Province.Select"] = "Select province",
        [$"{Prefix}.Location.District"] = "District",
        [$"{Prefix}.Location.District.Select"] = "Select district",
        [$"{Prefix}.Location.Neighborhood"] = "Neighborhood",
        [$"{Prefix}.Location.Neighborhood.Select"] = "Select neighborhood",
        [$"{Prefix}.Location.PostalCode"] = "Postal Code",
        [$"{Prefix}.Location.BuildingNo"] = "Building No",
        [$"{Prefix}.Location.ApartmentNo"] = "Apartment No",

        [$"{Prefix}.CustomerType"] = "Customer Type",
        [$"{Prefix}.CustomerType.Individual"] = "Individual",
        [$"{Prefix}.CustomerType.Corporate"] = "Corporate",

        [$"{Prefix}.TaxOffice"] = "Tax Office",
        [$"{Prefix}.TaxOffice.Select"] = "Select tax office",

        [$"{Prefix}.Kep.Address"] = "Registered E-Mail Address (KEP)",
        [$"{Prefix}.Corporate.Mersis"] = "MERSIS No",
        [$"{Prefix}.Corporate.TicaretSicilNo"] = "Trade Registry No"
    };
}
