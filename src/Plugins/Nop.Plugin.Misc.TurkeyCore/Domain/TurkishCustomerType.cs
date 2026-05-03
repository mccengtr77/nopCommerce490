namespace Nop.Plugin.Misc.TurkeyCore.Domain;

/// <summary>
/// Müşteri tipi — bireysel/kurumsal ayrımı (6502 sayılı Tüketici Kanunu kapsamında
/// tüketici / tacir ayrımıyla örtüşür)
/// </summary>
public enum TurkishCustomerType
{
    /// <summary>
    /// Bireysel müşteri (tüketici) — TCKN ile temsil edilir
    /// </summary>
    Individual = 1,

    /// <summary>
    /// Kurumsal müşteri (tacir) — VKN ile temsil edilir, e-fatura mükellefi olabilir
    /// </summary>
    Corporate = 2
}
