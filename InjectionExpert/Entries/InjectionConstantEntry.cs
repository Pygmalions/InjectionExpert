using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Constant={Value}")]
public class InjectionConstantEntry(object value) : InjectionEntry(InjectionLifespan.Singleton)
{
    public object Value { get; } = value;

    public override object GetInjection(Type type, object? key, InjectionTarget target) => Value;

    public override string ToString() => $"(Constant, {Value})";
}