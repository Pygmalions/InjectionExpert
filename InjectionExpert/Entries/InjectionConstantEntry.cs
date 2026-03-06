using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Constant={Value}")]
public class InjectionConstantEntry(object value) : InjectionEntry
{
    public override InjectionLifespan Lifespan => InjectionLifespan.Singleton;

    public object Value { get; } = value;

    public override bool IsAssignableTo(Type type)
        => Value.GetType().IsAssignableTo(type);

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
        => Value;

    public override string ToString() => $"(Constant, {Value})";
}