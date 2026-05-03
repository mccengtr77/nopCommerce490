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
