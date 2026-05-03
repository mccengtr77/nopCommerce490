namespace Nop.Plugin.Misc.TurkishConsumerLaw.Localization;

/// <summary>
/// TurkishConsumerLaw plugin lokalizasyon kaynakları.
/// </summary>
public static class LocaleResources
{
    public const string Prefix = "Plugins.Misc.TurkishConsumerLaw";

    public static readonly IDictionary<string, string> Turkish = new Dictionary<string, string>
    {
        // Plugin tanıtımı
        [$"{Prefix}.FriendlyName"] = "Türkiye Tüketici Hukuku",
        [$"{Prefix}.Description"] = "Türk tüketici hukuku uyumluluk paketi (ETBİS, MSS/ÖBF, cayma, KVKK, çerez, İYS, garanti)",

        // Admin menü
        [$"{Prefix}.Menu.Etbis"] = "ETBİS",
        [$"{Prefix}.Menu.Contracts"] = "Sözleşme Şablonları",
        [$"{Prefix}.Menu.Withdrawal"] = "Cayma Talepleri",
        [$"{Prefix}.Menu.Kvkk"] = "KVKK",
        [$"{Prefix}.Menu.Cookies"] = "Çerez Tanımları",
        [$"{Prefix}.Menu.Iys"] = "İYS",
        [$"{Prefix}.Menu.Warranty"] = "Garanti",
        [$"{Prefix}.Menu.Complaints"] = "Şikayetler",

        // ETBİS
        [$"{Prefix}.Etbis.Title"] = "ETBİS (Elektronik Ticaret Bilgi Sistemi)",
        [$"{Prefix}.Etbis.LegalNote"] = "6563 sayılı E-Ticaret Kanunu gereği ETBİS karekodu site footer'ında zorunludur.",
        [$"{Prefix}.Etbis.MersisNo"] = "MERSİS No",
        [$"{Prefix}.Etbis.MersisNo.Hint"] = "16 haneli MERSİS numaranızı girin",
        [$"{Prefix}.Etbis.TradeName"] = "Ticari Ünvan",
        [$"{Prefix}.Etbis.TradeName.Hint"] = "MERSİS'te kayıtlı ticari ünvanınız",
        [$"{Prefix}.Etbis.QrCodeHtml"] = "Karekod HTML/Embed Kodu",
        [$"{Prefix}.Etbis.QrCodeHtml.Hint"] = "eticaret.gov.tr'den aldığınız ETBİS karekod HTML kodunu yapıştırın (örn. <img src=\"...\"/> veya inline SVG)",
        [$"{Prefix}.Etbis.RegistrationDate"] = "Kayıt Tarihi",
        [$"{Prefix}.Etbis.VerificationUrl"] = "Doğrulama URL",
        [$"{Prefix}.Etbis.VerificationUrl.Hint"] = "ETBİS sicil doğrulama bağlantısı (eticaret.gov.tr/...)",
        [$"{Prefix}.Etbis.Active"] = "Aktif",
        [$"{Prefix}.Etbis.ShowInFooter"] = "Footer'da Göster",
        [$"{Prefix}.Etbis.ShowInFooter.Hint"] = "ETBİS karekodu storefront footer'ında göster (yasal zorunluluk)",
        [$"{Prefix}.Etbis.NotConfigured"] = "ETBİS bilgileriniz henüz yapılandırılmadı. 6563 sayılı kanun gereği zorunludur.",

        // KVKK ortak
        [$"{Prefix}.Kvkk.LegalNote"] = "KVKK 2026/347 İlke Kararı: Aydınlatma metni ile açık rıza metni AYRI olmalı; \"okudum + rıza veriyorum\" tek kutucuk hukuka aykırı.",

        // Cayma
        [$"{Prefix}.Withdrawal.LegalNote2026"] = "2026 güncellemesi: Cep telefonu/tablet/bilgisayar cayma kapsamında, iade kargo ücretini satıcı öder.",
    };

    public static readonly IDictionary<string, string> English = new Dictionary<string, string>
    {
        [$"{Prefix}.FriendlyName"] = "Turkish Consumer Law Compliance",
        [$"{Prefix}.Description"] = "Turkish consumer law compliance pack (ETBIS, distance sales, withdrawal, KVKK/personal data, cookies, IYS, warranty)",

        [$"{Prefix}.Etbis.Title"] = "ETBIS (Turkish E-Commerce Information System)",
        [$"{Prefix}.Etbis.MersisNo"] = "MERSIS No",
        [$"{Prefix}.Etbis.TradeName"] = "Trade Name",
        [$"{Prefix}.Etbis.QrCodeHtml"] = "QR Code HTML",
        [$"{Prefix}.Etbis.ShowInFooter"] = "Show in footer",
    };
}
