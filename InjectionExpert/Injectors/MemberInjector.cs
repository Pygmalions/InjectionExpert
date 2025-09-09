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
    /// <param name="type">Type to get the member injector for.</param>
    /// <returns>Member injector.</returns>
    public static MemberInjector For(Type type) => Cache[type];
    
    /// <summary>
    /// Inject the members of the specified object.
    /// If the specified object is a boxed struct, then the boxed value will be replaced.
    /// </summary>
    /// <param name="target">Target to inject.</param>
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
    public static bool TryInject<TTarget>(
        TTarget target, IInjectionProvider provider, out InjectionTarget missing,
        bool onlyNullMembers = false) where TTarget : notnull
    {
        return For(typeof(TTarget)).TryInject(target, provider, out missing, onlyNullMembers);
    }

    private readonly Type _type;

    private readonly MultiDictionary<(Type Type, object? Key), MemberInfo> _injections;

    public IEnumerable<(Type Type, object? Key, MemberInfo Member)> Dependencies
        => _injections.SelectMany(pair => pair.Value.Select(member => (pair.Key.Type, pair.Key.Key, member)));
    
    private readonly Func<object, IInjectionProvider, bool, InjectionTarget?> _functor;

    private MemberInjector(
        Type type, Func<object, IInjectionProvider, bool, InjectionTarget?> functor,
        MultiDictionary<(Type Type, object? Key), MemberInfo> injections)
    {
        _type = type;
        _functor = functor;
        _injections = injections;
    }

    /// <summary>
    /// Inject the members of the specified object.
    /// If the specified object is a boxed struct, then the boxed value will be replaced.
    /// </summary>
    /// <param name="target">Target object, can be boxed structs.</param>
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
    /// <returns>
    /// True if the members with specified type of injections are updated;
    /// false if no member is injected with the specified type.
    /// </returns>
    public bool TryUpdate(object target, Type type, object? key, object? injection)
    {
        if (!_injections.TryGetValues((type, key), out var members))
            return false;
        foreach (var member in members)
        {
            switch (member)
            {
                case FieldInfo field:
                    field.SetValue(target, injection);
                    break;
                case PropertyInfo property:
                    property.SetValue(target, injection);
                    break;
                default:
                    throw new Exception("Unsupported injection member type \"{member.MemberType}\".");
            }
        }

        return true;
    }
}