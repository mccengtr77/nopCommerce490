using Nop.Core;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

/// <summary>
/// Kullanıcının verdiği veya geri çektiği rızaların **append-only audit log'u**.
///
/// Her rıza değişikliği yeni satır olur (mevcut satır UPDATE edilmez) — KVKK m.7
/// ispat yükümlülüğü için tüm onay tarihi zincirinin görülebilmesi gerek.
/// "Mevcut etkin rıza" = scope başına en güncel kayıt.
/// </summary>
public class ConsentRecord : BaseEntity
{
    /// <summary>
    /// Kayıtlı müşteri (0 = anonim/tanımsız)
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Onay kapsamı (KVKK kişisel veri / profilleme / yurtdışı / ETK SMS/email/call)
    /// </summary>
    public ConsentScope Scope { get; set; }

    /// <summary>
    /// Onay verildi mi (true) yoksa geri çekildi mi (false)
    /// </summary>
    public bool Granted { get; set; }

    /// <summary>
    /// Hangi metnin onaylandığı — sürüm ve metin değişikliği denetimi için
    /// </summary>
    public string TextVersion { get; set; } = string.Empty;

    /// <summary>
    /// Onay/red kaynağı (kayıt formu, profil, banner, geri çekme...)
    /// </summary>
    public ConsentSource Source { get; set; }

    public string IpAddress { get; set; } = string.Empty;
    public string UserAgent { get; set; } = string.Empty;

    /// <summary>
    /// SHA-256 — kayıt manipülasyon kontrolü
    /// </summary>
    public string ContentHash { get; set; } = string.Empty;

    public DateTime CreatedOnUtc { get; set; }
}
