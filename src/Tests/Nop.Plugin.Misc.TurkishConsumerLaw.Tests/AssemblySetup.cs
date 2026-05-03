using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests;

/// <summary>
/// Assembly-level setup: <see cref="CacheKey"/> ctor'u <see cref="Singleton{T}.Instance"/>
/// üzerinden <see cref="CacheConfig"/>'e erişir; test ortamında init edilmediğinde NRE.
/// </summary>
[SetUpFixture]
public class AssemblySetup
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        Singleton<AppSettings>.Instance ??= new AppSettings(new List<IConfig> { new CacheConfig() });
    }
}
