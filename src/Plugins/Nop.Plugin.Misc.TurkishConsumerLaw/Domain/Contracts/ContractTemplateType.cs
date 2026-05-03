namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Contracts;

/// <summary>
/// Sözleşme türü.
/// </summary>
public enum ContractTemplateType
{
    /// <summary>
    /// Mesafeli Satış Sözleşmesi — 6502 sayılı Tüketici Kanunu m.48
    /// ve Mesafeli Sözleşmeler Yönetmeliği kapsamında zorunlu.
    /// </summary>
    Mss = 1,

    /// <summary>
    /// Ön Bilgilendirme Formu — Mesafeli Sözleşmeler Yönetmeliği m.5
    /// gereği sipariş öncesi sunulması zorunlu.
    /// 2026 güncellemesi: Arabuluculuk şartı bilgisi de zorunlu eklendi.
    /// </summary>
    Obf = 2
}
