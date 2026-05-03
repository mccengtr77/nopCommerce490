using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Common;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

/// <summary>
/// TurkishAddressExtension mapping.
///
/// <para>
/// <b>Cascade path notu</b>: Province/District/Neighborhood için <c>.ForeignKey&lt;T&gt;()</c>
/// kullanılmıyor — TurkishProvince → TurkishDistrict → TurkishAddressExtension cascade zinciri
/// + TurkishProvince → TurkishAddressExtension doğrudan cascade'i SQL Server'da
/// "multiple cascade paths" hatası veriyor (Error 1785).
/// </para>
/// <para>
/// Çözüm: Sadece <c>Indexed()</c> ile index oluşturulup FK constraint atlanır (soft FK).
/// Province/District silindiğinde extension'da orphan referans kalabilir; service katmanı
/// <see cref="ITurkishLocationService"/> üzerinden lookup yaparken null kontrolü yapar.
/// </para>
/// </summary>
public class TurkishAddressExtensionBuilder : NopEntityBuilder<TurkishAddressExtension>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishAddressExtension.AddressId)).AsInt32().NotNullable().ForeignKey<Address>()
            // Province/District/Neighborhood: soft FK (sadece index)
            .WithColumn(nameof(TurkishAddressExtension.ProvinceId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(TurkishAddressExtension.DistrictId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(TurkishAddressExtension.NeighborhoodId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(TurkishAddressExtension.AdresAdi)).AsString(100).Nullable()
            .WithColumn(nameof(TurkishAddressExtension.BinaNo)).AsString(20).Nullable()
            .WithColumn(nameof(TurkishAddressExtension.DaireNo)).AsString(20).Nullable()
            // Müşteri tipi enum: explicit AsInt32 (Karar 22)
            .WithColumn(nameof(TurkishAddressExtension.MusteriTipi)).AsInt32().NotNullable().WithDefaultValue(1)
            // Faturalama: TCKN (bireysel) ya da VKN+VergiDairesi (kurumsal). Şifreli string.
            .WithColumn(nameof(TurkishAddressExtension.TcKimlikNo)).AsString(256).Nullable()
            .WithColumn(nameof(TurkishAddressExtension.VergiNo)).AsString(256).Nullable()
            .WithColumn(nameof(TurkishAddressExtension.VergiDairesiId)).AsInt32().Nullable().Indexed();
    }
}
