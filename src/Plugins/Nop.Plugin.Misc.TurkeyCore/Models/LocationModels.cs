namespace Nop.Plugin.Misc.TurkeyCore.Models;

/// <summary>
/// AJAX endpoint'leri için lightweight DTO'lar.
/// Storefront cascading dropdown'larında JSON olarak döner — entity'leri direkt
/// expose etmemek için ayrı tutulmuştur (over-fetching ve internal alan sızıntısı önlenir).
/// </summary>
public record ProvinceListItemModel(int Id, string Name, int PlateCode);

public record DistrictListItemModel(int Id, string Name);

public record NeighborhoodListItemModel(int Id, string Name, string PostalCode);

/// <summary>
/// TCKN/VKN doğrulama AJAX yanıtı.
/// </summary>
public record ValidationResultModel(bool IsValid, string? Message = null);
