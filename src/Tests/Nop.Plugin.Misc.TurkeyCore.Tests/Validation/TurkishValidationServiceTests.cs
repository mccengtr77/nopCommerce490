using FluentAssertions;
using Nop.Plugin.Misc.TurkeyCore.Services.Validation;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Validation;

/// <summary>
/// <see cref="TurkishValidationService"/> birim testleri.
/// Servis state'siz olduğu için her test kendi instance'ını oluşturur.
/// </summary>
[TestFixture]
public class TurkishValidationServiceTests
{
    private ITurkishValidationService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _sut = new TurkishValidationService();
    }

    #region TCKN — Geçerli

    /// <summary>
    /// Algoritma kuralına göre türetilmiş geçerli TCKN'ler.
    /// Her biri NVI checksum (10. ve 11. hane) kuralını sağlar.
    /// </summary>
    [TestCase("12345678950")]   // d[0..8]=123456789, d10=5, d11=0
    [TestCase("98765432150")]   // d[0..8]=987654321, d10=5, d11=0
    [TestCase("29384756150")]   // d[0..8]=293847561
    [TestCase("11111111288")]   // ilk 10 farklı (sadece son ikisi farklı yeterli)
    [TestCase("19111111130")]
    [TestCase("11111111974")]
    [TestCase("19090909018")]   // negatif modulo edge case (oddSum*7 < evenSum)
    public void ValidateTcKimlikNo_ValidNumbers_ReturnsTrue(string tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeTrue($"TCKN '{tckn}' algoritmaya göre geçerli");
    }

    [Test]
    public void ValidateTcKimlikNo_AcceptsLeadingTrailingWhitespace()
    {
        _sut.ValidateTcKimlikNo("  12345678950  ").Should().BeTrue();
    }

    #endregion

    #region TCKN — Geçersiz

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void ValidateTcKimlikNo_NullOrEmpty_ReturnsFalse(string? tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeFalse();
    }

    [TestCase("1234567890")]      // 10 hane
    [TestCase("123456789012")]    // 12 hane
    [TestCase("1")]
    public void ValidateTcKimlikNo_WrongLength_ReturnsFalse(string tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeFalse();
    }

    [TestCase("1234567890a")]     // alfabetik karakter
    [TestCase("12345 78950")]     // ortada boşluk
    [TestCase("12345-78950")]     // tire
    [TestCase("12345.78950")]     // nokta
    public void ValidateTcKimlikNo_NonDigitCharacters_ReturnsFalse(string tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeFalse();
    }

    [Test]
    public void ValidateTcKimlikNo_StartsWithZero_ReturnsFalse()
    {
        // 11 hane, ama ilk hane 0 → reddedilmeli
        _sut.ValidateTcKimlikNo("01234567890").Should().BeFalse();
    }

    [TestCase("00000000000")]     // tüm sıfır + ilk hane sıfır
    [TestCase("11111111110")]     // tüm 1, 11. hane 0 — algoritma teknik olarak geçer ama gerçekte verilmez
    [TestCase("22222222220")]
    [TestCase("99999999990")]
    public void ValidateTcKimlikNo_AllSameDigits_ReturnsFalse(string tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeFalse();
    }

    [TestCase("12345678951")]     // d11 yanlış (gerçek: 0)
    [TestCase("12345678960")]     // d10 yanlış (gerçek: 5)
    [TestCase("12345678999")]     // hem d10 hem d11 yanlış
    [TestCase("98765432151")]     // d11 yanlış
    public void ValidateTcKimlikNo_WrongChecksum_ReturnsFalse(string tckn)
    {
        _sut.ValidateTcKimlikNo(tckn).Should().BeFalse();
    }

    #endregion

    #region VKN — Geçerli

    /// <summary>
    /// GİB algoritmasına göre türetilmiş geçerli VKN'ler.
    /// </summary>
    [TestCase("1234567890")]      // klasik test VKN — toplam 0, checksum 0
    [TestCase("9999999994")]      // sınır: tüm 9'lar
    [TestCase("1000000000")]      // 1 + 8 sıfır + checksum 0
    [TestCase("2938475611")]
    public void ValidateVergiNo_ValidNumbers_ReturnsTrue(string vkn)
    {
        _sut.ValidateVergiNo(vkn).Should().BeTrue($"VKN '{vkn}' algoritmaya göre geçerli");
    }

    #endregion

    #region VKN — Geçersiz

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public void ValidateVergiNo_NullOrEmpty_ReturnsFalse(string? vkn)
    {
        _sut.ValidateVergiNo(vkn).Should().BeFalse();
    }

    [TestCase("123456789")]       // 9 hane
    [TestCase("12345678901")]     // 11 hane
    [TestCase("1")]
    public void ValidateVergiNo_WrongLength_ReturnsFalse(string vkn)
    {
        _sut.ValidateVergiNo(vkn).Should().BeFalse();
    }

    [TestCase("123456789a")]
    [TestCase("12345 7890")]
    public void ValidateVergiNo_NonDigitCharacters_ReturnsFalse(string vkn)
    {
        _sut.ValidateVergiNo(vkn).Should().BeFalse();
    }

    [TestCase("1234567891")]      // off-by-one checksum (gerçek: 0)
    [TestCase("1234567892")]
    [TestCase("9999999990")]      // gerçek: 4
    public void ValidateVergiNo_WrongChecksum_ReturnsFalse(string vkn)
    {
        _sut.ValidateVergiNo(vkn).Should().BeFalse();
    }

    #endregion

    #region IBAN TR — Geçerli

    /// <summary>
    /// Türkiye için geçerli MOD-97 IBAN örnekleri.
    /// </summary>
    [TestCase("TR330006100519786457841326")]   // Wikipedia / Garanti BBVA örnek IBAN
    [TestCase("tr330006100519786457841326")]   // küçük harf
    [TestCase("TR33 0006 1005 1978 6457 8413 26")]  // standart 4'lü gruplama
    [TestCase("  TR330006100519786457841326  ")]    // baş/son boşluk
    public void ValidateIbanTr_ValidIbans_ReturnsTrue(string iban)
    {
        _sut.ValidateIbanTr(iban).Should().BeTrue();
    }

    #endregion

    #region IBAN TR — Geçersiz

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public void ValidateIbanTr_NullOrEmpty_ReturnsFalse(string? iban)
    {
        _sut.ValidateIbanTr(iban).Should().BeFalse();
    }

    [TestCase("TR3300061005197864578413")]        // 24 hane (eksik)
    [TestCase("TR330006100519786457841326XX")]    // 28 hane (fazla)
    public void ValidateIbanTr_WrongLength_ReturnsFalse(string iban)
    {
        _sut.ValidateIbanTr(iban).Should().BeFalse();
    }

    [TestCase("DE89370400440532013000")]          // Almanya IBAN
    [TestCase("GB82WEST12345698765432")]          // İngiltere IBAN
    [TestCase("AB330006100519786457841326")]      // hatalı ülke kodu (TR yerine AB)
    public void ValidateIbanTr_NonTurkishCountryCode_ReturnsFalse(string iban)
    {
        _sut.ValidateIbanTr(iban).Should().BeFalse();
    }

    [TestCase("TR330006100519786457841327")]      // son hane değişti → MOD-97 fail
    [TestCase("TR340006100519786457841326")]      // check digit değişti
    public void ValidateIbanTr_WrongChecksum_ReturnsFalse(string iban)
    {
        _sut.ValidateIbanTr(iban).Should().BeFalse();
    }

    [Test]
    public void ValidateIbanTr_InvalidCharacter_ReturnsFalse()
    {
        // '!' geçerli IBAN karakteri değil
        _sut.ValidateIbanTr("TR33000610051978645784132!").Should().BeFalse();
    }

    #endregion

    #region GSM — Validate

    [TestCase("5321234567")]            // 10 hane, 532 prefix (Turkcell)
    [TestCase("05321234567")]           // 11 hane, leading 0
    [TestCase("905321234567")]          // 12 hane, 90 prefix
    [TestCase("+905321234567")]         // uluslararası format
    [TestCase("0532 123 45 67")]        // boşluklu
    [TestCase("0532-123-45-67")]        // tireli
    [TestCase("(0532) 123 45 67")]      // parantezli
    public void ValidateGsmNumber_ValidFormats_ReturnsTrue(string gsm)
    {
        _sut.ValidateGsmNumber(gsm).Should().BeTrue();
    }

    [TestCase("5421234567")]   // Vodafone 542
    [TestCase("5051234567")]   // Türk Telekom 505
    [TestCase("5551234567")]   // Türk Telekom 555
    public void ValidateGsmNumber_AllOperators_ReturnsTrue(string gsm)
    {
        _sut.ValidateGsmNumber(gsm).Should().BeTrue();
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("5001234567")]   // 500 prefix yok
    [TestCase("4321234567")]   // 432 — sabit hat olarak GSM değil
    [TestCase("532123456")]    // 9 hane, eksik
    [TestCase("53212345678")]  // 11 hane (leading 0 yok)
    public void ValidateGsmNumber_Invalid_ReturnsFalse(string? gsm)
    {
        _sut.ValidateGsmNumber(gsm).Should().BeFalse();
    }

    #endregion

    #region GSM — Normalize

    [TestCase("5321234567",          "+90 532 123 45 67")]
    [TestCase("05321234567",         "+90 532 123 45 67")]
    [TestCase("+905321234567",       "+90 532 123 45 67")]
    [TestCase("905321234567",        "+90 532 123 45 67")]
    [TestCase("0532 123 45 67",      "+90 532 123 45 67")]
    [TestCase("0532-123-45-67",      "+90 532 123 45 67")]
    [TestCase("(0532) 123 45 67",    "+90 532 123 45 67")]
    public void NormalizeGsmNumber_VariousFormats_ReturnsCanonical(string input, string expected)
    {
        _sut.NormalizeGsmNumber(input).Should().Be(expected);
    }

    [TestCase("")]
    [TestCase(null)]
    [TestCase("5001234567")]
    [TestCase("notanumber")]
    public void NormalizeGsmNumber_Invalid_ReturnsNull(string? input)
    {
        _sut.NormalizeGsmNumber(input).Should().BeNull();
    }

    #endregion

    #region Türkçe karakter normalize

    [TestCase("İstanbul",   "Istanbul")]
    [TestCase("Çağlayan",   "Caglayan")]
    [TestCase("Şükrü",      "Sukru")]
    [TestCase("öğrenci",    "ogrenci")]
    [TestCase("ÇĞİÖŞÜ",     "CGIOSU")]
    [TestCase("çğıöşü",     "cgiosu")]
    [TestCase("Hello World", "Hello World")]                  // ASCII unchanged
    [TestCase("Türkçe Test 123", "Turkce Test 123")]          // mixed
    [TestCase("",           "")]
    public void NormalizeTurkishCharacters_VariousInputs_ReturnsAscii(string input, string expected)
    {
        _sut.NormalizeTurkishCharacters(input).Should().Be(expected);
    }

    [Test]
    public void NormalizeTurkishCharacters_Null_ReturnsEmpty()
    {
        _sut.NormalizeTurkishCharacters(null).Should().Be(string.Empty);
    }

    [Test]
    public void NormalizeTurkishCharacters_PreservesNonTurkishDiacritics()
    {
        // Almanca ä, ß gibi karakterler dokunulmamalı (TR-spesifik değil)
        _sut.NormalizeTurkishCharacters("Müller naïve").Should().Be("Muller naïve");
    }

    #endregion
}
