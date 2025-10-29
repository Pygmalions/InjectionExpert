using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Redirection=({TargetType}, {TargetKey})")]
public class InjectionRedirectionEntry(IInjectionProvider provider, Type targetType, object? targetKey)
    : InjectionEntry(InjectionLifespan.Transient)
{
    public Type TargetType { get; } = targetType;

    public object? TargetKey { get; } = targetKey;

    public override object GetInjection(Type type, object? key, InjectionTarget target)
        => provider.GetInjectionItem(TargetType, TargetKey, target)!;
    
    public override string ToString() => $"(Redirection, Type: {TargetType}, Key: {TargetKey})";
}