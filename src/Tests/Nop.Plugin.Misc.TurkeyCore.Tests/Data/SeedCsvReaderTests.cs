using FluentAssertions;
using Nop.Plugin.Misc.TurkeyCore.Data.Seed;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Data;

/// <summary>
/// <see cref="SeedCsvReader"/> birim testleri.
/// </summary>
[TestFixture]
public class SeedCsvReaderTests
{
    #region ParseLines

    [Test]
    public void ParseLines_BasicCsv_ParsesAllRows()
    {
        const string csv = "Plate|Name\n1|Adana\n2|Adıyaman\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(2);
        rows[0].Should().BeEquivalentTo(new[] { "1", "Adana" });
        rows[1].Should().BeEquivalentTo(new[] { "2", "Adıyaman" });
    }

    [Test]
    public void ParseLines_SkipsCommentLines()
    {
        const string csv = "Plate|Name\n# Bu yorum\n1|Adana\n# Başka yorum\n2|İstanbul\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(2);
        rows[1][1].Should().Be("İstanbul");
    }

    [Test]
    public void ParseLines_SkipsEmptyLines()
    {
        const string csv = "Plate|Name\n\n1|Adana\n   \n2|Ankara\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(2);
    }

    [Test]
    public void ParseLines_TrimsWhitespaceFromFields()
    {
        const string csv = "Plate|Name\n 1 |  Adana  \n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(1);
        rows[0][0].Should().Be("1");
        rows[0][1].Should().Be("Adana");
    }

    [Test]
    public void ParseLines_HandlesUtf8Bom()
    {
        const string csv = "﻿Plate|Name\n1|Adana\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(1);
        rows[0][0].Should().Be("1");
    }

    [Test]
    public void ParseLines_SkipsHeaderRow()
    {
        // Header pure-alpha (sadece harf + alt çizgi) — atılır
        const string csv = "ProvincePlate|Name\n1|Adana\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(1);
        rows[0][0].Should().Be("1");
    }

    [Test]
    public void ParseLines_HeaderlessCsv_AcceptsAllRows()
    {
        // İlk satırın hücresi rakam içeriyor → header değil, data olarak kabul edilir
        const string csv = "1|Adana\n2|Ankara\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(2);
    }

    [Test]
    public void ParseLines_TurkishCharactersInData()
    {
        const string csv = "Plate|Name\n4|Ağrı\n63|Şanlıurfa\n81|Düzce\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Select(r => r[1]).Should().BeEquivalentTo(new[] { "Ağrı", "Şanlıurfa", "Düzce" });
    }

    [Test]
    public void ParseLines_FourColumns_AllReturned()
    {
        // Mahalle CSV: ProvincePlate|DistrictName|NeighborhoodName|PostalCode
        const string csv = "P|D|N|Pc\n34|Beyoğlu|Cihangir|34433\n";

        using var reader = new StringReader(csv);
        var rows = SeedCsvReader.ParseLines(reader);

        rows.Should().HaveCount(1);
        rows[0].Should().HaveCount(4);
        rows[0][3].Should().Be("34433");
    }

    #endregion

    #region Read (embedded resource)

    [Test]
    public void Read_Provinces_ReturnsAll81Provinces()
    {
        var rows = SeedCsvReader.Read("provinces.csv");

        rows.Should().HaveCount(81, "T.C. resmi plaka kodu listesi 81 ildir");

        // Plaka kodları 1-81 aralığında
        var plates = rows.Select(r => int.Parse(r[0])).OrderBy(p => p).ToList();
        plates.Should().BeEquivalentTo(Enumerable.Range(1, 81));
    }

    [Test]
    public void Read_Provinces_ContainsKnownProvinces()
    {
        var rows = SeedCsvReader.Read("provinces.csv");
        var byPlate = rows.ToDictionary(r => int.Parse(r[0]), r => r[1]);

        byPlate[1].Should().Be("Adana");
        byPlate[6].Should().Be("Ankara");
        byPlate[34].Should().Be("İstanbul");
        byPlate[35].Should().Be("İzmir");
        byPlate[81].Should().Be("Düzce");
    }

    [Test]
    public void Read_Districts_HasIstanbul39Districts()
    {
        var rows = SeedCsvReader.Read("districts.csv");
        var istanbulDistricts = rows.Where(r => r[0] == "34").ToList();

        // İstanbul'un 39 ilçesi var
        istanbulDistricts.Should().HaveCount(39);
        istanbulDistricts.Select(r => r[1]).Should().Contain(new[]
        {
            "Beyoğlu", "Kadıköy", "Beşiktaş", "Şişli", "Üsküdar"
        });
    }

    [Test]
    public void Read_Neighborhoods_HasFourFields()
    {
        var rows = SeedCsvReader.Read("neighborhoods.csv");

        rows.Should().NotBeEmpty();
        rows.All(r => r.Length == 4).Should().BeTrue("Mahalle CSV'sinde 4 hücre olmalı: Plate|District|Name|PostalCode");

        // Tüm posta kodları 5 haneli
        rows.All(r => r[3].Length == 5 && r[3].All(char.IsDigit))
            .Should().BeTrue("Posta kodları 5 haneli olmalı");
    }

    [Test]
    public void Read_TaxOffices_HasIstanbulOffices()
    {
        var rows = SeedCsvReader.Read("taxoffices.csv");

        rows.Should().NotBeEmpty();
        rows.Where(r => r[0] == "34").Should().HaveCountGreaterThan(20,
            "İstanbul'un büyük şehir olarak ana vergi daireleri seed'de olmalı");
    }

    [Test]
    public void Read_NotFound_Throws()
    {
        var act = () => SeedCsvReader.Read("nonexistent-file.csv");
        act.Should().Throw<InvalidOperationException>()
           .WithMessage("*nonexistent-file.csv*");
    }

    [TestCase("")]
    [TestCase("   ")]
    [TestCase(null)]
    public void Read_InvalidName_Throws(string? name)
    {
        var act = () => SeedCsvReader.Read(name!);
        act.Should().Throw<ArgumentException>();
    }

    #endregion
}
