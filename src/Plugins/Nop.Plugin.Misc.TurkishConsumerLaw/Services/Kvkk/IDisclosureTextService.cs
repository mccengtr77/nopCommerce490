using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Kvkk;

/// <summary>
/// KVKK aydınlatma metni servisi.
/// </summary>
public interface IDisclosureTextService
{
    /// <summary>
    /// Belirtilen mağaza için aktif aydınlatma metnini döner.
    /// Multi-store fallback: önce mağazaya özel, yoksa global.
    /// </summary>
    Task<DisclosureText?> GetActiveAsync(int storeId);

    Task<DisclosureText?> GetByIdAsync(int id);
    Task<IList<DisclosureText>> GetAllAsync();

    Task InsertAsync(DisclosureText text);
    Task UpdateAsync(DisclosureText text);
    Task DeleteAsync(DisclosureText text);
}
