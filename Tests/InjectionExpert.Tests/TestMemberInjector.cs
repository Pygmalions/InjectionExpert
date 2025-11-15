using System.Runtime.CompilerServices;
using System.Reflection;
using InjectionExpert.Injectors;
using JetBrains.Annotations;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(MemberInjector))]
public class TestMemberInjector
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private class StubMemberInjectionTarget
    {
        [Injection] public double DoubleField;
        [Injection] public long LongField;
        [Injection] public int NumberField;
        [Injection] public string StringField = "";
        [Injection] public int NumberProperty { get; set; }
    }

    [Test]
    public void Injector_Create_NotNull()
    {
        var injector = MemberInjector.For(typeof(StubMemberInjectionTarget));
        Assert.That(injector, Is.Not.Null);
    }

    [Test]
    public void Injector_InjectFields()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(2L)
            .AddSingleton(1.0)
            .AddSingleton("Sample");
        var sample = new StubMemberInjectionTarget();
        MemberInjector
            .For(typeof(StubMemberInjectionTarget))
            .TryInject(sample, container, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sample.NumberField, Is.EqualTo(1));
            Assert.That(sample.DoubleField, Is.EqualTo(1.0));
            Assert.That(sample.StringField, Is.EqualTo("Sample"));
            Assert.That(sample.LongField, Is.EqualTo(2));
        }
    }

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    private class StubMemberInjectionTargetWithRequired
    {
        [Injection] public required double DoubleField;
        [Injection] public long LongField;
        [Injection] public int NumberField;
        [Injection] public string StringField = "";
        [Injection] public int NumberProperty { get; set; }
    }

    [Test]
    public void Injector_MissingRequired_AbortedWithMissingTarget()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton((long)2)
            .AddSingleton("Sample");
        var sample = new StubMemberInjectionTargetWithRequired()
        {
            DoubleField = 1.0
        };
        var succeeded = MemberInjector
            .For(typeof(StubMemberInjectionTargetWithRequired))
            .TryInject(sample, container, out var missing);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.False);
            Assert.That(missing.OwnerInstance, Is.EqualTo(sample));
        }
    }

    [Test]
    public void Injector_InjectProperties()
    {
        var container = new InjectionContainer()
            .AddSingleton(1);
        var sample = new StubMemberInjectionTarget();
        MemberInjector
            .For(typeof(StubMemberInjectionTarget))
            .TryInject(sample, container, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sample.NumberProperty, Is.EqualTo(1));
        }
    }

    private class StubWithObjects
    {
        [Injection] public StrongBox<int>? ObjectField;
        [Injection] public StrongBox<int>? ObjectProperty { get; set; }
    }

    [Test]
    public void Injector_InjectOnlyNull_ReferenceTypes()
    {
        var container = new InjectionContainer()
            .AddSingleton(new StrongBox<int>(1));
        var sample = new StubWithObjects()
        {
            ObjectField = new StrongBox<int>(0),
        };

        MemberInjector
            .For(typeof(StubWithObjects))
            .TryInject(sample, container, out _, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sample.ObjectField?.Value, Is.EqualTo(0));
            Assert.That(sample.ObjectProperty?.Value, Is.EqualTo(1));
        }
    }

    private class StubWithNullable
    {
        [Injection] public int? IntegerField;

        [Injection] public double? DoubleProperty { get; set; }
    }

    [Test]
    public void Injector_InjectOnlyNull_NullableTypes()
    {
        var container = new InjectionContainer()
            .AddSingleton((int?)1)
            .AddSingleton((double?)1.0);

        var sample = new StubWithNullable()
        {
            IntegerField = 2
        };

        MemberInjector
            .For(typeof(StubWithNullable))
            .TryInject(sample, container, out _, true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(sample.IntegerField.Value, Is.EqualTo(2));
            Assert.That(sample.DoubleProperty, Is.EqualTo(1.0));
        }
    }

    private class StubWithKeys
    {
        [Injection(Key = 1)] public string Key1 = null!;
        [Injection(Key = 2)] public string Key2 = null!;
    }

    [Test]
    public void Injector_WithKeys()
    {
        var container = new InjectionContainer()
            .AddSingleton("Value1", 1)
            .AddSingleton("Value2", 2);
        var target = new StubWithKeys();
        var succeeded = MemberInjector
            .For(typeof(StubWithKeys))
            .TryInject(target, container, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Key1, Is.EqualTo("Value1"));
            Assert.That(target.Key2, Is.EqualTo("Value2"));
        }
    }

    private class StubWithIgnoredMembers
    {
        [Injection] public required int Member1;
        [Injection(false)] public required int Member2;
    }

    [Test]
    public void Injector_WithIgnoredMembers()
    {
        var container = new InjectionContainer()
            .AddSingleton(3);

        var target = new StubWithIgnoredMembers()
        {
            Member1 = 1,
            Member2 = 2
        };
        var succeeded = MemberInjector
            .For(typeof(StubWithIgnoredMembers))
            .TryInject(target, container, out _);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Member1, Is.EqualTo(3));
            Assert.That(target.Member2, Is.EqualTo(2));
        }
    }
    
    private class StubWithInitOnlyMembers
    {
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public required int Member1 { get; init; }
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public required int Member2 { get; init; }
    }

    [Test]
    public void Injector_WithRequiredInitMembers()
    {
        var container = new InjectionContainer()
            .AddSingleton(3);

        var instance = RuntimeHelpers.GetUninitializedObject(typeof(StubWithInitOnlyMembers));

        var succeeded = MemberInjector
            .For(typeof(StubWithInitOnlyMembers))
            .TryInject(instance, container, out _);

        var target = (StubWithInitOnlyMembers)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Member1, Is.EqualTo(3));
            Assert.That(target.Member2, Is.EqualTo(3));
        }
    }

    [Test]
    public void TryUpdate_NoMatchingDependency_ReturnsFalse_And_NoChange()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton("orig");
        var sample = new StubMemberInjectionTarget();
        MemberInjector.For(typeof(StubMemberInjectionTarget)).TryInject(sample, container, out _);

        // No DateTime dependency exists
        var updated = MemberInjector.For(typeof(StubMemberInjectionTarget))
            .TryUpdate(sample, typeof(DateTime), null, DateTime.Now);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.False);
            Assert.That(sample.NumberField, Is.EqualTo(1));
            Assert.That(sample.NumberProperty, Is.EqualTo(1));
            Assert.That(sample.StringField, Is.EqualTo("orig"));
        }
    }

    [Test]
    public void TryUpdate_UpdateFieldAndProperty()
    {
        var sample = new StubMemberInjectionTarget
        {
            // Initially different values
            NumberField = 5,
            NumberProperty = 6
        };

        var updated = MemberInjector.For(typeof(StubMemberInjectionTarget))
            .TryUpdate(sample, typeof(int), null, 42);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.True);
            Assert.That(sample.NumberField, Is.EqualTo(42));
            Assert.That(sample.NumberProperty, Is.EqualTo(42));
        }
    }

    [Test]
    public void TryUpdate_OnlyNullMembers_ForReferenceTypes()
    {
        var sample = new StubWithObjects
        {
            ObjectField = new StrongBox<int>(0),
            ObjectProperty = null
        };

        var newBox = new StrongBox<int>(9);
        var updated = MemberInjector.For(typeof(StubWithObjects))
            .TryUpdate(sample, typeof(StrongBox<int>), null, newBox, onlyNullMembers: true);

        using (Assert.EnterMultipleScope())
        {
            // Field unchanged because it's not null
            Assert.That(sample.ObjectField?.Value, Is.EqualTo(0));
            // Property set because it was null
            Assert.That(sample.ObjectProperty?.Value, Is.EqualTo(9));
            Assert.That(updated, Is.True);
        }
    }

    [Test]
    public void TryUpdate_OnlyNullMembers_ForNullableValueTypes()
    {
        var sample = new StubWithNullable
        {
            IntegerField = 2,
            DoubleProperty = null
        };

        var updatedInt = MemberInjector.For(typeof(StubWithNullable))
            .TryUpdate(sample, typeof(int?), null, (int?)5, onlyNullMembers: true);
        var updatedDouble = MemberInjector.For(typeof(StubWithNullable))
            .TryUpdate(sample, typeof(double?), null, (double?)7.5, onlyNullMembers: true);

        using (Assert.EnterMultipleScope())
        {
            // int? field stays 2 because not null and onlyNullMembers=true
            Assert.That(sample.IntegerField, Is.EqualTo(2));
            // double? property becomes 7.5 because it was null
            Assert.That(sample.DoubleProperty, Is.EqualTo(7.5));
            // Overall, at least one update occurred
            Assert.That(updatedInt || updatedDouble, Is.True);
        }
    }

    [Test]
    public void TryUpdate_WithKeys_UpdatesOnlyMatchingKey()
    {
        var target = new StubWithKeys
        {
            Key1 = "x",
            Key2 = "y"
        };

        var updatedKey1 = MemberInjector.For(typeof(StubWithKeys))
            .TryUpdate(target, typeof(string), 1, "A");
        var updatedKey2 = MemberInjector.For(typeof(StubWithKeys))
            .TryUpdate(target, typeof(string), 2, "B");
        var updatedWrongKey = MemberInjector.For(typeof(StubWithKeys))
            .TryUpdate(target, typeof(string), 3, "C");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updatedKey1, Is.True);
            Assert.That(updatedKey2, Is.True);
            Assert.That(updatedWrongKey, Is.False);
            Assert.That(target.Key1, Is.EqualTo("A"));
            Assert.That(target.Key2, Is.EqualTo("B"));
        }
    }

    [Test]
    public void TryUpdate_OnlyNullMembers_NoActualUpdate_ReturnsFalse()
    {
        var target = new StubWithObjects
        {
            ObjectField = new StrongBox<int>(1),
            ObjectProperty = new StrongBox<int>(2)
        };

        var updated = MemberInjector.For(typeof(StubWithObjects))
            .TryUpdate(target, typeof(StrongBox<int>), null, new StrongBox<int>(3), onlyNullMembers: true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated, Is.False);
            Assert.That(target.ObjectField?.Value, Is.EqualTo(1));
            Assert.That(target.ObjectProperty?.Value, Is.EqualTo(2));
        }
    }

    [Test]
    public void Dependencies_BasicAttributedMembers()
    {
        var injector = MemberInjector.For(typeof(StubMemberInjectionTarget));
        var deps = injector.Dependencies.ToList();

        Assert.That(deps, Has.Count.EqualTo(5));

        // Expect names and types
        var expected = new (string Name, Type Type)[]
        {
            ("DoubleField", typeof(double)),
            ("LongField", typeof(long)),
            ("NumberField", typeof(int)),
            ("StringField", typeof(string)),
            ("NumberProperty", typeof(int))
        };

        foreach (var (name, type) in expected)
        {
            Assert.That(deps.Any(d => d.Type == type && d.Key == null && d.Member.Name == name),
                Is.True, $"Missing dependency for {name}:{type.Name}");
        }
    }

    [Test]
    public void Dependencies_WithKeys_AreReported()
    {
        var injector = MemberInjector.For(typeof(StubWithKeys));
        var deps = injector.Dependencies.ToList();

        Assert.That(deps.Count, Is.EqualTo(2));
        using (Assert.EnterMultipleScope())
        {
            Assert.That(deps.Any(d => d.Type == typeof(string) && Equals(d.Key, 1) && d.Member.Name == "Key1"), Is.True);
            Assert.That(deps.Any(d => d.Type == typeof(string) && Equals(d.Key, 2) && d.Member.Name == "Key2"), Is.True);
        }
    }

    [Test]
    public void Dependencies_Ignores_DisabledAttributedMembers()
    {
        var injector = MemberInjector.For(typeof(StubWithIgnoredMembers));
        var deps = injector.Dependencies.ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(deps.Any(d => d.Member.Name == "Member1" && d.Type == typeof(int)), Is.True);
            Assert.That(deps.Any(d => d.Member.Name == "Member2"), Is.False);
        }
    }

    [Test]
    public void Dependencies_Include_RequiredInitMembers()
    {
        var injector = MemberInjector.For(typeof(StubWithInitOnlyMembers));
        var deps = injector.Dependencies.ToList();

        using (Assert.EnterMultipleScope())
        {
            // Two required properties without [Injection] should still be dependencies
            Assert.That(deps.Count(d => d.Type == typeof(int) && (d.Member.Name == "Member1" || d.Member.Name == "Member2")), Is.EqualTo(2));
            Assert.That(deps.All(d => d.Key == null), Is.True);
        }

        // Verify the members are properties
        foreach (var dep in deps)
        {
            Assert.That(dep.Member.MemberType, Is.EqualTo(MemberTypes.Property));
        }
    }
}