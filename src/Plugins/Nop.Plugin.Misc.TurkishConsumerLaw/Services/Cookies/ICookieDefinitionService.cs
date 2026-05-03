using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Cookies;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Cookies;

/// <summary>
/// Site çerez kataloğu yönetimi. Banner'da kategori bazlı listeleme için kullanılır.
/// </summary>
public interface ICookieDefinitionService
{
    /// <summary>
    /// Tüm aktif çerez tanımlarını DisplayOrder + ad sırasıyla getirir (cache'li).
    /// </summary>
    Task<IList<CookieDefinition>> GetActiveAsync();

    /// <summary>
    /// Belirli bir kategorideki aktif çerez tanımları.
    /// </summary>
    Task<IList<CookieDefinition>> GetByCategoryAsync(CookieCategory category);

    Task<CookieDefinition?> GetByIdAsync(int id);
    Task InsertAsync(CookieDefinition definition);
    Task UpdateAsync(CookieDefinition definition);
    Task DeleteAsync(CookieDefinition definition);
}
