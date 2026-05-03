using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Location;
using Nop.Services.Security;
using NopAddress = Nop.Core.Domain.Common.Address;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Address;

/// <inheritdoc />
public class TurkishAddressService : ITurkishAddressService
{
    #region Fields

    protected readonly IEncryptionService _encryptionService;
    protected readonly IRepository<TurkishAddressExtension> _extensionRepository;
    protected readonly ITurkishLocationService _locationService;

    #endregion

    #region Ctor

    public TurkishAddressService(
        IEncryptionService encryptionService,
        IRepository<TurkishAddressExtension> extensionRepository,
        ITurkishLocationService locationService)
    {
        _encryptionService = encryptionService;
        _extensionRepository = extensionRepository;
        _locationService = locationService;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<TurkishAddressExtension?> GetExtensionAsync(NopAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        if (address.Id <= 0)
            return null;

        return await GetByAddressIdAsync(address.Id);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishAddressExtension?> GetByAddressIdAsync(int addressId)
    {
        if (addressId <= 0)
            return null;

        var list = await _extensionRepository.GetAllAsync(
            query => query.Where(e => e.AddressId == addressId),
            getCacheKey: null);
        return list.FirstOrDefault();
    }

    /// <inheritdoc />
    public virtual async Task<TurkishAddressExtension> GetOrCreateExtensionAsync(NopAddress address)
    {
        ArgumentNullException.ThrowIfNull(address);

        var existing = await GetExtensionAsync(address);
        if (existing is not null)
            return existing;

        var fresh = new TurkishAddressExtension { AddressId = address.Id };
        await _extensionRepository.InsertAsync(fresh);
        return fresh;
    }

    /// <inheritdoc />
    public virtual async Task SetLocationAsync(NopAddress address,
        int? provinceId, int? districtId, int? neighborhoodId,
        string? binaNo = null, string? daireNo = null)
    {
        var extension = await GetOrCreateExtensionAsync(address);
        extension.ProvinceId = provinceId;
        extension.DistrictId = districtId;
        extension.NeighborhoodId = neighborhoodId;
        extension.BinaNo = binaNo;
        extension.DaireNo = daireNo;
        await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task SetInvoiceInfoAsync(NopAddress address,
        TurkishCustomerType musteriTipi,
        string? tcKimlikNo = null,
        string? vergiNo = null,
        int? vergiDairesiId = null)
    {
        var extension = await GetOrCreateExtensionAsync(address);
        extension.MusteriTipi = musteriTipi;

        if (musteriTipi == TurkishCustomerType.Individual)
        {
            extension.TcKimlikNo = string.IsNullOrWhiteSpace(tcKimlikNo)
                ? null
                : _encryptionService.EncryptText(tcKimlikNo);
            extension.VergiNo = null;
            extension.VergiDairesiId = null;
        }
        else
        {
            extension.TcKimlikNo = null;
            extension.VergiNo = string.IsNullOrWhiteSpace(vergiNo)
                ? null
                : _encryptionService.EncryptText(vergiNo);
            extension.VergiDairesiId = vergiDairesiId;
        }

        await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task SetAdresAdiAsync(NopAddress address, string? adresAdi)
    {
        var extension = await GetOrCreateExtensionAsync(address);
        extension.AdresAdi = string.IsNullOrWhiteSpace(adresAdi) ? null : adresAdi.Trim();
        await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task UpsertExtensionAsync(TurkishAddressExtension extension)
    {
        ArgumentNullException.ThrowIfNull(extension);
        if (extension.AddressId <= 0)
            throw new ArgumentException("AddressId belirlenmiş olmalı", nameof(extension));

        if (extension.Id == 0)
            await _extensionRepository.InsertAsync(extension);
        else
            await _extensionRepository.UpdateAsync(extension);
    }

    /// <inheritdoc />
    public virtual async Task<TurkishAddressDetailsModel?> GetDetailsAsync(NopAddress address)
    {
        var extension = await GetExtensionAsync(address);
        if (extension is null)
            return null;

        // Adlar location servisinden cache'li olarak gelir — performansı düşürmez
        var province = extension.ProvinceId.HasValue
            ? await _locationService.GetProvinceByIdAsync(extension.ProvinceId.Value)
            : null;
        var district = extension.DistrictId.HasValue
            ? await _locationService.GetDistrictByIdAsync(extension.DistrictId.Value)
            : null;
        var neighborhood = extension.NeighborhoodId.HasValue
            ? await _locationService.GetNeighborhoodByIdAsync(extension.NeighborhoodId.Value)
            : null;

        return new TurkishAddressDetailsModel(
            ProvinceName: province?.Name,
            DistrictName: district?.Name,
            NeighborhoodName: neighborhood?.Name,
            PostalCode: neighborhood?.PostalCode,
            BinaNo: extension.BinaNo,
            DaireNo: extension.DaireNo);
    }

    #endregion
}
