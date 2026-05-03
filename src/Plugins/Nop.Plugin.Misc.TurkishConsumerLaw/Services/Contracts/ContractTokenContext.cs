namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <summary>
/// Token replace için gerekli tüm değişkenler. Bir sipariş için tek seferlik snapshot.
/// HTML üretiminde kullanılır.
/// </summary>
public class ContractTokenContext
{
    // Satıcı (mağaza) bilgileri
    public string SellerName { get; set; } = string.Empty;
    public string SellerAddress { get; set; } = string.Empty;
    public string SellerPhone { get; set; } = string.Empty;
    public string SellerKep { get; set; } = string.Empty;
    public string SellerMersis { get; set; } = string.Empty;

    // Alıcı (müşteri) bilgileri
    public string BuyerName { get; set; } = string.Empty;
    public string BuyerAddress { get; set; } = string.Empty;
    public string BuyerPhone { get; set; } = string.Empty;
    public string BuyerTcKimlikNo { get; set; } = string.Empty;
    public string BuyerVergiNo { get; set; } = string.Empty;

    // Sipariş bilgileri
    public string OrderNumber { get; set; } = string.Empty;
    public string OrderDate { get; set; } = string.Empty;

    /// <summary>HTML tablo halinde ürün listesi</summary>
    public string ProductListHtml { get; set; } = string.Empty;

    // Ücret bilgileri (formatlanmış string — ₺ sembolü dahil)
    public string Subtotal { get; set; } = string.Empty;
    public string TaxTotal { get; set; } = string.Empty;
    public string ShippingFee { get; set; } = string.Empty;
    public string Discount { get; set; } = string.Empty;
    public string GrandTotal { get; set; } = string.Empty;

    // Adres bilgileri
    public string ShippingAddress { get; set; } = string.Empty;
    public string BillingAddress { get; set; } = string.Empty;

    // Kargo / ödeme
    public string PaymentMethod { get; set; } = string.Empty;
    public string ShippingMethod { get; set; } = string.Empty;
    public string EstimatedDeliveryTime { get; set; } = string.Empty;

    // Yasal sabitler
    public int WithdrawalDays { get; set; } = TurkishConsumerLawDefaults.Withdrawal.PeriodDays;
    public string MediationNotice { get; set; } = string.Empty;
}
