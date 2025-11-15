namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(InjectionScope))]
public class TestInjectionScope
{
    private sealed class Dummy
    {
    }

    [Test]
    public void Scoped_CachedWithinSameScope()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Scoped, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var scope = container.NewScope(new InjectionTarget(typeof(Dummy)));
        var aItem = scope.GetInjectionItem(typeof(object))!;
        var bItem = scope.GetInjectionItem(typeof(object))!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(aItem?.Instance, Is.SameAs(bItem?.Instance));
            Assert.That(created, Is.EqualTo(1));
        }
    }

    [Test]
    public void ChildScope_SeesParentsScopedInstance()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Scoped, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var parent = container.NewScope(new InjectionTarget(typeof(Dummy)));
        var parentObj = parent.GetInjection<object>();

        using var child = parent.NewScope(new InjectionTarget(typeof(Dummy)));
        var childObj = child.GetInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childObj, Is.SameAs(parentObj));
            Assert.That(created, Is.EqualTo(1));
            Assert.That(child.Parent, Is.SameAs(parent));
        }
    }

    [Test]
    public void ParentScope_DoesNotSeeChildrenScopedInstance()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Scoped, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var parent = container.NewScope(new InjectionTarget(typeof(Dummy)));
        using var child = parent.NewScope(new InjectionTarget(typeof(Dummy)));

        var childObj = child.RequireInjection<object>();
        var parentObj = parent.RequireInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(childObj, Is.Not.SameAs(parentObj));
            Assert.That(created, Is.EqualTo(2));
        }
    }

    [Test]
    public void SiblingScopes_WhenParentNotCached_AreIsolated()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Scoped, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var parent = container.NewScope(new InjectionTarget(typeof(Dummy)));
        using var childA = parent.NewScope(new InjectionTarget(typeof(Dummy)));
        using var childB = parent.NewScope(new InjectionTarget(typeof(Dummy)));

        var a = childA.RequireInjection<object>();
        var b = childB.RequireInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.Not.SameAs(b));
            Assert.That(created, Is.EqualTo(2));
        }
    }

    [Test]
    public void SiblingScopes_WhenParentCached_Share()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Scoped, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var parent = container.NewScope(new InjectionTarget(typeof(Dummy)));
        var p = parent.RequireInjection<object>();
        using var childA = parent.NewScope(new InjectionTarget(typeof(Dummy)));
        using var childB = parent.NewScope(new InjectionTarget(typeof(Dummy)));

        var a = childA.RequireInjection<object>();
        var b = childB.RequireInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.SameAs(b));
            Assert.That(a, Is.SameAs(p));
            Assert.That(created, Is.EqualTo(1));
        }
    }

    [Test]
    public void Transient_NotCached()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Transient, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var scope = container.NewScope(new InjectionTarget(typeof(Dummy)));
        var a = scope.RequireInjection<object>();
        var b = scope.RequireInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.Not.SameAs(b));
            Assert.That(created, Is.EqualTo(2));
        }
    }

    [Test]
    public void Singleton_Behavior_UnchangedAcrossScopes()
    {
        var container = new InjectionContainer();
        int created = 0;
        container.AddInjection(InjectionLifespan.Singleton, typeof(object), (_, _, _, _) =>
        {
            created++;
            return new object();
        });

        using var scope1 = container.NewScope(new InjectionTarget(typeof(Dummy)));
        using var scope2 = container.NewScope(new InjectionTarget(typeof(Dummy)));

        var a = scope1.RequireInjection<object>();
        var b = scope1.RequireInjection<object>();
        var c = scope2.RequireInjection<object>();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(a, Is.SameAs(b));
            Assert.That(a, Is.SameAs(c));
            Assert.That(created, Is.EqualTo(1));
        }
    }

    [Test]
    public void Properties_TargetAndParent_AreSet()
    {
        var container = new InjectionContainer();
        var target = new InjectionTarget(typeof(Dummy));
        using var scope = container.NewScope(target);
        Assert.That(scope.Target, Is.EqualTo(target));
        Assert.That(scope.Parent, Is.Null);

        var childTarget = new InjectionTarget(typeof(string));
        using var child = scope.NewScope(childTarget);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(child.Target, Is.EqualTo(childTarget));
            Assert.That(child.Parent, Is.SameAs(scope));
        }
    }

    [Test]
    public void Disposed_GetAndNewScope_Throws()
    {
        var container = new InjectionContainer();
        var scope = container.NewScope(new InjectionTarget(typeof(Dummy)));
        scope.Dispose();

        using (Assert.EnterMultipleScope())
        {
            Assert.Throws<ObjectDisposedException>(() => scope.GetInjectionItem(typeof(object)));
            Assert.Throws<ObjectDisposedException>(() => scope.NewScope());
        }
    }
}