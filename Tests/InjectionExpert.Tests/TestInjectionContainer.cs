using InjectionExpert.Entries;
using JetBrains.Annotations;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(InjectionContainer))]
public class TestInjectionContainer
{
    private class StubEmptyClass
    {
    }

    [Test]
    public void AddInjection_And_GetInjection()
    {
        var container = new InjectionContainer();
        container.AddInjection(typeof(int), null, new InjectionConstantEntry(123));
        var value = (int?)container.GetInjection(typeof(int));
        Assert.That(value, Is.EqualTo(123));
    }

    [Test]
    public void AddTransient_Type_CreatesNewInstances()
    {
        var container = new InjectionContainer();
        container.AddInjection(InjectionLifespan.Transient, 
            typeof(StubEmptyClass), 
            typeof(StubEmptyClass));
        var a = container.GetInjection(typeof(StubEmptyClass));
        var b = container.GetInjection(typeof(StubEmptyClass));
        Assert.That(a, Is.Not.SameAs(b));
    }

    [Test]
    public void AddFactory_Singleton_CachesInstances()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Singleton, typeof(string),
            (_, _, _, _) =>
            {
                created++;
                return Guid.NewGuid().ToString();
            });
        var a = container.RequireInjection<string>();
        var b = container.RequireInjection<string>();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.EqualTo(b));
            Assert.That(created, Is.EqualTo(1));
        }
    }

    [Test]
    public void Redirection_Redirects()
    {
        var container = new InjectionContainer();
        container.AddSingleton("text");
        container.AddRedirection(typeof(object), null, typeof(string), null);
        var instance = container.RequireInjection<object>();
        Assert.That(instance, Is.EqualTo("text"));
    }

    [Test]
    public void TryAdd_ExistingEntry_Fails()
    {
        var container = new InjectionContainer();
        var added = container.TryAddSingleton(typeof(int), 1);
        var addedAgain = container.TryAddSingleton(typeof(int), 2);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(added, Is.True);
            Assert.That(addedAgain, Is.False);
        }

        var removed = container.RemoveInjection(typeof(int), null);
        Assert.That(removed, Is.True);
        var missing = container.GetInjection(typeof(int));
        Assert.That(missing, Is.Null);
    }

    [Test]
    public void RemoveInjection_RemovesEntry()
    {
        var container = new InjectionContainer();
        container.TryAddSingleton(typeof(int), 1);
        var removed = container.RemoveInjection(typeof(int), null);
        Assert.That(removed, Is.True);
        var missing = container.GetInjection(typeof(int));
        Assert.That(missing, Is.Null);
    }

    [Test]
    public void InvalidateCache_RecreatesSingletons()
    {
        var container = new InjectionContainer();
        var created = 0;
        container.AddInjection(InjectionLifespan.Singleton, typeof(Guid), (_, _, _, _) =>
        {
            created++;
            return Guid.NewGuid();
        });
        _ = container.RequireInjection<Guid>();
        container.InvalidateCache();
        _ = container.RequireInjection<Guid>();
        Assert.That(created, Is.EqualTo(2));
    }

    [Test]
    public void Clear_RemovesAllEntries()
    {
        var container = new InjectionContainer();
        container.AddSingleton(1);
        container.AddSingleton("ok");
        container.Clear();
        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.GetInjection(typeof(int)), Is.Null);
            Assert.That(container.GetInjection<string>(), Is.Null);
        }
    }

    [Test]
    public void Enumeration_ListsAllEntries()
    {
        var container = new InjectionContainer();
        container.AddSingleton(1);
        container.AddSingleton("ok");
        container.AddInjection(InjectionLifespan.Transient, typeof(object),
            typeof(StubEmptyClass), key: "k");
        var list = container.ToList();
        // At least 3 items (int, string, object with a key)
        Assert.That(list.Count, Is.GreaterThanOrEqualTo(3));
        Assert.That(list.Any(i => i.Type == typeof(int) && i.Key is null));
        Assert.That(list.Any(i => i.Type == typeof(object) && Equals(i.Key, "k")));
    }

    [UsedImplicitly]
    private interface ISampleGenericInterface<TType>
    {
    }

    [UsedImplicitly]
    private class SampleGenericClass<TContent> : ISampleGenericInterface<TContent>
    {
    }
    
    [Test]
    public void GetInjectionItem_Generic_ResolvesGenericDefinition()
    {
        var container = new InjectionContainer()
            .AddSingleton(typeof(ISampleGenericInterface<>), typeof(SampleGenericClass<>));

        var injection = container.GetInjection(typeof(ISampleGenericInterface<int>));
        
        Assert.That(injection, Is.Not.Null);
        Assert.That(injection, Is.TypeOf<SampleGenericClass<int>>());
    }
}