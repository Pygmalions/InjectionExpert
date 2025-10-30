using System.Diagnostics;

namespace InjectionExpert.Entries;

[DebuggerDisplay("Redirection=({TargetType}, {TargetKey})")]
public class InjectionRedirectionEntry(Type targetType, object? targetKey)
    : InjectionEntry(InjectionLifespan.Transient)
{
    public Type TargetType { get; } = targetType;

    public object? TargetKey { get; } = targetKey;

    public override object GetInjection(IInjectionProvider provider, Type type, object? key, InjectionTarget target)
        => GetInjection(provider, target);
    
    public object GetInjection(IInjectionProvider provider, InjectionTarget target = default)
        => provider.GetInjection(TargetType, TargetKey, target)!;
    
    public override string ToString() => $"(Redirection, Type: {TargetType}, Key: {TargetKey})";
}