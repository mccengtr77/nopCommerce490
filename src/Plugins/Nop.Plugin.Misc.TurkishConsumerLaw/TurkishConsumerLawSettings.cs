using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.TurkishConsumerLaw;

/// <summary>
/// TurkishConsumerLaw plugin ayarları.
/// Multi-store: Her mağazanın kendi ETBİS, İYS, sözleşme şablonu olabilir.
/// </summary>
public class TurkishConsumerLawSettings : ISettings
{
    #region ETBİS

    /// <summary>
    /// ETBİS karekod footer'da gösterilsin mi
    /// </summary>
    public bool ShowEtbisQrCodeInFooter { get; set; } = true;

    #endregion

    #region İYS

    /// <summary>
    /// İYS API entegrasyonu aktif mi (false = mock mode)
    /// </summary>
    public bool IysIntegrationEnabled { get; set; }

    /// <summary>
    /// Marka adı (İYS'de kayıtlı)
    /// </summary>
    public string IysBrandCode { get; set; } = string.Empty;

    /// <summary>
    /// API base URL (sandbox: https://api.iys.org.tr/sandbox, prod: https://api.iys.org.tr)
    /// </summary>
    public string IysApiBaseUrl { get; set; } = "https://api.iys.org.tr/sandbox";

    public string IysUsername { get; set; } = string.Empty;

    /// <summary>
    /// API password — şifreli sakla
    /// </summary>
    public string IysPasswordEncrypted { get; set; } = string.Empty;

    #endregion

    #region KVKK / 2026 yeni standart

    /// <summary>
    /// Kayıt formunda KVKK aydınlatma metni "okudum" beyanı zorunlu mu
    /// </summary>
    public bool KvkkDisclosureMandatory { get; set; } = true;

    /// <summary>
    /// 2026/347 İlke Kararı: aydınlatma ve açık rıza AYRI tutuluyor — bu setting
    /// kontroldür, default true (kapatılırsa hukuka aykırı). Sadece eski sistemlerden
    /// migration için geçici olarak açılabilir.
    /// </summary>
    public bool EnforceSeparateDisclosureAndConsent { get; set; } = true;

    #endregion

    #region Çerez

    /// <summary>
    /// Çerez consent banner aktif mi
    /// </summary>
    public bool CookieConsentEnabled { get; set; } = true;

    /// <summary>
    /// Banner üst/alt — "top" / "bottom"
    /// </summary>
    public string CookieBannerPosition { get; set; } = "bottom";

    #endregion

    #region Cayma hakkı

    /// <summary>
    /// Cayma hakkı süresi (gün) — yasa 14, override için
    /// </summary>
    public int WithdrawalPeriodDays { get; set; } = TurkishConsumerLawDefaults.Withdrawal.PeriodDays;

    /// <summary>
    /// İade kargo ücretini satıcı mı öder (2026 yeni kural — true olmalı)
    /// </summary>
    public bool ReturnShippingPaidBySeller { get; set; } = TurkishConsumerLawDefaults.Withdrawal.ReturnShippingPaidBySeller;

    #endregion

    #region Şablon ve PDF

    /// <summary>
    /// Sözleşme PDF font ailesi (Türkçe karakter desteği için)
    /// </summary>
    public string PdfFontFamily { get; set; } = "Roboto";

    /// <summary>
    /// MSS/ÖBF saklama süresi (yıl)
    /// </summary>
    public int ContractRetentionYears { get; set; } = TurkishConsumerLawDefaults.Retention.ContractRetentionYears;

    #endregion
}
