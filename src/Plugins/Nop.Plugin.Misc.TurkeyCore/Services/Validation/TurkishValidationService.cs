using System.Numerics;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Validation;

/// <summary>
/// <see cref="ITurkishValidationService"/> implementasyonu.
/// Algoritmalar resmi kaynaklara göre uygulanmıştır:
/// - TCKN: NVI / Nüfus ve Vatandaşlık İşleri Genel Müdürlüğü
/// - VKN: GİB (Gelir İdaresi Başkanlığı) standartı
/// - IBAN: ISO 13616 / MOD-97-10
/// </summary>
public class TurkishValidationService : ITurkishValidationService
{
    #region Constants

    // Aktif Türkiye GSM operatör prefix'leri (530-559 aralığı, 5XX serisi)
    // Kaynak: BTK numaralandırma planı. Liste güncel tutulmalı.
    private static readonly HashSet<string> _gsmPrefixes = new()
    {
        "501","505","506","507","551","552","553","554","555","559",                     // Türk Telekom (Avea kökenli) + Türkcell
        "530","531","532","533","534","535","536","537","538","539",                     // Turkcell
        "540","541","542","543","544","545","546","547","548","549",                     // Vodafone
        "561"                                                                            // BiP/sanal
    };

    // Türkçe-spesifik karakterleri ASCII karşılıklarına eşleştirir.
    // Latin1'de zaten ASCII olan karakterler (I, i) burada yok — değişmemeleri gerek.
    // 'İ' (U+0130) → 'I', 'ı' (U+0131) → 'i' Türkçe dotted-I özelliği.
    private static readonly Dictionary<char, char> _turkishCharMap = new()
    {
        ['ç'] = 'c', ['Ç'] = 'C',
        ['ğ'] = 'g', ['Ğ'] = 'G',
        ['ı'] = 'i', ['İ'] = 'I',
        ['ö'] = 'o', ['Ö'] = 'O',
        ['ş'] = 's', ['Ş'] = 'S',
        ['ü'] = 'u', ['Ü'] = 'U'
    };

    #endregion

    #region TCKN

    /// <inheritdoc />
    public bool ValidateTcKimlikNo(string? tckn)
    {
        if (string.IsNullOrWhiteSpace(tckn))
            return false;

        var digits = tckn.Trim();

        if (digits.Length != TurkeyCoreDefaults.Validation.TcKimlikNoLength)
            return false;

        if (!digits.All(char.IsDigit))
            return false;

        // İlk hane 0 olamaz
        if (digits[0] == '0')
            return false;

        // Tüm haneler aynı (11111111110 vb.) — algoritma teknik olarak geçer ama gerçekte geçersiz
        if (digits.Take(10).Distinct().Count() == 1)
            return false;

        var d = digits.Select(c => c - '0').ToArray();

        // 10. hane: ((1+3+5+7+9. hanelerin toplamı × 7) - (2+4+6+8. hanelerin toplamı)) mod 10
        var oddSum = d[0] + d[2] + d[4] + d[6] + d[8];
        var evenSum = d[1] + d[3] + d[5] + d[7];
        var digit10 = ((oddSum * 7) - evenSum) % 10;
        if (digit10 < 0)
            digit10 += 10;

        if (d[9] != digit10)
            return false;

        // 11. hane: ilk 10 hanenin toplamının birler basamağı
        var sumFirst10 = d.Take(10).Sum();
        var digit11 = sumFirst10 % 10;

        return d[10] == digit11;
    }

    #endregion

    #region VKN

    /// <inheritdoc />
    public bool ValidateVergiNo(string? vkn)
    {
        if (string.IsNullOrWhiteSpace(vkn))
            return false;

        var digits = vkn.Trim();

        if (digits.Length != TurkeyCoreDefaults.Validation.VergiNoLength)
            return false;

        if (!digits.All(char.IsDigit))
            return false;

        // GİB algoritması: 10 haneli VKN için pozisyonel checksum
        // 1..9. haneler ile son hane (checksum) ilişkisi:
        //   for i in 0..8: tmp = (d[i] + (9 - i)) mod 10
        //                  v[i] = (tmp * 2^(9-i)) mod 9; eğer tmp != 0 ve v == 0 ise v = 9
        //   sum = sum(v[0..8])
        //   checksum = (10 - (sum mod 10)) mod 10
        var d = digits.Select(c => c - '0').ToArray();

        long sum = 0;
        for (var i = 0; i < 9; i++)
        {
            var tmp = (d[i] + (9 - i)) % 10;
            if (tmp == 0)
                continue;

            var v = (tmp * (long)BigInteger.ModPow(2, 9 - i, 9));
            v %= 9;
            if (v == 0)
                v = 9;

            sum += v;
        }

        var checksum = (10 - (sum % 10)) % 10;
        return d[9] == checksum;
    }

