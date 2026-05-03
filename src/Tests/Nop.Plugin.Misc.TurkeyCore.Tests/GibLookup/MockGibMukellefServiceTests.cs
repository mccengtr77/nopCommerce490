using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Plugin.Misc.TurkeyCore.Services.GibLookup;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.GibLookup;

/// <summary>
/// <see cref="MockGibMukellefService"/> birim testleri.
/// </summary>
[TestFixture]
public class MockGibMukellefServiceTests
{
    private Mock<IStaticCacheManager> _cache = null!;

    [SetUp]
    public void SetUp()
    {
        _cache = new Mock<IStaticCacheManager>();

        _cache.Setup(c => c.PrepareKeyForDefaultCache(It.IsAny<CacheKey>(), It.IsAny<object[]>()))
              .Returns<CacheKey, object[]>((key, args) => key.Create(o => o, args));

        // Cache mock'u: factory'yi her seferinde çağır (cache miss simulasyonu)
        _cache.Setup(c => c.GetAsync(It.IsAny<CacheKey>(), It.IsAny<Func<Task<MukellefInfo?>>>()))
              .Returns<CacheKey, Func<Task<MukellefInfo?>>>((_, factory) => factory());
    }

    private MockGibMukellefService Create(
        IEnumerable<string>? always = null,
        IEnumerable<string>? never = null)
    {
        return new MockGibMukellefService(_cache.Object,
            always ?? Enumerable.Empty<string>(),
            never ?? Enumerable.Empty<string>());
    }

    #region IsEFaturaMukellefiAsync — fallback heuristic

    [Test]
    public async Task IsEFaturaMukellefi_TenDigitVkn_ReturnsTrue()
    {
        var sut = Create();

        var result = await sut.IsEFaturaMukellefiAsync("1234567890");

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsEFaturaMukellefi_ElevenDigitTckn_ReturnsFalse()
    {
        var sut = Create();

        var result = await sut.IsEFaturaMukellefiAsync("12345678950");

        result.Should().BeFalse();
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task IsEFaturaMukellefi_NullOrEmpty_ReturnsNull(string? input)
    {
        var sut = Create();

        var result = await sut.IsEFaturaMukellefiAsync(input!);

        result.Should().BeNull();
    }

    [Test]
    public async Task IsEFaturaMukellefi_InvalidLength_ReturnsNull()
    {
        var sut = Create();

        // 9 hane → ne VKN ne TCKN
        (await sut.IsEFaturaMukellefiAsync("123456789")).Should().BeNull();
        // 12 hane → ne VKN ne TCKN
        (await sut.IsEFaturaMukellefiAsync("123456789012")).Should().BeNull();
    }

    [Test]
    public async Task IsEFaturaMukellefi_NonDigit_ReturnsNull()
    {
        var sut = Create();

        var result = await sut.IsEFaturaMukellefiAsync("123456789a");

        result.Should().BeNull();
    }

    [Test]
    public async Task IsEFaturaMukellefi_TrimsInput()
    {
        var sut = Create();

        var result = await sut.IsEFaturaMukellefiAsync("  1234567890  ");

        result.Should().BeTrue();
    }

    #endregion

    #region Beyaz/kara liste override

    [Test]
    public async Task IsEFaturaMukellefi_AlwaysList_OverridesFalseHeuristic()
    {
        // 11 hane TCKN normalde false döner, ama beyaz listede → true
        var sut = Create(always: new[] { "12345678950" });

        var result = await sut.IsEFaturaMukellefiAsync("12345678950");

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsEFaturaMukellefi_NeverList_OverridesTrueHeuristic()
    {
        // 10 hane VKN normalde true döner, ama kara listede → false
        var sut = Create(never: new[] { "1234567890" });

        var result = await sut.IsEFaturaMukellefiAsync("1234567890");

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsEFaturaMukellefi_BothLists_AlwaysWins()
    {
        // Hem beyaz hem kara listede ise beyaz öncelikli (kod sırası)
        var sut = Create(
            always: new[] { "1234567890" },
            never: new[] { "1234567890" });

        var result = await sut.IsEFaturaMukellefiAsync("1234567890");

        result.Should().BeTrue();
    }

    #endregion

    #region GetMukellefInfoAsync

    [Test]
    public async Task GetMukellefInfo_ValidVkn_ReturnsInfoWithSorguTarihi()
    {
        var sut = Create();

        var info = await sut.GetMukellefInfoAsync("1234567890");

        info.Should().NotBeNull();
        info!.VknOrTckn.Should().Be("1234567890");
        info.IsEFaturaMukellefi.Should().BeTrue();
        info.SorguTarihiUtc.Should().NotBeNull();
        info.SorguTarihiUtc!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GetMukellefInfo_AlwaysList_ReturnsInfo()
    {
        var sut = Create(always: new[] { "5555555555" });

        var info = await sut.GetMukellefInfoAsync("5555555555");

        info.Should().NotBeNull();
        info!.IsEFaturaMukellefi.Should().BeTrue();
    }

    #endregion

    #region Cache davranışı

    [Test]
    public async Task GetMukellefInfo_UsesCache()
    {
        var sut = Create();

        await sut.GetMukellefInfoAsync("1234567890");

        // Cache'e parametreli key ile gidilmeli
        _cache.Verify(c => c.PrepareKeyForDefaultCache(
            It.Is<CacheKey>(k => k.Key.Contains("GibMukellef")),
            It.Is<object[]>(p => p.Length == 1 && (string)p[0] == "1234567890")), Times.Once);

        _cache.Verify(c => c.GetAsync(
            It.IsAny<CacheKey>(),
            It.IsAny<Func<Task<MukellefInfo?>>>()), Times.Once);
    }

    #endregion
}
