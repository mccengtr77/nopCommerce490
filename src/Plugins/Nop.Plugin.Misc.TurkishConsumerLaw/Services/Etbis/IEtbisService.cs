using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Etbis;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Etbis;

/// <summary>
/// ETBİS kayıt yönetim servisi. Genel tasarım: bir mağazada tek "aktif" kayıt olur,
/// admin yeni kayıt eklerken eskiyi pasifleştirme akışı UI'da yönetilir.
/// </summary>
public interface IEtbisService
{
    /// <summary>
    /// Belirtilen mağaza için aktif ETBİS kaydını döner (yoksa null).
    /// Multi-store fallback: önce mağazaya özel, yoksa storeId=0 kayıt aranır.
    /// Cache'lidir — admin yeni kayıt yaparken cache invalide edilir.
    /// </summary>
    Task<EtbisRegistration?> GetActiveAsync(int storeId);

    Task<EtbisRegistration?> GetByIdAsync(int id);

    Task<IList<EtbisRegistration>> GetAllAsync();

    Task InsertAsync(EtbisRegistration registration);
    Task UpdateAsync(EtbisRegistration registration);
    Task DeleteAsync(EtbisRegistration registration);
}
