using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using EmitToolbox;

namespace InjectionExpert.Injectors;

[RequiresDynamicCode("`System.Reflection.Emit` is used in this class.")]
public partial class ConstructorInjector
{
    private static readonly DynamicTypeCache<ConstructorInjector>
        Cache = new(CreateInjector, moduleNamePrefix: "GeneratedConstructorInjectors_");

    public static ConstructorInjector For(Type type) => Cache[type];

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
    public static bool TryInject<TTarget>(
        object target, IInjectionProvider provider, out InjectionTarget missing) 
        where TTarget : notnull
    {
        return For(typeof(TTarget)).TryInject(target, provider, out missing);
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
    public static bool TryInject<TTarget>(
        [NotNullWhen(true)] out TTarget? target,
        IInjectionProvider provider, out InjectionTarget missing)
    {
        if (For(typeof(TTarget)).TryInject(out var instance, provider, out missing))
        {
            target = (TTarget)instance;
            return true;
        }
        target = default;
        return false;
    }
    
    private readonly Type _type;
    
    private readonly Func<object, IInjectionProvider, InjectionTarget?> _functor;
    
    private ConstructorInjector(Type type, Func<object, IInjectionProvider, InjectionTarget?> functor)
    {
        _type = type;
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
        [NotNullWhen(true)] out object? target,
        IInjectionProvider provider, out InjectionTarget missing)
    {
        target = RuntimeHelpers.GetUninitializedObject(_type);
        if (TryInject(target, provider, out missing))
            return true;
        target = null;
        return false;
    }
}