namespace Nop.Plugin.Misc.TurkishConsumerLaw.Services.Contracts;

/// <summary>
/// Sözleşme şablonundaki <c>{{...}}</c> token'larını <see cref="ContractTokenContext"/> değerleriyle değiştirir.
/// Pure function — IO yapmaz, deterministic.
/// </summary>
public interface IContractTokenReplacer
{
    /// <summary>
    /// Token replace edilmiş HTML döner. Bilinmeyen token'lar olduğu gibi bırakılır
    /// (admin görsün ve düzeltsin).
    /// </summary>
    string Replace(string template, ContractTokenContext context);
}
