using Moq;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(InjectionScope))]
public class TestInjectionScope
{
    [Test]
    public void GetInjection_EntryNotFound_ReturnsNull()
    {
        var provider = new Mock<IInjectionProvider>();
        provider.Setup(target => target.GetEntry(typeof(string), It.IsAny<object?>()))
            .Returns((InjectionEntry?)null);
        var scope = new InjectionScope(provider.Object);

        var injection = scope.GetInjection(typeof(string));

        Assert.That(injection, Is.Null);
    }

    [Test]
    public void HasEntry_ProviderContainsEntry_ReturnsExpected()
    {
        var provider = new Mock<IInjectionProvider>();
        provider.Setup(target => target.HasEntry(typeof(string), null)).Returns(true);
        provider.Setup(target => target.HasEntry(typeof(int), null)).Returns(false);
        var scope = new InjectionScope(provider.Object);

        Assert.That(scope.HasEntry(typeof(string)), Is.True);
        Assert.That(scope.HasEntry(typeof(int)), Is.False);
        provider.Verify(target => target.HasEntry(typeof(string), null), Times.Once);
        provider.Verify(target => target.HasEntry(typeof(int), null), Times.Once);
    }

    [Test]
    public void GetInjection_EntryIsScoped_ReusesInstance()
    {
        var provider = new Mock<IInjectionProvider>();
        var createCount = 0;
        var entry = new Mock<InjectionEntry>();
        entry.SetupGet(target => target.Lifespan).Returns(InjectionLifespan.Scoped);
        entry.Setup(target => target.GetInjection(It.IsAny<IInjectionProvider>(), typeof(string), It.IsAny<object?>(), default))
            .Returns(() => $"v-{++createCount}");
        provider.Setup(target => target.GetEntry(typeof(string), It.IsAny<object?>()))
            .Returns(entry.Object);
        var scope = new InjectionScope(provider.Object);

        var first = scope.GetInjection(typeof(string));
        var second = scope.GetInjection(typeof(string));

        Assert.That(first, Is.EqualTo("v-1"));
        Assert.That(second, Is.SameAs(first));
        Assert.That(createCount, Is.EqualTo(1));
        provider.Verify(target => target.GetEntry(typeof(string), It.IsAny<object?>()), Times.Once);
    }

    [Test]
    public void GetInjection_CircularDependencyDetected_ThrowsInjectionFailureException()
    {
        var provider = new Mock<IInjectionProvider>();
        var entry = new Mock<InjectionEntry>();
        entry.SetupGet(target => target.Lifespan).Returns(InjectionLifespan.Transient);
        entry.Setup(target => target.GetInjection(It.IsAny<IInjectionProvider>(), typeof(string), It.IsAny<object?>(), default))
            .Returns((IInjectionProvider injectionProvider, Type _, object? _, InjectionTarget _) =>
                injectionProvider.GetInjection(typeof(string))!);
        provider.Setup(target => target.GetEntry(typeof(string), It.IsAny<object?>()))
            .Returns(entry.Object);
        var scope = new InjectionScope(provider.Object);

        Assert.Throws<InjectionFailureException>(() => scope.GetInjection(typeof(string)));
    }

    [Test]
    public async Task DisposeAsync_CalledAfterTrackingDisposables_DisposesInReverseOrderAndThrowsOnSecondCall()
    {
        var disposeLog = new List<string>();
        var provider = new Mock<IInjectionProvider>();
        var syncEntry = new Mock<InjectionEntry>();
        syncEntry.SetupGet(target => target.Lifespan).Returns(InjectionLifespan.Transient);
        syncEntry.Setup(target => target.GetInjection(It.IsAny<IInjectionProvider>(), typeof(object), "sync", default))
            .Returns(new SyncDisposable("sync", disposeLog));
        var asyncEntry = new Mock<InjectionEntry>();
        asyncEntry.SetupGet(target => target.Lifespan).Returns(InjectionLifespan.Transient);
        asyncEntry.Setup(target => target.GetInjection(It.IsAny<IInjectionProvider>(), typeof(object), "async", default))
            .Returns(new AsyncDisposable("async", disposeLog));

        provider.Setup(target => target.GetEntry(typeof(object), "sync")).Returns(syncEntry.Object);
        provider.Setup(target => target.GetEntry(typeof(object), "async")).Returns(asyncEntry.Object);

        var scope = new InjectionScope(provider.Object);

        _ = scope.GetInjection(typeof(object), "sync");
        _ = scope.GetInjection(typeof(object), "async");

        await scope.DisposeAsync();

        Assert.That(disposeLog, Is.EqualTo(new[] { "async", "sync" }));
        Assert.ThrowsAsync<ObjectDisposedException>(async () => await scope.DisposeAsync());
    }

    private sealed class SyncDisposable(string id, List<string> log) : IDisposable
    {
        public void Dispose() => log.Add(id);
    }

    private sealed class AsyncDisposable(string id, List<string> log) : IAsyncDisposable
    {
        public ValueTask DisposeAsync()
        {
            log.Add(id);
            return ValueTask.CompletedTask;
        }
    }
}