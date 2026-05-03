using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.TaxOffice;

/// <summary>
/// Türkiye vergi daireleri sorgu ve yönetim servisi.
/// Liste GİB tarafından yayınlanır, sistem seed'i + admin müdahalesi ile güncellenir.
/// </summary>
public interface ITurkishTaxOfficeService
{
    /// <summary>
    /// Tüm aktif vergi dairelerini getirir (cache'li).
    /// </summary>
    Task<IList<TurkishTaxOffice>> GetAllAsync();

    /// <summary>
    /// Bir ile bağlı tüm aktif vergi dairelerini getirir (cache'li).
    /// </summary>
    Task<IList<TurkishTaxOffice>> GetByProvinceIdAsync(int provinceId);

    /// <summary>
    /// Id ile vergi dairesi getirir.
    /// </summary>
    Task<TurkishTaxOffice?> GetByIdAsync(int taxOfficeId);

    /// <summary>
    /// GİB kodu ile vergi dairesi getirir.
    /// </summary>
    Task<TurkishTaxOffice?> GetByCodeAsync(string code);

    /// <summary>
    /// Yeni vergi dairesi ekler (admin).
    /// </summary>
    Task InsertAsync(TurkishTaxOffice taxOffice);

    /// <summary>
    /// Vergi dairesi günceller (admin).
    /// </summary>
    Task UpdateAsync(TurkishTaxOffice taxOffice);

    /// <summary>
    /// Vergi dairesi siler (admin).
    /// </summary>
    Task DeleteAsync(TurkishTaxOffice taxOffice);
}
