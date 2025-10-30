using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EmitToolbox;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Injectors;

[RequiresDynamicCode("`System.Reflection.Emit` is used in this class.")]
public partial class MemberInjector
{
    private static readonly DynamicResourceCacheForType<MemberInjector>
        Cache = new(CreateInjector, moduleNamePrefix: "GeneratedMemberInjectors_");

    /// <summary>
    /// Get the member injector for the specified type.
    /// </summary>
    /// <param name="type">Type of the instances to inject.</param>
    /// <returns>Member injector for the specified type.</returns>
    public static MemberInjector For(Type type) => Cache[type];

    private readonly MultiDictionary<(Type Type, object? Key), MemberInfo> _dependencies;

    private readonly Func<object, IInjectionProvider, bool, InjectionTarget?> _functor;
    
    public Type TargetType { get; }
    
    public IEnumerable<(Type Type, object? Key, MemberInfo Member)> Dependencies
        => _dependencies.SelectMany(pair => pair.Value.Select(member => (pair.Key.Type, pair.Key.Key, member)));
    
    private MemberInjector(
        Type type, Func<object, IInjectionProvider, bool, InjectionTarget?> functor,
        MultiDictionary<(Type Type, object? Key), MemberInfo> dependencies)
    {
        TargetType = type;
        _functor = functor;
        _dependencies = dependencies;
    }

    /// <summary>
    /// Inject the members of the specified object.
    /// If the specified object is a boxed struct, then the boxed value will be replaced.
    /// </summary>
    /// <param name="target">Target object can be boxed structs.</param>
    /// <param name="provider">Provider to get injections from.</param>
    /// <param name="missing">
    /// The requester whose injection requirement cannot be satisfied.
    /// It will be the default value if this method returns true.
    /// </param>
    /// <param name="onlyNullMembers">
    /// If true, the injector will not inject not null members.
    /// Value types without <see cref="Nullable{T}"/> will always be injected. 
    /// </param>
    /// <returns>
    /// True if all required injections of the specified object are found and injected,
    /// otherwise false.
    /// </returns>
    public bool TryInject(object target, IInjectionProvider provider, out InjectionTarget missing,
        bool onlyNullMembers = false)
    {
        var requester = _functor(target, provider, onlyNullMembers);
        if (requester != null)
        {
            missing = requester.Value;
            return false;
        }

        missing = default;
        return true;
    }

    /// <summary>
    /// Try to update the injections of the specified object.
    /// </summary>
    /// <param name="target">
    /// Object whose members with specified type of injections will be updated.
    /// </param>
    /// <param name="type">Injection type.</param>
    /// <param name="key">Key for the injection.</param>
    /// <param name="injection">Injection instance.</param>
    /// <param name="onlyNullMembers">If true, only null members will be updated to the new value.</param>
    /// <returns>
    /// True if the members with the specified type of injections are updated;
    /// false if no member is injected with the specified type.
    /// </returns>
    public bool TryUpdate(object target, Type type, object? key, object? injection, 
        bool onlyNullMembers = false)
    {
        if (!_dependencies.TryGetValues((type, key), out var members))
            return false;
        var updated = false;
        foreach (var member in members)
        {
            switch (member)
            {
                case FieldInfo field:
                    if (onlyNullMembers && field.GetValue(target) != null)
                        break;
                    field.SetValue(target, injection);
                    updated = true;
                    break;
                case PropertyInfo property:
                    if (onlyNullMembers && property.GetValue(target) != null)
                        break;
                    property.SetValue(target, injection);
                    updated = true;
                    break;
                default:
                    throw new Exception("Unsupported injection member type \"{member.MemberType}\".");
            }
        }
        return updated;
    }
}