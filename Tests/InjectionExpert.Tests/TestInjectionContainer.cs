namespace InjectionExpert.Tests;

[TestFixture, TestOf(typeof(InjectionContainer))]
public class TestInjectionContainer
{
    public interface IStubGenericInterface<T1, T2>
    {}
    
    public class StubGenericType<T2, T1> : IStubGenericInterface<T1, T2>
    {
        public T2 Member2 = default!;
        
        public T1 Member1 = default!;
    }
    
    
    [Test]
    public void GenericDefinition_ByType_Transient()
    {
        var container = new InjectionContainer();
        container.AddTransient(typeof(IStubGenericInterface<,>), typeof(StubGenericType<,>));
        
        var instance = container.GetInjection<IStubGenericInterface<int, long>>();
        
        Assert.That(instance, Is.Not.Null);
    }
}