namespace Nop.Plugin.Misc.TurkeyCore.Services.Validation;

/// <summary>
/// Türkiye'ye özgü kimlik, vergi, banka ve iletişim numarası validasyonları.
/// Tüm metodlar deterministic — IO yapmaz, dolayısıyla async değildir.
/// </summary>
public interface ITurkishValidationService
{
    /// <summary>
    /// TC Kimlik No validasyonu (NVI algoritması).
    /// 11 hane, ilk hane 0 olamaz, 10. ve 11. haneler checksum.
    /// </summary>
    bool ValidateTcKimlikNo(string? tckn);

    /// <summary>
    /// Vergi Kimlik No validasyonu (GİB algoritması).
    /// 10 haneli kurumsal vergi numarası.
    /// </summary>
    bool ValidateVergiNo(string? vkn);

    /// <summary>
    /// Vergi numarası alanı için esnek validasyon: değer 10 hane ise tüzel kişi VKN olarak,
    /// 11 hane ise şahıs firması TCKN'si olarak doğrulanır. Türkiye'de şahıs firmalarının
    /// vergi numarası kişinin TC Kimlik No'su ile aynıdır (213 sayılı VUK uyarınca).
    /// </summary>
    /// <returns>true: geçerli VKN ya da geçerli TCKN. false: ikisi de değil ya da uzunluk hatalı.</returns>
    bool ValidateVergiOrTckn(string? value);

    /// <summary>
    /// IBAN TR validasyonu (MOD-97 + ülke kodu kontrolü).
    /// TR + 24 rakam = 26 karakter (boşluksuz).
    /// </summary>
    bool ValidateIbanTr(string? iban);

    /// <summary>
    /// GSM numarası validasyonu — Türkiye operatör prefix listesine göre.
    /// Girdi farklı formatlarda olabilir, normalize edilip kontrol edilir.
    /// </summary>
    bool ValidateGsmNumber(string? gsm);

    /// <summary>
    /// GSM numarasını "+90 5XX XXX XX XX" kanonik formatına çevirir.
    /// </summary>
    /// <returns>Normalize edilmiş numara veya null (geçersizse).</returns>
    string? NormalizeGsmNumber(string? gsm);

    /// <summary>
    /// Türkçe karakterleri ASCII karşılıklarına dönüştürür (İ→I, ğ→g vb.).
    /// Arama, slug üretme veya legacy sistem entegrasyonlarında kullanılır.
    /// </summary>
    string NormalizeTurkishCharacters(string? input);
}
