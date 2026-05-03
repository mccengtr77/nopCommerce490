using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <summary>
/// Sözleşme şablonu yönetim servisi (admin CRUD + storefront lookup).
/// </summary>
public interface IContractTemplateService
{
    /// <summary>
    /// Belirli tip için aktif şablonu döner. Multi-store fallback'li
    /// (önce mağazaya özel, yoksa global).
    /// </summary>
    Task<ContractTemplate?> GetActiveAsync(ContractTemplateType type, int storeId);

    Task<ContractTemplate?> GetByIdAsync(int id);

    Task<IList<ContractTemplate>> GetAllAsync();

    Task InsertAsync(ContractTemplate template);
    Task UpdateAsync(ContractTemplate template);
    Task DeleteAsync(ContractTemplate template);
}
