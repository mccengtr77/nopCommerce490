using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkeyCore.Tests;

/// <summary>
/// Assembly-level setup: nopCommerce'in <see cref="CacheKey"/> ctor'u
/// <see cref="Singleton{T}.Instance"/> üzerinden <see cref="CacheConfig"/> default cache time'a
/// erişir. Test ortamında bu Singleton init edilmediğinde NullReferenceException atar.
/// Bu sınıf tüm test fixture'lardan önce gerekli AppSettings'i kurar.
/// </summary>
[SetUpFixture]
public class AssemblySetup
{
    [OneTimeSetUp]
    public void GlobalSetup()
    {
        if (Singleton<AppSettings>.Instance is null)
        {
            Singleton<AppSettings>.Instance = new AppSettings(new List<IConfig>
            {
                new CacheConfig()
            });
        }
    }
}
