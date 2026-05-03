using Nop.Core.Caching;

namespace Nop.Plugin.Misc.TurkeyCore.Services.GibLookup;

/// <summary>
/// <see cref="IGibMukellefService"/>'nin default mock implementasyonu.
///
/// Davranış kuralı (öncelik sırası):
/// 1. Boş/null girdi → null
/// 2. Beyaz listede (always-mukellef) → true
/// 3. Kara listede (never-mukellef) → false
/// 4. Düşüş kuralı (heuristic): 10 hane VKN → true (kurumsal varsayım), 11 hane TCKN → false (bireysel varsayım)
///
/// Beyaz/kara liste, geliştirme/test sırasında belirli numaraları override etmek için kullanılır
/// (örn. "test ortamında bu VKN her zaman mukellef döner"). <see cref="TurkeyCoreSettings"/>'tan
/// virgülle ayrılmış string olarak çekilir — şimdilik hardcoded boş.
/// </summary>
public class MockGibMukellefService : IGibMukellefService
{
    #region Fields

    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly HashSet<string> _alwaysMukellef;
    protected readonly HashSet<string> _neverMukellef;

    #endregion

    #region Ctor

    public MockGibMukellefService(IStaticCacheManager staticCacheManager)
        : this(staticCacheManager,
               alwaysMukellef: Enumerable.Empty<string>(),
               neverMukellef: Enumerable.Empty<string>())
    {
    }

    /// <summary>
    /// Beyaz/kara listeleri parametrik konstrüktör — testler ve gelecekteki settings entegrasyonu için.
    /// </summary>
    public MockGibMukellefService(
        IStaticCacheManager staticCacheManager,
        IEnumerable<string> alwaysMukellef,
        IEnumerable<string> neverMukellef)
    {
        _staticCacheManager = staticCacheManager;
        _alwaysMukellef = new HashSet<string>(alwaysMukellef ?? Enumerable.Empty<string>());
        _neverMukellef = new HashSet<string>(neverMukellef ?? Enumerable.Empty<string>());
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public virtual async Task<bool?> IsEFaturaMukellefiAsync(string vknOrTckn)
    {
        var info = await GetMukellefInfoAsync(vknOrTckn);
        return info?.IsEFaturaMukellefi;
    }

    /// <inheritdoc />
    public virtual async Task<MukellefInfo?> GetMukellefInfoAsync(string vknOrTckn)
    {
        if (string.IsNullOrWhiteSpace(vknOrTckn))
            return null;

        var normalized = vknOrTckn.Trim();

        var key = _staticCacheManager.PrepareKeyForDefaultCache(
            TurkeyCoreDefaults.Cache.GibMukellef, normalized);

        return await _staticCacheManager.GetAsync(key, () => Task.FromResult(Resolve(normalized)));
    }

    /// <summary>
    /// Cache miss durumunda mock kuralları uygula.
    /// </summary>
    protected virtual MukellefInfo? Resolve(string normalized)
    {
        if (_alwaysMukellef.Contains(normalized))
            return new MukellefInfo(normalized, IsEFaturaMukellefi: true,
                SorguTarihiUtc: DateTime.UtcNow);

        if (_neverMukellef.Contains(normalized))
            return new MukellefInfo(normalized, IsEFaturaMukellefi: false,
                SorguTarihiUtc: DateTime.UtcNow);

        // Düşüş heuristic: hane sayısı ile bireysel/kurumsal ayrımı
        var isVkn = normalized.Length == TurkeyCoreDefaults.Validation.VergiNoLength
                    && normalized.All(char.IsDigit);
        var isTckn = normalized.Length == TurkeyCoreDefaults.Validation.TcKimlikNoLength
                     && normalized.All(char.IsDigit);

        if (!isVkn && !isTckn)
            return null;

        return new MukellefInfo(
            VknOrTckn: normalized,
            IsEFaturaMukellefi: isVkn,  // Kurumsallar varsayılan mukellef, bireyseller değil
            Unvan: null,
            VergiDairesiAdi: null,
            SorguTarihiUtc: DateTime.UtcNow);
    }

    #endregion
}
