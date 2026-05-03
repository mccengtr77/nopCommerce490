using FluentAssertions;
using Moq;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Misc.TurkeyCore.Domain;
using Nop.Plugin.Misc.TurkeyCore.Services.Customer;
using Nop.Services.Security;
using NUnit.Framework;
using NopCustomer = Nop.Core.Domain.Customers.Customer;

namespace Nop.Plugin.Misc.TurkeyCore.Tests.Customer;

/// <summary>
/// <see cref="TurkishCustomerService"/> birim testleri.
/// IRepository ve IEncryptionService mock'lanır.
/// </summary>
[TestFixture]
public class TurkishCustomerServiceTests
{
    private Mock<IRepository<TurkishCustomerExtension>> _repo = null!;
    private Mock<IEncryptionService> _encryption = null!;
    private TurkishCustomerService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _repo = new Mock<IRepository<TurkishCustomerExtension>>();
        _encryption = new Mock<IEncryptionService>();

        // Encryption mock'u kontrolü kolaylaştırmak için "ENC:" prefix ekler
        _encryption.Setup(e => e.EncryptText(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns<string, string>((plain, _) => $"ENC:{plain}");
        _encryption.Setup(e => e.DecryptText(It.IsAny<string>(), It.IsAny<string>()))
                   .Returns<string, string>((cipher, _) =>
                       cipher.StartsWith("ENC:") ? cipher[4..] : cipher);

        _sut = new TurkishCustomerService(_repo.Object, _encryption.Object);
    }

    private void SetupRepoReturns(IList<TurkishCustomerExtension> data)
    {
        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Func<IQueryable<TurkishCustomerExtension>, IQueryable<TurkishCustomerExtension>>>(),
                It.IsAny<Func<ICacheKeyService, CacheKey>>(),
                It.IsAny<bool>()))
             .ReturnsAsync(data);
    }

    #region GetExtensionAsync

    [Test]
    public async Task GetExtensionAsync_NullCustomer_Throws()
    {
        var act = () => _sut.GetExtensionAsync(null!);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [TestCase(0)]
    [TestCase(-1)]
    public async Task GetExtensionAsync_InvalidCustomerId_ReturnsNullWithoutQuery(int id)
    {
        var customer = new NopCustomer { Id = id };

        var result = await _sut.GetExtensionAsync(customer);

        result.Should().BeNull();
        _repo.Verify(r => r.GetAllAsync(
            It.IsAny<Func<IQueryable<TurkishCustomerExtension>, IQueryable<TurkishCustomerExtension>>>(),
            It.IsAny<Func<ICacheKeyService, CacheKey>>(),
            It.IsAny<bool>()), Times.Never);
    }

    [Test]
    public async Task GetExtensionAsync_ExistingCustomer_ReturnsExtension()
    {
        var ext = new TurkishCustomerExtension { Id = 5, CustomerId = 100 };
        SetupRepoReturns(new[] { ext });

        var result = await _sut.GetExtensionAsync(new NopCustomer { Id = 100 });

        result.Should().Be(ext);
    }

    [Test]
    public async Task GetExtensionAsync_NoExtension_ReturnsNull()
    {
        SetupRepoReturns(Array.Empty<TurkishCustomerExtension>());

        var result = await _sut.GetExtensionAsync(new NopCustomer { Id = 100 });

        result.Should().BeNull();
    }

    #endregion

    #region GetOrCreateExtensionAsync

    [Test]
    public async Task GetOrCreateExtensionAsync_NoExtension_CreatesAndInsertsIndividual()
    {
        SetupRepoReturns(Array.Empty<TurkishCustomerExtension>());

        var result = await _sut.GetOrCreateExtensionAsync(new NopCustomer { Id = 100 });

        result.CustomerId.Should().Be(100);
        result.MusteriTipi.Should().Be(TurkishCustomerType.Individual);
        _repo.Verify(r => r.InsertAsync(It.Is<TurkishCustomerExtension>(e => e.CustomerId == 100), It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task GetOrCreateExtensionAsync_ExistingExtension_ReturnsWithoutInsert()
    {
        var ext = new TurkishCustomerExtension { Id = 5, CustomerId = 100, MusteriTipi = TurkishCustomerType.Corporate };
        SetupRepoReturns(new[] { ext });

        var result = await _sut.GetOrCreateExtensionAsync(new NopCustomer { Id = 100 });

        result.Should().Be(ext);
        _repo.Verify(r => r.InsertAsync(It.IsAny<TurkishCustomerExtension>(), It.IsAny<bool>()), Times.Never);
    }

    #endregion

    #region UpsertExtensionAsync

    [Test]
    public async Task UpsertExtensionAsync_NewEntity_Inserts()
    {
        var ext = new TurkishCustomerExtension { Id = 0, CustomerId = 100 };

        await _sut.UpsertExtensionAsync(ext);

        _repo.Verify(r => r.InsertAsync(ext, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task UpsertExtensionAsync_ExistingEntity_Updates()
    {
        var ext = new TurkishCustomerExtension { Id = 5, CustomerId = 100 };

        await _sut.UpsertExtensionAsync(ext);

        _repo.Verify(r => r.UpdateAsync(ext, It.IsAny<bool>()), Times.Once);
    }

    [Test]
    public async Task UpsertExtensionAsync_NoCustomerId_Throws()
    {
        var ext = new TurkishCustomerExtension { CustomerId = 0 };

        var act = () => _sut.UpsertExtensionAsync(ext);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region TCKN encrypt/decrypt

    [Test]
    public async Task SetTcKimlikNoAsync_StoresEncrypted()
    {
        var existing = new TurkishCustomerExtension { Id = 5, CustomerId = 100 };
        SetupRepoReturns(new[] { existing });

        await _sut.SetTcKimlikNoAsync(new NopCustomer { Id = 100 }, "12345678950");

        existing.TcKimlikNo.Should().Be("ENC:12345678950");
        _encryption.Verify(e => e.EncryptText("12345678950", It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task SetTcKimlikNoAsync_TrimsInput()
    {
        var existing = new TurkishCustomerExtension { Id = 5, CustomerId = 100 };
        SetupRepoReturns(new[] { existing });

        await _sut.SetTcKimlikNoAsync(new NopCustomer { Id = 100 }, "  12345678950  ");

        _encryption.Verify(e => e.EncryptText("12345678950", It.IsAny<string>()), Times.Once);
    }

    [TestCase("")]
    [TestCase("  ")]
    [TestCase(null)]
    public async Task SetTcKimlikNoAsync_NullOrEmpty_StoresNull(string? value)
    {
        var existing = new TurkishCustomerExtension { Id = 5, CustomerId = 100 };
        SetupRepoReturns(new[] { existing });

        await _sut.SetTcKimlikNoAsync(new NopCustomer { Id = 100 }, value);

        existing.TcKimlikNo.Should().BeNull();
        _encryption.Verify(e => e.EncryptText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetTcKimlikNoAsync_DecryptsStoredValue()
    {
        var ext = new TurkishCustomerExtension { CustomerId = 100, TcKimlikNo = "ENC:12345678950" };
        SetupRepoReturns(new[] { ext });

        var result = await _sut.GetTcKimlikNoAsync(new NopCustomer { Id = 100 });

        result.Should().Be("12345678950");
    }

    [Test]
    public async Task GetTcKimlikNoAsync_NoExtension_ReturnsNull()
    {
        SetupRepoReturns(Array.Empty<TurkishCustomerExtension>());

        var result = await _sut.GetTcKimlikNoAsync(new NopCustomer { Id = 100 });

        result.Should().BeNull();
        _encryption.Verify(e => e.DecryptText(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Test]
    public async Task GetTcKimlikNoAsync_NullStoredValue_ReturnsNull()
    {
        var ext = new TurkishCustomerExtension { CustomerId = 100, TcKimlikNo = null };
        SetupRepoReturns(new[] { ext });

        var result = await _sut.GetTcKimlikNoAsync(new NopCustomer { Id = 100 });

        result.Should().BeNull();
    }

    #endregion

    #region Müşteri tipi sorgular

    [Test]
    public async Task IsCorporateAsync_CorporateExtension_ReturnsTrue()
    {
        SetupRepoReturns(new[] { new TurkishCustomerExtension { CustomerId = 100, MusteriTipi = TurkishCustomerType.Corporate } });

        var result = await _sut.IsCorporateAsync(new NopCustomer { Id = 100 });

        result.Should().BeTrue();
    }

    [Test]
    public async Task IsCorporateAsync_IndividualExtension_ReturnsFalse()
    {
        SetupRepoReturns(new[] { new TurkishCustomerExtension { CustomerId = 100, MusteriTipi = TurkishCustomerType.Individual } });

        var result = await _sut.IsCorporateAsync(new NopCustomer { Id = 100 });

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsCorporateAsync_NoExtension_ReturnsFalse()
    {
        SetupRepoReturns(Array.Empty<TurkishCustomerExtension>());

        var result = await _sut.IsCorporateAsync(new NopCustomer { Id = 100 });

        result.Should().BeFalse();
    }

    [Test]
    public async Task IsEFaturaMukellefiAsync_NotQueried_ReturnsNull()
    {
        SetupRepoReturns(new[] { new TurkishCustomerExtension { CustomerId = 100, IsEFaturaMukellefi = null } });

        var result = await _sut.IsEFaturaMukellefiAsync(new NopCustomer { Id = 100 });

        result.Should().BeNull();
    }

    #endregion
}
