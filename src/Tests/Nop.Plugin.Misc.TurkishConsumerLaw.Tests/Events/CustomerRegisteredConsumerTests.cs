using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Nop.Plugin.Misc.TurkishConsumerLaw.Domain.Kvkk;
using Nop.Plugin.Misc.TurkishConsumerLaw.Events;
using NUnit.Framework;

namespace Nop.Plugin.Misc.TurkishConsumerLaw.Tests.Events;

/// <summary>
/// <see cref="CustomerRegisteredConsumer.ParseScopes"/> birim testleri.
/// Form parse mantığı izole edilmiş statik metoddur — full pipeline mock gerekmiyor.
/// </summary>
[TestFixture]
public class CustomerRegisteredConsumerTests
{
    private static IFormCollection MakeForm(params string[] scopeValues)
    {
        var fields = new Dictionary<string, StringValues>
        {
            ["tcl-consent"] = new StringValues(scopeValues)
        };
        return new FormCollection(fields);
    }

    [Test]
    public void ParseScopes_ValidIntegers_ReturnsScopes()
    {
        var form = MakeForm("1", "2", "10");

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().BeEquivalentTo(new[]
        {
            ConsentScope.KvkkPersonalData,
            ConsentScope.KvkkProfiling,
            ConsentScope.EtkSms
        });
    }

    [Test]
    public void ParseScopes_UnknownEnumValue_Skipped()
    {
        // 999 enum'da yok
        var form = MakeForm("1", "999", "11");

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().BeEquivalentTo(new[]
        {
            ConsentScope.KvkkPersonalData,
            ConsentScope.EtkEmail
        });
    }

    [Test]
    public void ParseScopes_NonNumeric_Skipped()
    {
        var form = MakeForm("abc", "2", "");

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().HaveCount(1);
        result[0].Should().Be(ConsentScope.KvkkProfiling);
    }

    [Test]
    public void ParseScopes_EmptyForm_ReturnsEmpty()
    {
        var form = new FormCollection(new Dictionary<string, StringValues>());

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().BeEmpty();
    }

    [Test]
    public void ParseScopes_NoConsentField_ReturnsEmpty()
    {
        // Başka alan var ama tcl-consent yok
        var form = new FormCollection(new Dictionary<string, StringValues>
        {
            ["someOtherField"] = new StringValues("value")
        });

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().BeEmpty();
    }

    [Test]
    public void ParseScopes_DuplicateValues_KeepsBoth()
    {
        // Şu an dedup yapılmıyor; aynı scope iki kere işaretlenirse iki kayıt olur.
        // Bu davranış açıkça testle korunuyor — gelecekte değişirse bu test güncellenir.
        var form = MakeForm("1", "1", "2");

        var result = CustomerRegisteredConsumer.ParseScopes(form);

        result.Should().HaveCount(3);
    }
}
