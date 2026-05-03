namespace Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;

/// <summary>
/// Onayın hangi noktadan geldiği — denetim ve geri çekme akışı için.
/// </summary>
public enum ConsentSource
{
    /// <summary>Müşteri kayıt formu</summary>
    Registration = 1,

    /// <summary>Müşteri profil sayfası (geri dönüp güncelleme)</summary>
    Profile = 2,

    /// <summary>Checkout sırasında ek onay</summary>
    Checkout = 3,

    /// <summary>Banner/Modal — site genelinde toplu onay</summary>
    Banner = 4,

    /// <summary>Müşteri rıza geri çekti (bu kayıt yeni "denied" satırı olur)</summary>
    Withdrawal = 5,

    /// <summary>Admin manuel müdahalesi (örn. müşteri çağrı merkezini arayıp rıza verdi)</summary>
    AdminManual = 6
}
