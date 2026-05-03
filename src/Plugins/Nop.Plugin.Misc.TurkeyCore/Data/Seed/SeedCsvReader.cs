using System.Reflection;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Seed;

/// <summary>
/// Plugin assembly'sine embedded resource olarak gömülü CSV dosyalarını okur.
///
/// CSV format kuralları:
/// - UTF-8 BOM kabul edilir (otomatik atılır)
/// - Pipe (<c>|</c>) delimiter kullanılır — Türkçe il/ilçe adlarındaki virgül karışıklığını önler
/// - İlk satır başlık (header), atılır
/// - <c>#</c> ile başlayan satırlar yorumdur, atılır
/// - Boş satırlar atılır
/// </summary>
public static class SeedCsvReader
{
    private const char Delimiter = '|';

    /// <summary>
    /// Embedded resource'tan CSV satırlarını okur. Her satır parçalanmış string[] olarak döner.
    /// </summary>
    /// <param name="resourceName">
    /// Dosya adı, örn. <c>provinces.csv</c>. Plugin assembly'sindeki tam yol
    /// (<c>Nop.Plugin.Misc.TurkeyCore.Data.Seed.provinces.csv</c>) otomatik üretilir.
    /// </param>
    /// <exception cref="InvalidOperationException">Resource bulunamazsa.</exception>
    public static IList<string[]> Read(string resourceName)
    {
        if (string.IsNullOrWhiteSpace(resourceName))
            throw new ArgumentException("resourceName boş olamaz", nameof(resourceName));

        var assembly = typeof(SeedCsvReader).Assembly;
        var fullName = $"Nop.Plugin.Misc.TurkeyCore.Data.Seed.{resourceName}";

        using var stream = assembly.GetManifestResourceStream(fullName)
            ?? throw new InvalidOperationException(
                $"Embedded resource bulunamadı: {fullName}. " +
                $"csproj <EmbeddedResource> ayarını ve dosya adını kontrol edin.");

        using var reader = new StreamReader(stream);
        return ParseLines(reader);
    }

    /// <summary>
    /// Reader'dan CSV satırlarını parse eder. Test edilebilirlik için ayrı.
    /// </summary>
    public static IList<string[]> ParseLines(TextReader reader)
    {
        var rows = new List<string[]>();
        var isFirstLine = true;

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            // BOM temizliği (sadece ilk satır)
            if (isFirstLine && line.Length > 0 && line[0] == '﻿')
                line = line[1..];
            isFirstLine = false;

            // Boş ve yorum satırları atla
            if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                continue;

            // Header satırını atla — ilk geçerli satır header kabul edilir
            if (rows.Count == 0 && IsHeaderRow(line))
                continue;

            var fields = line.Split(Delimiter);
            for (var i = 0; i < fields.Length; i++)
                fields[i] = fields[i].Trim();

            rows.Add(fields);
        }

        return rows;
    }

    /// <summary>
    /// Header satırı tespiti — tüm hücreler harf/alt-çizgi içeriyorsa header sayılır.
    /// </summary>
    private static bool IsHeaderRow(string line)
    {
        // Header'da rakam genelde olmaz; "Plate|Name" gibi pure-alpha satır
        var fields = line.Split(Delimiter);
        return fields.All(f => !string.IsNullOrWhiteSpace(f)
                                && f.Trim().All(c => char.IsLetter(c) || c == '_'));
    }
}
