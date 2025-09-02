using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using InjectionExpert.Injectors;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(MemberInjector))]
public class TestMemberInjectorGenerator
{
    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicProperties)]
    private class StubMemberInjectionTarget
    {
        [Injection] public double DoubleField = 0;
        [Injection] public long LongField = 0;
        [Injection] public int NumberField = 0;
        [Injection] public string StringField = "";
        [Injection] public int NumberProperty { get; set; } = 0;
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

        Assert.Multiple(() =>
        {
            Assert.That(sample.NumberField, Is.EqualTo(1));
            Assert.That(sample.DoubleField, Is.EqualTo(1.0));
            Assert.That(sample.StringField, Is.EqualTo("Sample"));
            Assert.That(sample.LongField, Is.EqualTo(2));
        });
    }

    private class StubMemberInjectionTargetWithRequired
    {
        [Injection] public required double DoubleField;
        [Injection] public long LongField = 0;
        [Injection] public int NumberField = 0;
        [Injection] public string StringField = "";
        [Injection] public int NumberProperty { get; set; } = 0;
    }

    [Test]
    public void Injector_InjectFailed()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(2L)
            .AddSingleton("Sample");
        var sample = new StubMemberInjectionTargetWithRequired()
        {
            DoubleField = 1.0
        };
        var succeeded = MemberInjector
            .For(typeof(StubMemberInjectionTargetWithRequired))
            .TryInject(sample, container, out var missing);

        Assert.Multiple(() =>
        {
            Assert.That(succeeded, Is.False);
            Assert.That(missing.Instance, Is.EqualTo(sample));
        });
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

        Assert.Multiple(() => { Assert.That(sample.NumberProperty, Is.EqualTo(1)); });
    }

    private class StubWithObjects
    {
        [Injection] public StrongBox<int>? ObjectField = null!;
        [Injection] public StrongBox<int>? ObjectProperty { get; set; } = null!;
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

        Assert.Multiple(() =>
        {
            Assert.That(sample.ObjectField?.Value, Is.EqualTo(0));
            Assert.That(sample.ObjectProperty?.Value, Is.EqualTo(1));
        });
    }
    
    private class StubWithNullable
    {
        [Injection] public int? IntegerField = null!;
        
        [Injection] public double? DoubleProperty { get; set; } = null!;
    }

    [Test]
    public void Injector_InjectOnlyNull_NullableTypes()
    {
        var container = new InjectionContainer()
            .AddSingleton<int?>(1)
            .AddSingleton<double?>(1.0);
        
        var sample = new StubWithNullable()
        {
            IntegerField = 2
        };
        
        MemberInjector
            .For(typeof(StubWithNullable))
            .TryInject(sample, container, out _, true);
        
        Assert.Multiple(() =>
        {
            Assert.That(sample.IntegerField.Value, Is.EqualTo(2));
            Assert.That(sample.DoubleProperty, Is.EqualTo(1.0));
        });
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

        Assert.Multiple(() =>
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Key1, Is.EqualTo("Value1"));
            Assert.That(target.Key2, Is.EqualTo("Value2"));
        });
    }
    
    private class StubWithIgnoredMembers
    {
        [Injection] 
        public required int Member1;
        [Injection(false)]
        public required int Member2;
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

        Assert.Multiple(() =>
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Member1, Is.EqualTo(3));
            Assert.That(target.Member2, Is.EqualTo(2));
        });
    }

    private class StubWithInitOnlyMembers
    {
        public required int Member1 { get; init; }
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
        
        Assert.Multiple(() =>
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target.Member1, Is.EqualTo(3));
            Assert.That(target.Member2, Is.EqualTo(3));
        });
    }
}