    /// <inheritdoc />
    public bool ValidateVergiOrTckn(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var trimmed = value.Trim();

        // Türkiye'de şahıs firmalarının vergi numarası TCKN'dir (213 sayılı VUK).
        // 10 hane → tüzel kişi VKN, 11 hane → şahıs firması TCKN.
        return trimmed.Length switch
        {
            TurkeyCoreDefaults.Validation.VergiNoLength => ValidateVergiNo(trimmed),
            TurkeyCoreDefaults.Validation.TcKimlikNoLength => ValidateTcKimlikNo(trimmed),
            _ => false
        };
    }

    #endregion

    #region IBAN

    /// <inheritdoc />
    public bool ValidateIbanTr(string? iban)
    {
        if (string.IsNullOrWhiteSpace(iban))
            return false;

        // Boşlukları ve büyük/küçük harf farkını normalize et
        var normalized = iban.Replace(" ", string.Empty).ToUpperInvariant();

        if (normalized.Length != TurkeyCoreDefaults.Validation.IbanTrLength)
            return false;

        if (!normalized.StartsWith(TurkeyCoreDefaults.Validation.IbanTrPrefix, StringComparison.Ordinal))
            return false;

        // İlk 4 karakter (TRkk) sona taşınır
        var rearranged = normalized[4..] + normalized[..4];

        // Harfleri sayısal karşılıklarına çevir: A=10, B=11, ..., Z=35
        // (TR için T=29, R=27)
        var numeric = new System.Text.StringBuilder(rearranged.Length * 2);
        foreach (var c in rearranged)
        {
            if (char.IsDigit(c))
                numeric.Append(c);
            else if (c >= 'A' && c <= 'Z')
                numeric.Append((c - 'A' + 10).ToString());
            else
                return false; // geçersiz karakter
        }

        // MOD-97: BigInteger gerekir (26 hane sayısal karşılık -> ~50 basamak)
        if (!BigInteger.TryParse(numeric.ToString(), out var value))
            return false;

        return value % 97 == 1;
    }

    #endregion

    #region GSM

    /// <inheritdoc />
    public bool ValidateGsmNumber(string? gsm)
    {
        return NormalizeGsmNumber(gsm) is not null;
    }

    /// <inheritdoc />
    public string? NormalizeGsmNumber(string? gsm)
    {
        if (string.IsNullOrWhiteSpace(gsm))
            return null;

        // Sadece rakamları al
        var digits = new string(gsm.Where(char.IsDigit).ToArray());

        // 10 haneli (5XX...) -> 90 ekle
        // 11 haneli (05XX...) -> baştaki 0'ı at, 90 ekle
        // 12 haneli (905XX...) -> olduğu gibi
        // +905XX biçimleri zaten yukarıdaki "sadece rakam" filtresinden 12 haneye düşer
        var msisdn = digits.Length switch
        {
            10 when digits[0] == '5' => "90" + digits,
            11 when digits.StartsWith("05") => "90" + digits[1..],
            12 when digits.StartsWith("905") => digits,
            _ => null
        };

        if (msisdn is null)
            return null;

        // 90 sonrası 5XX olmalı
        var subscriber = msisdn[2..]; // 10 hane (5XXXXXXXXX)
        var prefix = subscriber[..3];

        if (!_gsmPrefixes.Contains(prefix))
            return null;

        // +90 5XX XXX XX XX formatı
        return $"+90 {subscriber[..3]} {subscriber[3..6]} {subscriber[6..8]} {subscriber[8..]}";
    }

    #endregion

    #region Türkçe karakter normalize

    /// <inheritdoc />
    public string NormalizeTurkishCharacters(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var result = new System.Text.StringBuilder(input.Length);
        foreach (var c in input)
            result.Append(_turkishCharMap.TryGetValue(c, out var mapped) ? mapped : c);

        return result.ToString();
    }

    #endregion
}
