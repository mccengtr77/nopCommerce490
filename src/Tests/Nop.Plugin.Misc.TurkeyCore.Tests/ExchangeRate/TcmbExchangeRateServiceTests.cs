using System.Net;
using FluentAssertions;
using Moq;
using Moq.Protected;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using Nop.Services.Logging;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.ExchangeRate;

/// <summary>
/// <see cref="TcmbExchangeRateService"/> servis seviyesi testleri:
/// rate lookup, çapraz kur, cache invalidation, RefreshRatesAsync.
/// HttpClient, MockHttpMessageHandler ile sahte response döner.
/// </summary>
[TestFixture]
public class TcmbExchangeRateServiceTests
{
    private const string SampleFeedXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Tarih_Date Tarih="03.05.2026">
          <Currency Kod="USD" CurrencyCode="USD"><Unit>1</Unit>
            <ForexBuying>32.0</ForexBuying><ForexSelling>32.5</ForexSelling>
          </Currency>
          <Currency Kod="EUR" CurrencyCode="EUR"><Unit>1</Unit>
            <ForexBuying>34.0</ForexBuying><ForexSelling>34.5</ForexSelling>
          </Currency>
        </Tarih_Date>
        """;

    private Mock<IRepository<ExchangeRateLog>> _repo = null!;
    private Mock<IStaticCacheManager> _cache = null!;
    private Mock<ILogger> _logger = null!;
    private Mock<HttpMessageHandler> _httpHandler = null!;
    private TcmbExchangeRateService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<ExchangeRateLog>>();
        _cache = new Mock<IStaticCacheManager>();
        _logger = new Mock<ILogger>();
        _httpHandler = new Mock<HttpMessageHandler>();

        var httpClient = new HttpClient(_httpHandler.Object);

        _sut = new TcmbExchangeRateService(httpClient, _repo.Object, _cache.Object, _logger.Object);
    }

    /// <summary>
    /// IStaticCacheManager.GetAsync mock'u — hazır dictionary döner.
    /// </summary>
    private void SetupCacheReturns(IDictionary<string, decimal> rates)
    {
        _cache.Setup(c => c.GetAsync(
                It.IsAny<CacheKey>(),
                It.IsAny<Func<Task<IDictionary<string, decimal>>>>()))
              .ReturnsAsync(rates);
    }

    private void SetupHttpReturns(string content)
    {
        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(content)
            });
    }

    #region GetRateAsync

    [Test]
    public async Task GetRateAsync_Try_ReturnsOne()
    {
        var result = await _sut.GetRateAsync("TRY");
        result.Should().Be(1m);
    }

    [Test]
    public async Task GetRateAsync_TryLowercase_ReturnsOne()
    {
        var result = await _sut.GetRateAsync("try");
        result.Should().Be(1m);
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task GetRateAsync_NullOrEmpty_ReturnsNull(string? code)
    {
        var result = await _sut.GetRateAsync(code!);
        result.Should().BeNull();
    }

    [Test]
    public async Task GetRateAsync_CachedCurrency_ReturnsRate()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m,
            ["EUR"] = 34.5m
        });

        var result = await _sut.GetRateAsync("USD");
        result.Should().Be(32.5m);
    }

    [Test]
    public async Task GetRateAsync_LowercaseInput_FindsViaCaseInsensitive()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m
        });

        var result = await _sut.GetRateAsync("usd");
        result.Should().Be(32.5m);
    }

    [Test]
    public async Task GetRateAsync_UnknownCurrency_ReturnsNull()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m
        });

        var result = await _sut.GetRateAsync("XYZ");
        result.Should().BeNull();
    }

    #endregion

    #region GetCrossRateAsync

    [Test]
    public async Task GetCrossRateAsync_UsdToEur_ReturnsRatio()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m,
            ["EUR"] = 34.5m
        });

        var result = await _sut.GetCrossRateAsync("USD", "EUR");

        // 32.5 / 34.5 ≈ 0.94202898
        result.Should().NotBeNull();
        result!.Value.Should().BeApproximately(32.5m / 34.5m, 0.0001m);
    }

    [Test]
    public async Task GetCrossRateAsync_AnyToTry_ReturnsSourceRate()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m
        });

        var result = await _sut.GetCrossRateAsync("USD", "TRY");
        result.Should().Be(32.5m);
    }

    [Test]
    public async Task GetCrossRateAsync_TryToAny_ReturnsInverse()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m
        });

        var result = await _sut.GetCrossRateAsync("TRY", "USD");
        result.Should().BeApproximately(1m / 32.5m, 0.0001m);
    }

    [Test]
    public async Task GetCrossRateAsync_UnknownSource_ReturnsNull()
    {
        SetupCacheReturns(new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase)
        {
            ["USD"] = 32.5m
        });

        var result = await _sut.GetCrossRateAsync("XYZ", "USD");
        result.Should().BeNull();
    }

    #endregion

    #region RefreshRatesAsync

    [Test]
    public async Task RefreshRatesAsync_FetchesParsesAndInsertsRecords()
    {
        SetupHttpReturns(SampleFeedXml);

        var count = await _sut.RefreshRatesAsync();

        count.Should().Be(2);
        _repo.Verify(r => r.InsertAsync(
            It.Is<IList<ExchangeRateLog>>(list => list.Count == 2),
            It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task RefreshRatesAsync_InvalidatesCache()
    {
        SetupHttpReturns(SampleFeedXml);

        await _sut.RefreshRatesAsync();

        _cache.Verify(c => c.RemoveAsync(
            It.Is<CacheKey>(k => k.Key.Contains("ExchangeRates.Latest")),
            It.IsAny<object[]>()), Times.Once);
    }

    [Test]
    public async Task RefreshRatesAsync_HttpFailure_LogsAndThrows()
    {
        _httpHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("network down"));

        var act = () => _sut.RefreshRatesAsync();

        await act.Should().ThrowAsync<HttpRequestException>();
        _logger.Verify(l => l.ErrorAsync(
            It.Is<string>(s => s.Contains("TCMB feed")),
            It.IsAny<Exception>(),
            It.IsAny<Nop.Core.Domain.Customers.Customer>()), Times.Once);
    }

    [Test]
    public async Task RefreshRatesAsync_EmptyFeed_ReturnsZeroAndDoesNotInsert()
    {
        SetupHttpReturns("<Tarih_Date Tarih=\"03.05.2026\"></Tarih_Date>");

        var count = await _sut.RefreshRatesAsync();

        count.Should().Be(0);
        _repo.Verify(r => r.InsertAsync(It.IsAny<IList<ExchangeRateLog>>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion
}
