using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <summary>
/// Açık rıza metinleri servisi. Storefront'ta checkbox listesi için, admin'de CRUD için.
/// </summary>
public interface IExplicitConsentService
{
    /// <summary>
    /// Tüm aktif açık rıza metinlerini DisplayOrder sırasıyla getirir.
    /// </summary>
    Task<IList<ExplicitConsentText>> GetActiveAsync(int storeId);

    /// <summary>
    /// KVKK kapsamlı (Etk dışı) onayları döner — kayıt formunda "KVKK rızaları" bölümü için.
    /// </summary>
    Task<IList<ExplicitConsentText>> GetActiveKvkkAsync(int storeId);

    /// <summary>
    /// ETK (ticari ileti) onaylarını döner — kayıt formunda AYRI bölüm için.
    /// </summary>
    Task<IList<ExplicitConsentText>> GetActiveEtkAsync(int storeId);

    Task<ExplicitConsentText?> GetByIdAsync(int id);
    Task<ExplicitConsentText?> GetByScopeAsync(ConsentScope scope, int storeId);

    Task InsertAsync(ExplicitConsentText text);
    Task UpdateAsync(ExplicitConsentText text);
    Task DeleteAsync(ExplicitConsentText text);
}
