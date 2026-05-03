using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Location;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Location;

/// <summary>
/// <see cref="TurkishLocationService"/> birim testleri.
/// IRepository ve IStaticCacheManager mock'lanır, servisin
/// kontrol akışı (input validation, fallback, cache key seçimi) doğrulanır.
/// LINQ filter mantığı veritabanı katmanında olduğu için mock cevap olduğu gibi döner.
/// </summary>
[TestFixture]
public class TurkishLocationServiceTests
{
    private Mock<IRepository<TurkishProvince>> _provinceRepo = null!;
    private Mock<IRepository<TurkishDistrict>> _districtRepo = null!;
    private Mock<IRepository<TurkishNeighborhood>> _neighborhoodRepo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private TurkishLocationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _provinceRepo = new Mock<IRepository<TurkishProvince>>();
        _districtRepo = new Mock<IRepository<TurkishDistrict>>();
        _neighborhoodRepo = new Mock<IRepository<TurkishNeighborhood>>();
        _cache = new Mock<IStaticCacheManager>();

        // PrepareKeyForDefaultCache, parametreli CacheKey üretimini mock'lar
        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key.Create(o => o, args));

        _sut = new TurkishLocationService(
            _provinceRepo.Object,
            _districtRepo.Object,
            _neighborhoodRepo.Object,
            _cache.Object);
    }

    #region İl

    [Test]
    public async Task GetAllProvincesAsync_ReturnsRepositoryResult()
    {
        var data = new List<TurkishProvince>
        {
            new() { Id = 34, Name = "İstanbul", PlateCode = 34 },
            new() { Id = 6, Name = "Ankara", PlateCode = 6 }
        };
        _provinceRepo
            .Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishProvince>, IQueryable<TurkishProvince>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(data);

        var result = await _sut.GetAllProvincesAsync();

        result.Should().BeEquivalentTo(data);
    }

    [TestCase(0)]
    [TestCase(-1)]
    [TestCase(-100)]
    public async Task GetProvinceByIdAsync_NonPositiveId_ReturnsNullWithoutQuery(int id)
    {
        var result = await _sut.GetProvinceByIdAsync(id);

        result.Should().BeNull();
        _provinceRepo.Verify(r => r.GetByIdAsync(It.IsAny<int?>(), It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()),
            Times.Never);
    }

    [Test]
    public async Task GetProvinceByIdAsync_ValidId_DelegatesToRepository()
    {
        var province = new TurkishProvince { Id = 34, Name = "İstanbul", PlateCode = 34 };
        _provinceRepo.Setup(r => r.GetByIdAsync(34, It.IsAny<Func<ICacheKeyService, CacheKey>>(), It.IsAny<bool>(), It.IsAny<bool>()))
                     .ReturnsAsync(province);

        var result = await _sut.GetProvinceByIdAsync(34);

        result.Should().Be(province);
    }

    [TestCase(0)]
    [TestCase(82)]    // 81 maksimum
    [TestCase(-5)]
    [TestCase(100)]
    public async Task GetProvinceByPlateCodeAsync_OutOfRange_ReturnsNull(int plateCode)
    {
        var result = await _sut.GetProvinceByPlateCodeAsync(plateCode);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetProvinceByPlateCodeAsync_ValidCode_FindsInCachedList()
    {
        var data = new List<TurkishProvince>
        {
            new() { Id = 1, Name = "Adana", PlateCode = 1 },
            new() { Id = 34, Name = "İstanbul", PlateCode = 34 },
            new() { Id = 81, Name = "Düzce", PlateCode = 81 }
        };
        _provinceRepo
            .Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishProvince>, IQueryable<TurkishProvince>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(data);

        var result = await _sut.GetProvinceByPlateCodeAsync(34);

        result.Should().NotBeNull();
        result!.Name.Should().Be("İstanbul");
    }

    [Test]
    public async Task GetProvinceByPlateCodeAsync_NotFound_ReturnsNull()
    {
        _provinceRepo
            .Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishProvince>, IQueryable<TurkishProvince>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<TurkishProvince>());

        var result = await _sut.GetProvinceByPlateCodeAsync(34);

        result.Should().BeNull();
    }

    #endregion

    #region İlçe

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetDistrictsByProvinceIdAsync_InvalidId_ReturnsEmptyWithoutQuery(int provinceId)
    {
        var result = await _sut.GetDistrictsByProvinceIdAsync(provinceId);

        result.Should().BeEmpty();
        _districtRepo.Verify(r => r.GetAllAsync(
            It.IsAny<Func<IQueryable<TurkishDistrict>, IQueryable<TurkishDistrict>>>(),
            It.IsAny<Func<ICacheKeyService, CacheKey>>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetDistrictsByProvinceIdAsync_ValidId_UsesParameterizedCacheKey()
    {
        var data = new List<TurkishDistrict>
        {
            new() { Id = 100, ProvinceId = 34, Name = "Beyoğlu" },
            new() { Id = 101, ProvinceId = 34, Name = "Kadıköy" }
        };
        _districtRepo
            .Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishDistrict>, IQueryable<TurkishDistrict>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(data);

        var result = await _sut.GetDistrictsByProvinceIdAsync(34);

        result.Should().HaveCount(2);
        // Cache key 34 ile parameterize edilmeli
        _cache.Verify(c => c.PrepareKeyForDefaultCache(
            It.Is<CacheKey>(k => k.Key.Contains("Districts.ByProvince")),
            It.Is<object[]>(p => p.Length == 1 && (int)p[0] == 34)), Times.Once);
    }

    #endregion

    #region Mahalle

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task GetNeighborhoodsByPostalCodeAsync_NullOrEmpty_ReturnsEmpty(string? postalCode)
    {
        var result = await _sut.GetNeighborhoodsByPostalCodeAsync(postalCode!);

        result.Should().BeEmpty();
        _neighborhoodRepo.Verify(r => r.GetAllAsync(
            It.IsAny<Func<IQueryable<TurkishNeighborhood>, IQueryable<TurkishNeighborhood>>>(),
            It.IsAny<Func<ICacheKeyService, CacheKey>>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetNeighborhoodsByPostalCodeAsync_TrimsInput()
    {
        _neighborhoodRepo
            .Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishNeighborhood>, IQueryable<TurkishNeighborhood>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
            .ReturnsAsync(new List<TurkishNeighborhood>());

        await _sut.GetNeighborhoodsByPostalCodeAsync("  34000  ");

        // Cache key trimlenmiş haliyle "34000" parametresi almalı
        _cache.Verify(c => c.PrepareKeyForDefaultCache(
            It.IsAny<CacheKey>(),
            It.Is<object[]>(p => p.Length == 1 && (string)p[0] == "34000")), Times.Once);
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetNeighborhoodsByDistrictIdAsync_InvalidId_ReturnsEmpty(int districtId)
    {
        var result = await _sut.GetNeighborhoodsByDistrictIdAsync(districtId);
        result.Should().BeEmpty();
    }

    #endregion
}
