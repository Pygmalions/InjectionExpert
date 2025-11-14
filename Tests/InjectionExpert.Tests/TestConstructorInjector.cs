using System.Diagnostics.CodeAnalysis;
using InjectionExpert.Injectors;
using JetBrains.Annotations;

namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(ConstructorInjector))]
public class TestConstructorInjector
{
    [Test]
    public void Create_Injector_NotNull()
    {
        var injector = ConstructorInjector.For(typeof(StubOneConstructorInjectionClass));
        Assert.That(injector, Is.Not.Null);
    }

    private class StubOneConstructorInjectionClass(string text, int number, double value)
    {
        public readonly int Number = number;
        public readonly string Text = text;
        public readonly double Value = value;
    }
    
    [Test]
    public void Create_Class_OneConstructor()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5)
            .AddSingleton("Sample");

        var succeeded = ConstructorInjector
            .For(typeof(StubOneConstructorInjectionClass))
            .TryInject(out var instance, container);

        var target = (StubOneConstructorInjectionClass?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target, Is.Not.Null);
        }
        using (Assert.EnterMultipleScope())
        {
            Assert.That(target.Text, Is.EqualTo("Sample"));
            Assert.That(target.Number, Is.EqualTo(1));
            Assert.That(target.Value, Is.EqualTo(0.5));
        }
    }

    private class StubTwoConstructorInjectionClass(string text, int number, int value)
    {
        public readonly int Number = number;

        public readonly string Text = text;

        public readonly double Value = value;

        [UsedImplicitly]
        public StubTwoConstructorInjectionClass(int number, int value1, int value2, int value3) :
            this(string.Empty, number, value1 + value2 + value3)
        {
        }
    }

    [Test]
    public void Create_Class_TwoConstructor()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5);
        var succeeded = ConstructorInjector
            .For(typeof(StubTwoConstructorInjectionClass))
            .TryInject(out var instance, container);
        var target = (StubTwoConstructorInjectionClass?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target, Is.Not.Null);
        }
        using (Assert.EnterMultipleScope())
        {
            Assert.That(target.Text, Is.EqualTo(string.Empty));
            Assert.That(target.Number, Is.EqualTo(1));
            Assert.That(target.Value, Is.EqualTo(3));
        }
    }

    [Test]
    public void Create_Class_Unsatisfied_Null()
    {
        var container = new InjectionContainer()
            .AddSingleton(0.5);
        var succeeded = ConstructorInjector
            .For(typeof(StubOneConstructorInjectionClass))
            .TryInject(out var instance, container);

        var target = (StubOneConstructorInjectionClass?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.False);
            Assert.That(target, Is.Null);
        }
    }

    private struct StubOneConstructorInjectionStruct(string text, int number, double value)
    {
        public readonly string Text = text;
        public readonly int Number = number;
        public readonly double Value = value;
    }

    [Test]
    public void Create_Struct_OneConstructor()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5)
            .AddSingleton("Sample");
        var succeeded = ConstructorInjector
            .For(typeof(StubOneConstructorInjectionStruct))
            .TryInject(out var instance, container);

        var target = (StubOneConstructorInjectionStruct)instance!;

        Assert.That(succeeded, Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(target.Text, Is.EqualTo("Sample"));
            Assert.That(target.Number, Is.EqualTo(1));
            Assert.That(target.Value, Is.EqualTo(0.5));
        }
    }

    private struct StubTwoConstructorInjectionStruct(string text, int number, int value)
    {
        public readonly string Text = text;
        public readonly int Number = number;
        public readonly double Value = value;

        // ReSharper disable once UnusedMember.Local
        public StubTwoConstructorInjectionStruct(int number, int value1, int value2, int value3) :
            this(string.Empty, number, value1 + value2 + value3)
        {
        }
    }

    [Test]
    public void Create_Struct_TwoConstructor()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5);
        var succeeded = ConstructorInjector
            .For(typeof(StubTwoConstructorInjectionStruct))
            .TryInject(out var instance, container);

        var target = (StubTwoConstructorInjectionStruct)instance!;

        Assert.That(succeeded, Is.True);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(target.Text, Is.EqualTo(string.Empty));
            Assert.That(target.Number, Is.EqualTo(1));
            Assert.That(target.Value, Is.EqualTo(3));
        }
    }

    [Test]
    public void Create_Struct_Unsatisfied_Null()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5);
        var succeeded = ConstructorInjector
            .For(typeof(StubOneConstructorInjectionStruct))
            .TryInject(out var instance, container);

        var target = (StubOneConstructorInjectionStruct?)instance!;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.False);
            Assert.That(target, Is.Null);
        }
    }

    [DynamicallyAccessedMembers(
        DynamicallyAccessedMemberTypes.PublicFields |
        DynamicallyAccessedMemberTypes.PublicProperties)]
    private class StubWithDefaultArguments(int argument1, double argument2 = double.Pi)
    {
        public double Floating = argument2;
        public int Integer = argument1;
    }

    [Test]
    public void Injector_WithDefaultArguments()
    {
        var container = new InjectionContainer()
            .AddSingleton(1);
        var succeeded = ConstructorInjector
            .For(typeof(StubWithDefaultArguments))
            .TryInject(out var instance, container);
        
        var target = (StubWithDefaultArguments?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target?.Integer, Is.EqualTo(1));
            Assert.That(target?.Floating, Is.EqualTo(double.Pi));
        }
    }

    [Test]
    public void Injector_WithDefaultArguments_ShouldFail()
    {
        var container = new InjectionContainer();
        var succeeded = ConstructorInjector
            .For(typeof(StubOneConstructorInjectionClass))
            .TryInject(out _, container);
        Assert.That(succeeded, Is.False);
    }

    [Test]
    public void Injector_OverwriteDefaultArguments()
    {
        var container = new InjectionContainer()
            .AddSingleton(1)
            .AddSingleton(0.5);
        var succeeded = ConstructorInjector
            .For(typeof(StubWithDefaultArguments))
            .TryInject(out var instance, container);

        var target = (StubWithDefaultArguments?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target?.Integer, Is.EqualTo(1));
            Assert.That(target?.Floating, Is.EqualTo(0.5));
        }
    }

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields |
                                DynamicallyAccessedMemberTypes.PublicProperties)]
    private class StubWithDecimalDefaultArguments(int argument1, decimal argument2 = decimal.One)
    {
        public decimal Floating = argument2;
        public int Integer = argument1;
    }

    [Test]
    public void Injector_DefaultDecimalArgument()
    {
        var container = new InjectionContainer()
            .AddSingleton(1);
        var succeeded = ConstructorInjector
            .For(typeof(StubWithDecimalDefaultArguments))
            .TryInject(out var instance, container);

        var target = (StubWithDecimalDefaultArguments?)instance;
        
        Assert.That(succeeded, Is.True);
        Assert.That(target?.Integer, Is.EqualTo(1));
        Assert.That(target?.Floating, Is.EqualTo(decimal.One));
    }

    private class StubWithKeys([Injection(Key = 1)] string key1, [Injection(Key = 2)] string key2)
    {
        public readonly string Key1 = key1;
        public readonly string Key2 = key2;
    }

    [Test]
    public void Injector_WithKeys()
    {
        var container = new InjectionContainer()
            .AddSingleton("Value1", 1)
            .AddSingleton("Value2", 2);
        var succeeded = ConstructorInjector
            .For(typeof(StubWithKeys))
            .TryInject(out var instance, container);

        var target = (StubWithKeys?)instance;

        using (Assert.EnterMultipleScope())
        {
            Assert.That(succeeded, Is.True);
            Assert.That(target?.Key1, Is.EqualTo("Value1"));
            Assert.That(target?.Key2, Is.EqualTo("Value2"));
        }
    }
}