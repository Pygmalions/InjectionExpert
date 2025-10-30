using InjectionExpert.Entries;
using JetBrains.Annotations;

namespace InjectionExpert.Tests.Entries;

[TestFixture, TestOf(typeof(InjectionTypeDefinitionEntry))]
public class TestInjectionTypeDefinitionEntry
{
    [UsedImplicitly]
    public interface IStubGenericInterface<T1, T2>
    {}
    
    /// <summary>
    /// This class is to test the ability of the container to resolve generic definitions,
    /// especially to rearrange the generic type parameters.
    /// </summary>
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class StubGenericType<T2, T1> : IStubGenericInterface<T1, T2>
    {
        public T2 Member2 = default!;
        public T1 Member1 = default!;
    }
    
    [Test]
    public void GenericDefinition_ShuffledParameters_ShouldMatch()
    {
        var container = new InjectionContainer();
        container.AddTransient(typeof(IStubGenericInterface<,>), typeof(StubGenericType<,>));
        
        var instance = container.GetInjection(typeof(IStubGenericInterface<int, long>));
        
        Assert.That(instance, Is.Not.Null);
        Assert.That(instance, Is.InstanceOf<IStubGenericInterface<int, long>>());
    }
}