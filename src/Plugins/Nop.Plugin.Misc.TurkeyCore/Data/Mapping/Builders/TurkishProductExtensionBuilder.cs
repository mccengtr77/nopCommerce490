using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Catalog;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

/// <summary>
/// <see cref="TurkishProductExtension"/> tablo şeması.
///
/// Notlar:
/// - <c>ProductId</c> hard FK (cascade delete ile birlikte ürün silinince extension da silinir)
/// - <c>BaseCurrencyId</c> soft FK (Indexed) — nopCommerce currency'si silinirse extension orphan kalır
///   ama service katmanı null check yapar; cascade riski almıyoruz (multiple cascade paths önleme).
/// - Decimal alanlar 18,4 — Product.Price 18,4 ile uyumlu.
/// </summary>
public class TurkishProductExtensionBuilder : NopEntityBuilder<TurkishProductExtension>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishProductExtension.ProductId)).AsInt32().NotNullable().ForeignKey<Product>()
            .WithColumn(nameof(TurkishProductExtension.BaseCurrencyId)).AsInt32().Nullable().Indexed()
            .WithColumn(nameof(TurkishProductExtension.BasePrice)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(TurkishProductExtension.BaseOldPrice)).AsDecimal(18, 4).Nullable()
            .WithColumn(nameof(TurkishProductExtension.BaseProductCost)).AsDecimal(18, 4).Nullable();
    }
}
