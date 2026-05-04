using FluentMigrator.Builders.Create.Table;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Data.Mapping.Builders;

/// <summary>
/// <see cref="TurkishCartItemPriceLock"/> tablo şeması.
///
/// <c>ShoppingCartItemId</c> hard FK (cascade delete) — sepet kalemi silinince lock da silinir.
/// </summary>
public class TurkishCartItemPriceLockBuilder : NopEntityBuilder<TurkishCartItemPriceLock>
{
    public override void MapEntity(CreateTableExpressionBuilder table)
    {
        table
            .WithColumn(nameof(TurkishCartItemPriceLock.ShoppingCartItemId)).AsInt32().NotNullable().ForeignKey<ShoppingCartItem>()
            .WithColumn(nameof(TurkishCartItemPriceLock.LockedUnitPrice)).AsDecimal(18, 4).NotNullable()
            .WithColumn(nameof(TurkishCartItemPriceLock.LockedRate)).AsDecimal(18, 6).NotNullable()
            .WithColumn(nameof(TurkishCartItemPriceLock.BaseCurrencyCode)).AsString(10).NotNullable()
            .WithColumn(nameof(TurkishCartItemPriceLock.LockedAtUtc)).AsDateTime2().NotNullable();
    }
}
