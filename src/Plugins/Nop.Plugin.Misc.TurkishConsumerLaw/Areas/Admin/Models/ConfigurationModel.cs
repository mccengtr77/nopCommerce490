using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Areas.Admin.Models;

/// <summary>
/// TurkishConsumerLaw admin Configure model — ETBİS, KVKK, çerez ve İYS ayarlarını barındırır.
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region ETBİS

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.ShowInFooter")]
    public bool ShowEtbisQrCodeInFooter { get; set; }

    /// <summary>
    /// Mevcut aktif ETBİS kaydı — null ise admin "yeni kayıt" formu görür
    /// </summary>
    public EtbisRegistrationModel ActiveEtbis { get; set; } = new();

    #endregion

    #region KVKK

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.KvkkDisclosureMandatory")]
    public bool KvkkDisclosureMandatory { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.EnforceSeparateDisclosureAndConsent")]
    public bool EnforceSeparateDisclosureAndConsent { get; set; }

    #endregion

    #region Çerez

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.CookieConsentEnabled")]
    public bool CookieConsentEnabled { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.CookieBannerPosition")]
    public string CookieBannerPosition { get; set; } = "bottom";

    #endregion

    #region Cayma

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.WithdrawalPeriodDays")]
    public int WithdrawalPeriodDays { get; set; }

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Settings.ReturnShippingPaidBySeller")]
    public bool ReturnShippingPaidBySeller { get; set; }

    #endregion
}

public record EtbisRegistrationModel : BaseNopEntityModel
{
    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.MersisNo")]
    public string MersisNo { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.TradeName")]
    public string TradeName { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.RegistrationDate")]
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow.Date;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.VerificationUrl")]
    public string VerificationUrl { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.QrCodeHtml")]
    public string QrCodeHtml { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Misc.TurkishConsumerLaw.Etbis.Active")]
    public bool IsActive { get; set; } = true;
}
