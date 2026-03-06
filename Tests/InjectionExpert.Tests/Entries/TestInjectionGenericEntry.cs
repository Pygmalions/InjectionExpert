using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionGenericEntry))]
public class TestInjectionGenericEntry
{
    private abstract class GenericBase<TValue>;

    private sealed class GenericImplementation<TValue> : GenericBase<TValue>;

    private interface IGenericContract<TValue>;

    private sealed class GenericContractImplementation<TValue> : IGenericContract<TValue>;

    [Test]
    public void IsAssignableTo_ImplementationIsOpenGeneric_ReturnsExpected()
    {
        var entry = new InjectionGenericEntry(InjectionLifespan.Singleton, typeof(GenericImplementation<>));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Lifespan, Is.EqualTo(InjectionLifespan.Singleton));
            Assert.That(entry.Implementation, Is.EqualTo(typeof(GenericImplementation<>)));
            Assert.That(entry.IsAssignableTo(typeof(GenericBase<>)), Is.True);
            Assert.That(entry.IsAssignableTo(typeof(GenericBase<int>)), Is.True);
            Assert.That(entry.IsAssignableTo(typeof(IDisposable)), Is.False);
        }
    }

    [Test]
    public void IsAssignableTo_TargetIsConstructedInterface_ReturnsTrue()
    {
        var entry = new InjectionGenericEntry(InjectionLifespan.Transient, typeof(GenericContractImplementation<>));

        Assert.That(entry.IsAssignableTo(typeof(IGenericContract<int>)), Is.True);
    }

    [Test]
    public void GetInjection_RequestTypeIsImplementationDefinition_ReturnsClosedType()
    {
        var provider = new InjectionContainer();
        var entry = new InjectionGenericEntry(InjectionLifespan.Transient, typeof(GenericImplementation<>));

        var instance = entry.GetInjection(provider, typeof(GenericImplementation<int>), null, default);

        Assert.That(instance, Is.TypeOf<GenericImplementation<int>>());
    }

    [Test]
    public void GetInjection_RequestTypeMatchesGenericBase_ReturnsImplementationType()
    {
        var provider = new InjectionContainer();
        var entry = new InjectionGenericEntry(InjectionLifespan.Scoped, typeof(GenericImplementation<>));

        var instance = entry.GetInjection(provider, typeof(GenericBase<string>), null, default);

        Assert.That(instance, Is.TypeOf<GenericImplementation<string>>());
    }

    [Test]
    public void GetInjection_RequestTypeDoesNotMatch_ThrowsArgumentException()
    {
        var provider = new InjectionContainer();
        var entry = new InjectionGenericEntry(InjectionLifespan.Transient, typeof(GenericImplementation<>));

        Assert.Throws<ArgumentException>(() =>
            entry.GetInjection(provider, typeof(List<int>), null, default));
    }

    [Test]
    public void ToString_Called_ReturnsGenericText()
    {
        var entry = new InjectionGenericEntry(InjectionLifespan.Singleton, typeof(GenericImplementation<>));

        Assert.That(entry.ToString(), Is.EqualTo($"(Generic Definition: {typeof(GenericImplementation<>)})"));
    }
}
