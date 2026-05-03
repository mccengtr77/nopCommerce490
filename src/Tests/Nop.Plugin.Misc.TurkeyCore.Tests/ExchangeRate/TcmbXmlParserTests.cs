using FluentAssertions;
using Nop.Plugin.Misc.TurkeyCore.Services.ExchangeRate;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.ExchangeRate;

/// <summary>
/// <see cref="TcmbExchangeRateService.ParseFeedXml"/> birim testleri.
/// XML parser saf fonksiyon olduğu için doğrudan çağrılır — instance/HttpClient gerekmez.
/// </summary>
[TestFixture]
public class TcmbXmlParserTests
{
    private const string SampleFeedXml = """
        <?xml version="1.0" encoding="UTF-8"?>
        <Tarih_Date Tarih="03.05.2026" Date="05/03/2026" Bulten_No="2026/85">
          <Currency CrossOrder="0" Kod="USD" CurrencyCode="USD">
            <Unit>1</Unit>
            <Isim>ABD DOLARI</Isim>
            <CurrencyName>US DOLLAR</CurrencyName>
            <ForexBuying>32.0345</ForexBuying>
            <ForexSelling>32.0987</ForexSelling>
            <BanknoteBuying>32.0123</BanknoteBuying>
            <BanknoteSelling>32.1456</BanknoteSelling>
          </Currency>
          <Currency CrossOrder="1" Kod="EUR" CurrencyCode="EUR">
            <Unit>1</Unit>
            <Isim>EURO</Isim>
            <CurrencyName>EURO</CurrencyName>
            <ForexBuying>34.5000</ForexBuying>
            <ForexSelling>34.5500</ForexSelling>
            <BanknoteBuying>34.4800</BanknoteBuying>
            <BanknoteSelling>34.6000</BanknoteSelling>
          </Currency>
          <Currency CrossOrder="9" Kod="JPY" CurrencyCode="JPY">
            <Unit>100</Unit>
            <Isim>JAPON YENİ</Isim>
            <CurrencyName>JAPENESE YEN</CurrencyName>
            <ForexBuying>21.5000</ForexBuying>
            <ForexSelling>21.6000</ForexSelling>
            <BanknoteBuying>21.4800</BanknoteBuying>
            <BanknoteSelling>21.6200</BanknoteSelling>
          </Currency>
          <Currency CrossOrder="11" Kod="XDR" CurrencyCode="XDR">
            <Unit>1</Unit>
            <Isim>SDR (Özel Çekme Hakkı)</Isim>
            <CurrencyName>SDR (Special Drawing Right)</CurrencyName>
            <ForexBuying></ForexBuying>
            <ForexSelling></ForexSelling>
            <BanknoteBuying></BanknoteBuying>
            <BanknoteSelling></BanknoteSelling>
          </Currency>
        </Tarih_Date>
        """;

    [Test]
    public void ParseFeedXml_SampleFeed_ReturnsExpectedRecordCount()
    {
        // XDR Forex'siz olduğu için atlanır → 3 kayıt
        var result = TcmbExchangeRateService.ParseFeedXml(SampleFeedXml);

        result.Should().HaveCount(3);
        result.Select(r => r.CurrencyCode).Should().BeEquivalentTo("USD", "EUR", "JPY");
    }

    [Test]
    public void ParseFeedXml_UsdRecord_HasCorrectValues()
    {
        var result = TcmbExchangeRateService.ParseFeedXml(SampleFeedXml);

        var usd = result.Single(r => r.CurrencyCode == "USD");
        usd.ForexBuying.Should().Be(32.0345m);
        usd.ForexSelling.Should().Be(32.0987m);
        usd.BanknoteBuying.Should().Be(32.0123m);
        usd.BanknoteSelling.Should().Be(32.1456m);
        usd.RateDateUtc.Should().Be(new DateTime(2026, 5, 3, 0, 0, 0, DateTimeKind.Utc));
    }

    [Test]
    public void ParseFeedXml_JpyUnit100_NormalizesToPerUnit()
    {
        // JPY Unit=100, ForexSelling=21.6 → birim başına 0.216
        var result = TcmbExchangeRateService.ParseFeedXml(SampleFeedXml);

        var jpy = result.Single(r => r.CurrencyCode == "JPY");
        jpy.ForexSelling.Should().Be(0.216m);
        jpy.ForexBuying.Should().Be(0.215m);
        jpy.BanknoteBuying.Should().Be(0.2148m);
        jpy.BanknoteSelling.Should().Be(0.2162m);
    }

    [Test]
    public void ParseFeedXml_EmptyForexFields_SkipsRecord()
    {
        // XDR'de ForexBuying/Selling boş — kayıt atlanmalı
        var result = TcmbExchangeRateService.ParseFeedXml(SampleFeedXml);

        result.Should().NotContain(r => r.CurrencyCode == "XDR");
    }

    [Test]
    public void ParseFeedXml_EmptyString_ReturnsEmptyList()
    {
        TcmbExchangeRateService.ParseFeedXml("").Should().BeEmpty();
        TcmbExchangeRateService.ParseFeedXml("   ").Should().BeEmpty();
    }

    [Test]
    public void ParseFeedXml_WrongRoot_Throws()
    {
        var act = () => TcmbExchangeRateService.ParseFeedXml("<root/>");
        act.Should().Throw<InvalidOperationException>().WithMessage("*Tarih_Date*");
    }

    [Test]
    public void ParseFeedXml_InvalidXml_Throws()
    {
        var act = () => TcmbExchangeRateService.ParseFeedXml("<not closed");
        act.Should().Throw<System.Xml.XmlException>();
    }

    [Test]
    public void ParseFeedXml_MissingDateAttribute_FallsBackToToday()
    {
        const string xml = """
            <Tarih_Date>
              <Currency CurrencyCode="USD">
                <Unit>1</Unit>
                <ForexBuying>32.0</ForexBuying>
                <ForexSelling>32.5</ForexSelling>
              </Currency>
            </Tarih_Date>
            """;

        var result = TcmbExchangeRateService.ParseFeedXml(xml);

        result.Should().HaveCount(1);
        result[0].RateDateUtc.Date.Should().Be(DateTime.UtcNow.Date);
    }

    [Test]
    public void ParseFeedXml_FallsBackToKodAttribute_WhenCurrencyCodeMissing()
    {
        const string xml = """
            <Tarih_Date Tarih="03.05.2026">
              <Currency Kod="GBP">
                <Unit>1</Unit>
                <ForexBuying>40.0</ForexBuying>
                <ForexSelling>40.5</ForexSelling>
              </Currency>
            </Tarih_Date>
            """;

        var result = TcmbExchangeRateService.ParseFeedXml(xml);

        result.Should().HaveCount(1);
        result[0].CurrencyCode.Should().Be("GBP");
    }
}
