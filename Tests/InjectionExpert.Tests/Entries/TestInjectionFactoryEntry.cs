// Agent: Junie, gpt-5-2025-08-07
using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionFactoryEntry<>))]
public class TestInjectionFactoryEntry
{
    [Test]
    public void TransientFactory_CreatesNewInstances()
    {
        var provider = new InjectionContainer();
        var entry = new InjectionFactoryEntry<object>(
            InjectionLifespan.Transient,
            (_, _, _, _) => new object());

        var a = entry.GetInjection(provider, typeof(object), null, default);
        var b = entry.GetInjection(provider, typeof(object), null, default);
        Assert.That(a, Is.Not.SameAs(b));
    }

    [Test]
    public void SingletonFactory_CachesInstance_And_InvalidateCache()
    {
        var provider = new InjectionContainer();
        var created = 0;
        var entry = new InjectionFactoryEntry<string>(
            InjectionLifespan.Singleton,
            (_, _, _, _) =>
            {
                created++;
                return Guid.NewGuid().ToString();
            });

        var a = (string)entry.GetInjection(provider, typeof(string), null, default);
        var b = (string)entry.GetInjection(provider, typeof(string), null, default);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(created, Is.EqualTo(1));
        }

        var invalidated = entry.InvalidateCache();
        var c = (string)entry.GetInjection(provider, typeof(string), null, default);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(invalidated, Is.True);
            Assert.That(c, Is.Not.EqualTo(a));
            Assert.That(created, Is.EqualTo(2));
        }
    }

    [Test]
    public void UntypedFactory_InvokesStronglyTypedFactory()
    {
        var provider = new InjectionContainer();
        var expected = new object();
        var entry = new InjectionFactoryEntry<object>(
            InjectionLifespan.Transient,
            (_, _, _, _) => expected);

        var obj = entry.UntypedFactory(provider, typeof(object), null, default);
        Assert.That(obj, Is.SameAs(expected));
    }

    [Test]
    public void ToString_ContainsFactoryText()
    {
        var entry = new InjectionFactoryEntry<object>(
            InjectionLifespan.Transient,
            (_, _, _, _) => new object());
        Assert.That(entry.ToString(), Does.Contain("Factory"));
    }
}
