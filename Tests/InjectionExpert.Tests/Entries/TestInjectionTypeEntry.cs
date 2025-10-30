// Agent: Junie, gpt-5-2025-08-07
using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionTypeEntry))]
public class TestInjectionTypeEntry
{
    private class StubEmptyClass {}

    [Test]
    public void Transient_CreatesNewInstances()
    {
        var container = new InjectionContainer();
        var entry = new InjectionTypeEntry(InjectionLifespan.Transient, typeof(StubEmptyClass));

        var a = entry.GetInjection(container);
        var b = entry.GetInjection(container);

        Assert.That(a, Is.Not.SameAs(b));
    }

    [Test]
    public void Singleton_Caches_And_Invalidate()
    {
        var container = new InjectionContainer();
        var entry = new InjectionTypeEntry(InjectionLifespan.Singleton, typeof(StubEmptyClass));

        var a = entry.GetInjection(container);
        var b = entry.GetInjection(container);
        Assert.That(a, Is.SameAs(b));

        var invalidated = entry.InvalidateCache();
        var c = entry.GetInjection(container);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(invalidated, Is.True);
            Assert.That(c, Is.Not.SameAs(a));
        }
    }

    [Test]
    public void ToString_ContainsImplementationType()
    {
        var entry = new InjectionTypeEntry(InjectionLifespan.Transient, typeof(StubEmptyClass));
        Assert.That(entry.ToString(), 
            Does.Contain("Type").And.Contain(nameof(StubEmptyClass)));
    }
}
