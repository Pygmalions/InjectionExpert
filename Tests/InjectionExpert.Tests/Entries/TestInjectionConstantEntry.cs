using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionConstantEntry))]
public class TestInjectionConstantEntry
{
    [Test]
    public void GetInjection_EntryHasConstantValue_ReturnsSameValue()
    {
        var value = new object();
        var entry = new InjectionConstantEntry(value);
        var provider = new InjectionContainer();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.Lifespan, Is.EqualTo(InjectionLifespan.Singleton));
            Assert.That(entry.Value, Is.SameAs(value));
            Assert.That(entry.GetInjection(provider, typeof(object), null, default), Is.SameAs(value));
        }
    }

    [Test]
    public void IsAssignableTo_CheckingDifferentTypes_ReturnsExpected()
    {
        var entry = new InjectionConstantEntry("text");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(entry.IsAssignableTo(typeof(object)), Is.True);
            Assert.That(entry.IsAssignableTo(typeof(int)), Is.False);
        }
    }

    [Test]
    public void ToString_Called_ReturnsConstantText()
    {
        var entry = new InjectionConstantEntry("abc");

        Assert.That(entry.ToString(), Is.EqualTo("(Constant, abc)"));
    }
}
