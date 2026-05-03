namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Withdrawal;

/// <summary>
/// Cayma nedeni — yasal olarak gerekçe ZORUNLU değildir (cayma hakkı koşulsuz),
/// ama analiz ve müşteri memnuniyeti için kayıt edilir.
/// </summary>
public enum WithdrawalReason
{
    /// <summary>Belirtmek istemiyor</summary>
    NotSpecified = 0,

    /// <summary>Beklediği gibi değil / fikrini değiştirdi</summary>
    ChangedMind = 1,

    /// <summary>Üründe kalite sorunu var (kullanılabilir ama beklenenden düşük)</summary>
    QualityIssue = 2,

    /// <summary>Ürün arızalı / hasarlı geldi</summary>
    Defective = 3,

    /// <summary>Yanlış ürün gönderildi</summary>
    WrongProduct = 4,

    /// <summary>Ürün geç teslim edildi</summary>
    LateDelivery = 5,

    /// <summary>Daha iyi fiyat / ürün buldum</summary>
    BetterAlternative = 6,

    /// <summary>Diğer (Description alanında detay)</summary>
    Other = 99
}
