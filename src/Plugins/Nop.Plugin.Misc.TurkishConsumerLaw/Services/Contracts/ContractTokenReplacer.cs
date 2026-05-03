using System.Text.RegularExpressions;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <inheritdoc />
public class ContractTokenReplacer : IContractTokenReplacer
{
    /// <summary>
    /// Token regex: <c>{{TokenAdı}}</c>. Türkçe karakter kabul edilir.
    /// </summary>
    private static readonly Regex TokenPattern = new(
        @"\{\{(?<name>[A-Za-zĞğÜüŞşİıÖöÇç_][A-Za-zĞğÜüŞşİıÖöÇç_0-9]*)\}\}",
        RegexOptions.Compiled);

    /// <inheritdoc />
    public virtual string Replace(string template, ContractTokenContext context)
    {
        if (string.IsNullOrEmpty(template))
            return template ?? string.Empty;

        ArgumentNullException.ThrowIfNull(context);

        var map = BuildTokenMap(context);

        return TokenPattern.Replace(template, m =>
        {
            var name = m.Groups["name"].Value;
            return map.TryGetValue(name, out var value)
                ? value ?? string.Empty
                : m.Value;  // Bilinmeyen token: dokunma (admin görsün)
        });
    }

    /// <summary>
    /// Türkçe token adlarını <see cref="ContractTokenContext"/> alanlarına eşleyen sözlük.
    /// Public static — testlerde token listesi doğrulanabilir.
    /// </summary>
    public static IDictionary<string, string?> BuildTokenMap(ContractTokenContext c) =>
        new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            // Satıcı
            ["SatıcıAdı"]      = c.SellerName,
            ["SatıcıAdresi"]   = c.SellerAddress,
            ["SatıcıTelefon"]  = c.SellerPhone,
            ["SatıcıKep"]      = c.SellerKep,
            ["SatıcıMersis"]   = c.SellerMersis,

            // Alıcı
            ["AlıcıAdı"]       = c.BuyerName,
            ["AlıcıAdresi"]    = c.BuyerAddress,
            ["AlıcıTelefon"]   = c.BuyerPhone,
            ["AlıcıTcKimlik"]  = c.BuyerTcKimlikNo,
            ["AlıcıVergiNo"]   = c.BuyerVergiNo,

            // Sipariş
            ["SiparişNo"]       = c.OrderNumber,
            ["SiparişTarihi"]   = c.OrderDate,
            ["ÜrünListesi"]     = c.ProductListHtml,

            // Ücret
            ["AraToplam"]    = c.Subtotal,
            ["KdvToplam"]    = c.TaxTotal,
            ["KargoBedeli"]  = c.ShippingFee,
            ["İndirim"]      = c.Discount,
            ["GenelToplam"]  = c.GrandTotal,

            // Adres
            ["TeslimatAdresi"] = c.ShippingAddress,
            ["FaturaAdresi"]   = c.BillingAddress,

            // Kargo / ödeme
            ["ÖdemeYöntemi"]         = c.PaymentMethod,
            ["KargoFirması"]         = c.ShippingMethod,
            ["TahminiTeslimSüresi"]  = c.EstimatedDeliveryTime,

            // Yasal sabitler
            ["CaymaSüresi"]            = c.WithdrawalDays.ToString(),
            ["ArabuluculukBilgisi"]    = c.MediationNotice
        };

    /// <summary>
    /// Default arabuluculuk bilgilendirme metni — 2026/24.05 tarihli yönetmelik değişikliği.
    /// Mesafeli Sözleşmeler Yönetmeliği gereği ÖBF'de zorunlu.
    /// </summary>
    public const string DefaultMediationNotice =
        "Parasal sınırlar dahilinde Tüketici Mahkemesi'nin görevine giren uyuşmazlıklarda " +
        "mahkemeye başvurmadan önce arabulucuya başvurma şartı bulunmaktadır. " +
        "Tüketici hakem heyetleri, parasal sınırlar dışında zorunlu yetkili mercidir.";
}
