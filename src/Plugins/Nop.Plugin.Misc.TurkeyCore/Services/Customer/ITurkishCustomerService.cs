using NopCustomer = Nop.Core.Domain.Customers.Customer;
using Nop.Plugin.Misc.TurkeyCore.Domain;

namespace Nop.Plugin.Misc.TurkeyCore.Services.Customer;

/// <summary>
/// nopCommerce <see cref="NopCustomer"/>'ı ile <see cref="TurkishCustomerExtension"/>
/// arasındaki köprü servisi. Hassas alanlar (TCKN, VKN) <see cref="Nop.Services.Security.IEncryptionService"/>
/// üzerinden şifrelenmiş halde saklanır; servis API'si plain (decrypted) değerleri döner.
///
/// Diğer Türkiye plugin'leri (Iyzico, ConsumerLaw, EFatura) bu servis üzerinden
/// müşteri bilgilerine erişir — repository'leri doğrudan kullanmaz.
/// </summary>
public interface ITurkishCustomerService
{
    /// <summary>
    /// Müşteriye ait extension kaydını getirir. Yoksa null döner.
    /// </summary>
    Task<TurkishCustomerExtension?> GetExtensionAsync(NopCustomer customer);

    /// <summary>
    /// Müşteriye ait extension kaydını getirir; yoksa default değerlerle yeni bir tane oluşturup kaydeder.
    /// Kayıt formundan geçen müşterilerde otomatik bireysel olarak işaretlenir.
    /// </summary>
    Task<TurkishCustomerExtension> GetOrCreateExtensionAsync(NopCustomer customer);

    /// <summary>
    /// Extension kaydını ekler veya günceller. CustomerId ataması zorunlu.
    /// </summary>
    Task UpsertExtensionAsync(TurkishCustomerExtension extension);

    /// <summary>
    /// TC Kimlik No'yu şifreleyerek extension'a yazar (validasyon yapılmaz — caller doğrulamış olmalı).
    /// </summary>
    Task SetTcKimlikNoAsync(NopCustomer customer, string? tckn);

    /// <summary>
    /// Şifreli TCKN'yi çözerek döner. Yoksa null.
    /// </summary>
    Task<string?> GetTcKimlikNoAsync(NopCustomer customer);

    /// <summary>
    /// Vergi No'yu şifreleyerek extension'a yazar.
    /// </summary>
    Task SetVergiNoAsync(NopCustomer customer, string? vkn);

    /// <summary>
    /// Şifreli VKN'yi çözerek döner. Yoksa null.
    /// </summary>
    Task<string?> GetVergiNoAsync(NopCustomer customer);

    /// <summary>
    /// Müşteri kurumsal mı? Extension yoksa false (default bireysel).
    /// </summary>
    Task<bool> IsCorporateAsync(NopCustomer customer);

    /// <summary>
    /// Müşteri e-fatura mükellefi mi (GİB sorgusundan kayıtlı bilgi).
    /// null = henüz sorgulanmamış.
    /// </summary>
    Task<bool?> IsEFaturaMukellefiAsync(NopCustomer customer);
}
