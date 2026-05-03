using Nop.Data;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <inheritdoc />
public class ContractTemplateService : IContractTemplateService
{
    protected readonly IRepository<ContractTemplate> _repository;

    public ContractTemplateService(IRepository<ContractTemplate> repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public virtual async Task<ContractTemplate?> GetActiveAsync(ContractTemplateType type, int storeId)
    {
        var allActive = await _repository.GetAllAsync(
            query => query.Where(t => t.IsActive && t.Type == type)
                          .OrderByDescending(t => t.LimitedToStoreId == storeId)
                          .ThenBy(t => t.DisplayOrder),
            getCacheKey: null);

        return allActive.FirstOrDefault(t => t.LimitedToStoreId == storeId)
            ?? allActive.FirstOrDefault(t => t.LimitedToStoreId == 0);
    }

    /// <inheritdoc />
    public virtual async Task<ContractTemplate?> GetByIdAsync(int id)
    {
        if (id <= 0) return null;
        return await _repository.GetByIdAsync(id, cache => default);
    }

    /// <inheritdoc />
    public virtual async Task<IList<ContractTemplate>> GetAllAsync()
    {
        return await _repository.GetAllAsync(
            query => query.OrderBy(t => t.Type).ThenBy(t => t.DisplayOrder),
            getCacheKey: null);
    }

    /// <inheritdoc />
    public virtual async Task InsertAsync(ContractTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        template.CreatedOnUtc = DateTime.UtcNow;
        await _repository.InsertAsync(template);
    }

    /// <inheritdoc />
    public virtual async Task UpdateAsync(ContractTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        template.UpdatedOnUtc = DateTime.UtcNow;
        await _repository.UpdateAsync(template);
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(ContractTemplate template)
    {
        ArgumentNullException.ThrowIfNull(template);
        await _repository.DeleteAsync(template);
    }
}
