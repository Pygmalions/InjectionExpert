using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EmitToolbox;

namespace InjectionExpert.Injectors;

[RequiresDynamicCode("`System.Reflection.Emit` is used in this class.")]
public partial class ConstructorInjector
{
    private static readonly DynamicResourceCacheForType<ConstructorInjector>
        Cache = new(CreateInjector, moduleNamePrefix: "GeneratedConstructorInjectors_");

    /// <summary>
    /// Get the constructor injector for the specified type.
    /// </summary>
    /// <param name="type">Type of the instances to inject.</param>
    /// <returns>Constructor injector for the specified type.</returns>
    public static ConstructorInjector For(Type type) => Cache[type];
    
    private Type TargetType { get; }
    
    private readonly Func<object, IInjectionProvider, InjectionTarget?> _functor;
    
    private ConstructorInjector(
        Type targetType, Func<object, IInjectionProvider, InjectionTarget?> functor)
    {
        TargetType = targetType;
        _functor = functor;
    }

    /// <summary>
    /// Try to instantiate the instance with the given provider.
    /// </summary>
    /// <param name="target">Uninitialized instance.</param>
    /// <param name="provider">Provider to get injections from.</param>
    /// <param name="missing">
    /// The injection that cannot be found from the provider.
    /// It will be the default value if this method returns true.
    /// </param>
    /// <returns>
    /// True if the type has been successfully instantiated,
    /// false if any injection requirement cannot be satisfied.
    /// </returns>
    public bool TryInject(object target, IInjectionProvider provider, out InjectionTarget missing)
    {
        var requester = _functor(target, provider);
        if (requester != null)
        {
            missing = requester.Value;
            return false;
        }

        missing = default;
        return true;
    }

    /// <summary>
    /// Try to instantiate an instance of the specific type with the given provider.
    /// </summary>
    /// <param name="target">Instantiated instance.</param>
    /// <param name="provider">Provider to get injections from.</param>
    /// <param name="missing">
    /// The injection that cannot be found from the provider.
    /// It will be the default value if this method returns true.
    /// </param>
    /// <returns>
    /// True if the type has been successfully instantiated,
    /// false if any injection requirement cannot be satisfied.
    /// </returns>
    public bool TryInject(
        [MaybeNullWhen(false)] out object target,
        IInjectionProvider provider, out InjectionTarget missing)
    {
        target = RuntimeHelpers.GetUninitializedObject(TargetType);
        if (TryInject(target, provider, out missing))
            return true;
        target = null;
        return false;
    }
}