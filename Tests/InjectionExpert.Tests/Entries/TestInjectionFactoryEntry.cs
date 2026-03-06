using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionFactoryEntry<string>))]
public class TestInjectionFactoryEntry
{
    [Test]
    public void IsAssignableTo_CheckingCompatibleAndIncompatibleTypes_ReturnsExpected()
    {
        var entry = new InjectionFactoryEntry<string>(InjectionLifespan.Transient,
            (_, _, _, _) => "ok");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Lifespan, Is.EqualTo(InjectionLifespan.Transient));
            Assert.That(entry.Factory, Is.Not.Null);
            Assert.That(entry.IsAssignableTo(typeof(object)), Is.True);
            Assert.That(entry.IsAssignableTo(typeof(int)), Is.False);
        }
    }

    [Test]
    public void GetInjection_InvokingFactory_PassesSameParameters()
    {
        var provider = new InjectionContainer();
        var requestedType = typeof(string);
        var requestedKey = "k";
        var requestedTarget = default(InjectionTarget);

        IInjectionProvider? capturedProvider = null;
        Type? capturedType = null;
        object? capturedKey = null;
        InjectionTarget capturedTarget = default;

        var entry = new InjectionFactoryEntry<string>(InjectionLifespan.Singleton,
            (inProvider, inType, inKey, inTarget) =>
            {
                capturedProvider = inProvider;
                capturedType = inType;
                capturedKey = inKey;
                capturedTarget = inTarget;
                return "result";
            });

        var result = entry.GetInjection(provider, requestedType, requestedKey, requestedTarget);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Is.EqualTo("result"));
            Assert.That(capturedProvider, Is.SameAs(provider));
            Assert.That(capturedType, Is.EqualTo(requestedType));
            Assert.That(capturedKey, Is.EqualTo(requestedKey));
            Assert.That(capturedTarget, Is.EqualTo(requestedTarget));
        }
    }

    [Test]
    public void ToString_Called_ReturnsFactoryPrefix()
    {
        var entry = new InjectionFactoryEntry<string>(InjectionLifespan.Singleton,
            (_, _, _, _) => "x");

        Assert.That(entry.ToString(), Does.StartWith("(Factory: "));
    }
}
