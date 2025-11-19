using InjectionExpert.Entries;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionConstantEntry))]
public class TestInjectionConstantEntry
{
    [Test]
    public void GetInjection_ReturnsSameInstance()
    {
        var instance = new object();
        var entry = new InjectionConstantEntry(instance);

        var v1 = entry.GetInjection();
        var v2 = entry.GetInjection();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(v1, Is.SameAs(instance));
            Assert.That(v2, Is.SameAs(instance));
        }
    }

    [Test]
    public void InvalidateCache_NoEffect_ReturnsFalse()
    {
        var randomInt = TestContext.CurrentContext.Random.Next(1, int.MaxValue);
        var entry = new InjectionConstantEntry(randomInt);
        Assert.That(entry.InvalidateCache(), Is.False);
    }

    [Test]
    public void ToString_ContainsConstantValue()
    {
        var length = TestContext.CurrentContext.Random.Next(3, 16);
        var value = new string(Enumerable.Range(0, length)
            .Select(_ => (char)('a' + TestContext.CurrentContext.Random.Next(0, 26))).ToArray());
        var entry = new InjectionConstantEntry(value);
        Assert.That(entry.ToString(), Does.Contain("Constant").And.Contain(value));
    }
}
