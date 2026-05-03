using FluentAssertions;
using Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Contracts;

/// <summary>
/// <see cref="ContractTokenReplacer"/> birim testleri. Pure function — instance gerekmez,
/// IO yok, deterministic.
/// </summary>
[TestFixture]
public class ContractTokenReplacerTests
{
    private ContractTokenReplacer _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new ContractTokenReplacer();
    }

    private static ContractTokenContext SampleContext() => new()
    {
        SellerName = "ACME A.Ş.",
        SellerAddress = "İstanbul",
        BuyerName = "Ahmet Yılmaz",
        BuyerTcKimlikNo = "12345678950",
        BuyerVergiNo = "",
        OrderNumber = "ORD-2026-001",
        ProductListHtml = "<table><tr><td>Ürün A</td></tr></table>",
        Subtotal = "100,00 ₺",
        GrandTotal = "118,00 ₺",
        WithdrawalDays = 14,
        MediationNotice = "Arabuluculuk şartı bulunmaktadır."
    };

    #region Basit token replace

    [Test]
    public void Replace_SingleToken_ReplacesValue()
    {
        var ctx = SampleContext();
        var result = _sut.Replace("Hoşgeldiniz {{AlıcıAdı}}!", ctx);
        result.Should().Be("Hoşgeldiniz Ahmet Yılmaz!");
    }

    [Test]
    public void Replace_MultipleTokens_AllReplaced()
    {
        var ctx = SampleContext();
        var result = _sut.Replace(
            "Sipariş {{SiparişNo}}, Toplam: {{GenelToplam}}, Cayma: {{CaymaSüresi}} gün",
            ctx);
        result.Should().Be("Sipariş ORD-2026-001, Toplam: 118,00 ₺, Cayma: 14 gün");
    }

    [Test]
    public void Replace_TurkishTokenName_HandledCorrectly()
    {
        // ÜrünListesi, Şirket, vb. Türkçe karakterli token adları
        var ctx = SampleContext();
        var result = _sut.Replace("{{ÜrünListesi}}", ctx);
        result.Should().Contain("Ürün A");
    }

    [Test]
    public void Replace_HtmlContent_PreservesHtmlTags()
    {
        var ctx = SampleContext();
        var template = "<p>Toplam: <strong>{{GenelToplam}}</strong></p>";
        var result = _sut.Replace(template, ctx);
        result.Should().Be("<p>Toplam: <strong>118,00 ₺</strong></p>");
    }

    #endregion

    #region Bilinmeyen tokenlar

    [Test]
    public void Replace_UnknownToken_LeftIntact()
    {
        // Admin kafadan attığı token: değiştirilmemeli (admin UI'da görür ve düzeltir)
        var ctx = SampleContext();
        var result = _sut.Replace("{{Bilinmeyen}} ve {{AlıcıAdı}}", ctx);
        result.Should().Be("{{Bilinmeyen}} ve Ahmet Yılmaz");
    }

    [Test]
    public void Replace_PartialBraces_NotReplaced()
    {
        // Sadece {AlıcıAdı} (tek brace) — token değil
        var ctx = SampleContext();
        var result = _sut.Replace("Tek brace: {AlıcıAdı}", ctx);
        result.Should().Be("Tek brace: {AlıcıAdı}");
    }

    [Test]
    public void Replace_EmptyToken_LeftIntact()
    {
        var ctx = SampleContext();
        var result = _sut.Replace("{{}} boş", ctx);
        // {{}} regex tarafından match edilmez (en az 1 karakter zorunlu)
        result.Should().Be("{{}} boş");
    }

    #endregion

    #region Edge cases

    [TestCase("")]
    [TestCase(null)]
    public void Replace_EmptyTemplate_ReturnsEmpty(string? template)
    {
        var ctx = SampleContext();
        var result = _sut.Replace(template!, ctx);
        result.Should().Be(string.Empty);
    }

    [Test]
    public void Replace_NullContext_Throws()
    {
        var act = () => _sut.Replace("template", null!);
        act.Should().Throw<ArgumentNullException>();
    }

    [Test]
    public void Replace_EmptyValue_LeftEmpty()
    {
        var ctx = SampleContext();
        ctx.BuyerVergiNo = string.Empty;  // boş değer
        var result = _sut.Replace("VKN: {{AlıcıVergiNo}}", ctx);
        result.Should().Be("VKN: ");
    }

    [Test]
    public void Replace_DefaultMediationNotice_NotEmpty()
    {
        // 2026 yönetmelik gereği default arabuluculuk metni boş olmamalı
        ContractTokenReplacer.DefaultMediationNotice.Should().NotBeNullOrEmpty();
        ContractTokenReplacer.DefaultMediationNotice.Should().Contain("arabulucu");
    }

    #endregion

    #region Token map

    [Test]
    public void BuildTokenMap_ContainsAllExpectedTokens()
    {
        var ctx = SampleContext();
        var map = ContractTokenReplacer.BuildTokenMap(ctx);

        // Spec'te listelenen tüm tokenlar map'te olmalı
        var expected = new[]
        {
            "SatıcıAdı", "SatıcıAdresi", "SatıcıTelefon", "SatıcıKep", "SatıcıMersis",
            "AlıcıAdı", "AlıcıAdresi", "AlıcıTelefon", "AlıcıTcKimlik", "AlıcıVergiNo",
            "SiparişNo", "SiparişTarihi", "ÜrünListesi",
            "AraToplam", "KdvToplam", "KargoBedeli", "İndirim", "GenelToplam",
            "TeslimatAdresi", "FaturaAdresi",
            "ÖdemeYöntemi", "KargoFirması", "TahminiTeslimSüresi",
            "CaymaSüresi", "ArabuluculukBilgisi"
        };

        foreach (var token in expected)
            map.Should().ContainKey(token, $"'{token}' token map'te tanımlı olmalı");
    }

    #endregion

    #region ComputeHash

    [Test]
    public void ComputeHash_DeterministicForSameInput()
    {
        var h1 = ContractGenerationService.ComputeHash("hello world");
        var h2 = ContractGenerationService.ComputeHash("hello world");

        h1.Should().Be(h2);
        h1.Should().HaveLength(64);
        h1.Should().MatchRegex("^[0-9a-f]{64}$");
    }

    [Test]
    public void ComputeHash_DifferentInput_DifferentHash()
    {
        var h1 = ContractGenerationService.ComputeHash("hello world");
        var h2 = ContractGenerationService.ComputeHash("hello world!");
        h1.Should().NotBe(h2);
    }

    [Test]
    public void ComputeHash_NullInput_ReturnsHashOfEmpty()
    {
        var h1 = ContractGenerationService.ComputeHash(null!);
        var h2 = ContractGenerationService.ComputeHash(string.Empty);
        h1.Should().Be(h2);
    }

    #endregion
}
