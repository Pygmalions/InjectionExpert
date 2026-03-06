using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionTypeEntry))]
public class TestInjectionTypeEntry
{
    private sealed class PlainType;

    [Test]
    public void Constructor_ImplementationIsGenericTypeDefinition_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            _ = new InjectionTypeEntry(InjectionLifespan.Transient, typeof(List<>)));

        Assert.That(exception!.ParamName, Is.EqualTo("implementation"));
    }

    [Test]
    public void IsAssignableTo_ImplementationTypeIsString_ReturnsExpected()
    {
        var entry = new InjectionTypeEntry(InjectionLifespan.Scoped, typeof(string));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Lifespan, Is.EqualTo(InjectionLifespan.Scoped));
            Assert.That(entry.Implementation, Is.EqualTo(typeof(string)));
            Assert.That(entry.IsAssignableTo(typeof(object)), Is.True);
            Assert.That(entry.IsAssignableTo(typeof(IDisposable)), Is.False);
        }
    }

    [Test]
    public void GetInjection_ImplementationIsConcreteType_ReturnsInstance()
    {
        var provider = new InjectionContainer();
        var entry = new InjectionTypeEntry(InjectionLifespan.Transient, typeof(PlainType));

        var instance = entry.GetInjection(provider, typeof(PlainType), null, default);

        Assert.That(instance, Is.TypeOf<PlainType>());
    }

    [Test]
    public void ToString_Called_ReturnsImplementationText()
    {
        var entry = new InjectionTypeEntry(InjectionLifespan.Singleton, typeof(string));

        Assert.That(entry.ToString(), Is.EqualTo($"(Type: {typeof(string)})"));
    }
}
