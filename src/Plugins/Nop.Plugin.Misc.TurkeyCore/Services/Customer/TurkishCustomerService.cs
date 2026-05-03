using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Services.Security;
using NopCustomer = Nop.Core.Domain.Customers.Customer;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Customer;

/// <inheritdoc />
public class TurkishCustomerService : ITurkishCustomerService
{
    #region Fields

    protected readonly IRepository<TurkishCustomerExtension> _extensionRepository;
    protected readonly IEncryptionService _encryptionService;

    #endregion

    #region Ctor

    public TurkishCustomerService(
        IRepository<TurkishCustomerExtension> extensionRepository,
        IEncryptionService encryptionService)
    {
        _extensionRepository = extensionRepository;
        _encryptionService = encryptionService;
    }

    #endregion

    #region Extension lookup

    /// <inheritdoc />
    public virtual async Task<TurkishCustomerExtension?> GetExtensionAsync(NopCustomer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        if (customer.Id <= 0)
            return null;

        // GetAllAsync callback pattern — TurkishLocationService ile tutarlı, test'te mock kolay
        var list = await _extensionRepository.GetAllAsync(
            query => query.Where(e => e.CustomerId == customer.Id),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual async Task<TurkishCustomerExtension> GetOrCreateExtensionAsync(NopCustomer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        var existing = await GetExtensionAsync(customer);
        if (existing is not null)
            return existing;

        var fresh = new TurkishCustomerExtension
        {
            CustomerId = customer.Id,
            MusteriTipi = TurkishCustomerType.Individual
        };
        await _extensionRepository.InsertAsync(fresh);
        return fresh;
    }

    /// <inheritdoc />
    public virtual async Task UpsertExtensionAsync(TurkishCustomerExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        if (extension.CustomerId <= 0)
            throw new ArgumentException("CustomerId belirlenmiş olmalı", nameof(extension));

        if (extension.Id == 0)
            await _extensionRepository.InsertAsync(extension);
        else
            await _extensionRepository.UpdateAsync(extension);
    }

    #endregion

    #region TCKN / VKN encrypt-decrypt

    /// <inheritdoc />
    public virtual async Task SetTcKimlikNoAsync(NopCustomer customer, string? tckn)
    {
        var extension = await GetOrCreateExtensionAsync(customer);
        extension.TcKimlikNo = string.IsNullOrWhiteSpace(tckn)
            ? null
            : _encryptionService.EncryptText(tckn.Trim());
        await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task<string?> GetTcKimlikNoAsync(NopCustomer customer)
    {
        var extension = await GetExtensionAsync(customer);
        if (extension?.TcKimlikNo is null)
            return null;

        return _encryptionService.DecryptText(extension.TcKimlikNo);
    }

    /// <inheritdoc />
    public virtual async Task SetVergiNoAsync(NopCustomer customer, string? vkn)
    {
        var extension = await GetOrCreateExtensionAsync(customer);
        extension.VergiNo = string.IsNullOrWhiteSpace(vkn)
            ? null
            : _encryptionService.EncryptText(vkn.Trim());
        await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task<string?> GetVergiNoAsync(NopCustomer customer)
    {
        var extension = await GetExtensionAsync(customer);
        if (extension?.VergiNo is null)
            return null;

        return _encryptionService.DecryptText(extension.VergiNo);
    }

    #endregion

    #region Müşteri tipi sorgular

    /// <inheritdoc />
    public virtual async Task<bool> IsCorporateAsync(NopCustomer customer)
    {
        var extension = await GetExtensionAsync(customer);
        return extension?.MusteriTipi == TurkishCustomerType.Corporate;
    }

    /// <inheritdoc />
    public virtual async Task<bool?> IsEFaturaMukellefiAsync(NopCustomer customer)
    {
        var extension = await GetExtensionAsync(customer);
        return extension?.IsEFaturaMukellefi;
    }

    #endregion
}
