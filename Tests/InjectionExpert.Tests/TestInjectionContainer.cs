using Moq;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(InjectionContainer))]
public class TestInjectionContainer
{
    [Test]
    public void AddEntry_UsingNullAndKeyedEntries_SupportsAddGetHasRemoveAndClear()
    {
        var container = new InjectionContainer();
        var unkeyedEntry = new Mock<InjectionEntry>();
        unkeyedEntry.Setup(target => target.IsAssignableTo(typeof(string))).Returns(true);
        var keyedEntry = new Mock<InjectionEntry>();
        keyedEntry.Setup(target => target.IsAssignableTo(typeof(int))).Returns(true);

        container.AddEntry(typeof(string), null, unkeyedEntry.Object);
        container.AddEntry(typeof(int), "answer", keyedEntry.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.HasEntry(typeof(string)), Is.True);
            Assert.That(container.GetEntry(typeof(string)), Is.SameAs(unkeyedEntry.Object));
            
            Assert.That(container.HasEntry(typeof(int), "answer"), Is.True);
            Assert.That(container.GetEntry(typeof(int), "answer"), Is.SameAs(keyedEntry.Object));
        }

        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.RemoveEntry(typeof(int), "answer"), Is.True);
            Assert.That(container.HasEntry(typeof(int), "answer"), Is.False);
        }

        container.ClearEntries();
        Assert.That(container.HasEntry(typeof(string)), Is.False);
    }

    [Test]
    public void AddEntry_EntryNotAssignable_ThrowsArgumentException()
    {
        var container = new InjectionContainer();
        var incompatibleEntry = new Mock<InjectionEntry>();
        incompatibleEntry.Setup(target => target.IsAssignableTo(typeof(int))).Returns(false);

        var exception = Assert.Throws<ArgumentException>(() =>
            container.AddEntry(typeof(int), null, incompatibleEntry.Object));

        Assert.That(exception!.ParamName, Is.EqualTo("entry"));
    }

    [Test]
    public void TryAddEntry_EntryAlreadyExists_ReturnsFalse()
    {
        var container = new InjectionContainer();

        var first = new Mock<InjectionEntry>();
        var second = new Mock<InjectionEntry>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.TryAddEntry(typeof(string), null, first.Object), Is.True);
            Assert.That(container.TryAddEntry(typeof(string), null, second.Object), Is.False);
            Assert.That(container.GetEntry(typeof(string)), Is.SameAs(first.Object));
        }
    }

    [Test]
    public void Entries_EntryIsUnkeyed_ReturnsTupleWithNullKey()
    {
        var container = new InjectionContainer();
        var entry = new Mock<InjectionEntry>();
        entry.Setup(target => target.IsAssignableTo(typeof(string))).Returns(true);
        container.AddEntry(typeof(string), null, entry.Object);

        var tuple = container.Entries.Single(target => target.Type == typeof(string));
        
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tuple.Key, Is.Null);
            Assert.That(tuple.Entry, Is.SameAs(entry.Object));
        }
    }

    [Test]
    public void AddEntry_EntryIsOpenGeneric_ResolvesClosedGenericRequest()
    {
        var container = new InjectionContainer();
        var entry = new Mock<InjectionEntry>();
        entry.Setup(target => target.IsAssignableTo(typeof(IEnumerable<>))).Returns(true);

        container.AddEntry(typeof(IEnumerable<>), null, entry.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(container.HasEntry(typeof(IEnumerable<int>)), Is.True);
            Assert.That(container.GetEntry(typeof(IEnumerable<int>)), Is.SameAs(entry.Object));
            Assert.That(container.RemoveEntry(typeof(IEnumerable<>)), Is.True);
            Assert.That(container.HasEntry(typeof(IEnumerable<int>)), Is.False);
        }
    }

    [Test]
    public void Entries_ContainerHasOpenGenericEntries_IncludesEntriesAndClears()
    {
        var container = new InjectionContainer();
        var entry = new Mock<InjectionEntry>();
        entry.Setup(target => target.IsAssignableTo(typeof(IList<>))).Returns(true);

        container.AddEntry(typeof(IList<>), null, entry.Object);

        var tuple = container.Entries.Single(target => target.Type == typeof(IList<>));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(tuple.Key, Is.Null);
            Assert.That(tuple.Entry, Is.SameAs(entry.Object));
        }

        container.ClearEntries();
        Assert.That(container.HasEntry(typeof(IList<string>)), Is.False);
    }

    public class TestGenericBase<T1, T2>
    {
    }

    public class TestGenericType<TA, TB> : TestGenericBase<TB, TA>
    {
    }
    
    [Test]
    public void GetInjection_GenericParameterOrderDiffers_ResolvesCorrectOrder()
    {
        var container = new InjectionContainer();
        container.AddSingleton(typeof(TestGenericBase<,>), typeof(TestGenericType<,>));

        var instance = container.GetInjection(typeof(TestGenericBase<int, bool>));
        
        Assert.That(instance, Is.TypeOf<TestGenericType<bool, int>>());
    }
}