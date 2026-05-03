namespace Nop.Plugin.Misc.TurkeyCore.Services.GibLookup;

/// <summary>
/// GİB e-fatura mükellef sorgu servisi.
///
/// Resmi GİB endpoint'i public değildir; gerçek implementasyon özel entegratörler
/// (e-Logo, Mysoft, İzibiz) üzerinden yapılır ve EFatura plugin'i (Faz 2) tarafından sağlanır.
/// TurkeyCore'da default <see cref="MockGibMukellefService"/> bulunur — yapılandırılabilir
/// davranışla geliştirme/test ortamlarında çalışır.
/// </summary>
public interface IGibMukellefService
{
    /// <summary>
    /// VKN veya TCKN'nin e-fatura mükellefi olup olmadığını döner (cache'li).
    /// </summary>
    /// <param name="vknOrTckn">10 hane VKN veya 11 hane TCKN, normalize edilmiş.</param>
    /// <returns>true = mükellef (e-fatura), false = mükellef değil (e-arşiv), null = sorgu başarısız</returns>
    Task<bool?> IsEFaturaMukellefiAsync(string vknOrTckn);

    /// <summary>
    /// Mükellef detay bilgisi — unvan, vergi dairesi vb. (varsa).
    /// </summary>
    Task<MukellefInfo?> GetMukellefInfoAsync(string vknOrTckn);
}

/// <summary>
/// GİB mükellef detay projeksiyonu.
/// </summary>
public record MukellefInfo(
    string VknOrTckn,
    bool IsEFaturaMukellefi,
    string? Unvan = null,
    string? VergiDairesiAdi = null,
    DateTime? SorguTarihiUtc = null);
