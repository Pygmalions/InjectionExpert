// Agent: Junie, gpt-5-2025-08-07
using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionRedirectionEntry))]
public class TestInjectionRedirectionEntry
{
    [Test]
    public void GetInjection_Redirects_To_TargetType_And_Key()
    {
        var container = new InjectionContainer();
        var expected = new object();
        var destKey = $"dest-{TestContext.CurrentContext.Random.Next(1, 100000)}";
        container.AddSingleton(typeof(object), expected, key: destKey);

        var redirection = new InjectionRedirectionEntry(typeof(object), targetKey: destKey);
        var value = redirection.GetInjection(container);

        Assert.That(value, Is.SameAs(expected));
    }

    [Test]
    public void ToString_ContainsRedirectionInfo()
    {
        var redirection = new InjectionRedirectionEntry(typeof(int), 3);
        var text = redirection.ToString();
        Assert.That(text, Does.Contain("Redirection").And.Contain("System.Int32"));
    }
}
