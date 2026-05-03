using Nop.Core;

namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// nopCommerce'in <see cref="Nop.Core.Domain.Customers.Customer"/> entity'sini
/// değiştirmeden Türkiye'ye özgü alanları paralel tutmak için kullanılan extension tablosu.
///
/// Hassas alanlar (TCKN, VKN) <see cref="Nop.Services.Security.IEncryptionService"/> ile
/// şifrelenmiş halde saklanmalıdır — service katmanında ele alınır.
/// </summary>
public class TurkishCustomerExtension : BaseEntity
{
    /// <summary>
    /// nopCommerce Customer Id (FK)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// TC Kimlik No — şifreli saklanır (11 hane, plain hali validasyon servisindedir)
    /// </summary>
    public string? TcKimlikNo { get; set; }

    /// <summary>
    /// Vergi Kimlik No — şifreli saklanır (10 hane, kurumsal müşteriler için)
    /// </summary>
    public string? VergiNo { get; set; }

    /// <summary>
    /// Vergi dairesi Id (TurkishTaxOffice FK)
    /// </summary>
    public int? VergiDairesiId { get; set; }

    /// <summary>
    /// MERSİS No (16 hane, kurumsal)
    /// </summary>
    public string? Mersis { get; set; }

    /// <summary>
    /// Ticaret sicil numarası (kurumsal)
    /// </summary>
    public string? TicaretSicilNo { get; set; }

    /// <summary>
    /// KEP (Kayıtlı Elektronik Posta) adresi — kurumsal müşteriler için
    /// </summary>
    public string? KepAdresi { get; set; }

    /// <summary>
    /// Müşteri tipi: bireysel / kurumsal
    /// </summary>
    public TurkishCustomerType MusteriTipi { get; set; } = TurkishCustomerType.Individual;

    /// <summary>
    /// E-fatura mükellefi mi (GİB sorgusundan, null = henüz sorgulanmadı)
    /// </summary>
    public bool? IsEFaturaMukellefi { get; set; }

    /// <summary>
    /// GİB e-fatura mükellef sorgusu son kontrol zamanı (UTC)
    /// </summary>
    public DateTime? EFaturaMukellefiKontrolTarihiUtc { get; set; }
}
