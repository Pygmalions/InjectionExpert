using System.ComponentModel;
using System.Diagnostics;
using InjectionExpert.Injectors;

namespace InjectionExpert;

public static class InjectionProviderExtensions
{
    /// <summary>
    /// Get a resource of the specified category for this provider.
    /// </summary>
    /// <param name="provider">Provider to get the object from.</param>
    /// <param name="type">Category type of the resource.</param>
    /// <param name="key">Optional key for the requested injection.</param>
    /// <returns>Requested resource, or null if not found.</returns>
    [DebuggerStepThrough, StackTraceHidden]
    public static object? GetInjection(this IInjectionProvider provider, 
        Type type, object? key = null)
        => provider.GetInjection(type, key, default)?.Instance;
    
    /// <summary>
    /// Get a resource of the specified category for this provider.
    /// </summary>
    /// <typeparam name="TObject">Category type of the resource.</typeparam>
    /// <param name="provider">Provider to get the object from.</param>
    /// <param name="key">Optional key for the requested injection.</param>
    /// <returns>Requested resource, or null if not found.</returns>
    [DebuggerStepThrough, StackTraceHidden]
    public static TObject? GetInjection<TObject>(this IInjectionProvider provider, object? key = null)
        => (TObject?)provider.GetInjection(typeof(TObject), key);

    public static object RequireInjection(this IInjectionProvider provider, Type type, object? key = null)
        => provider.GetInjection(type, key) ??
           throw new Exception($"Failed to find required injection '{type.Name}' with key '{key}'");

    public static TObject RequireInjection<TObject>(this IInjectionProvider provider, object? key = null)
        => (TObject?)provider.GetInjection(typeof(TObject), key) ??
           throw new Exception($"Failed to find required injection '{typeof(TObject).Name}' with key '{key}'");

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static void NewObject(this IInjectionProvider provider, object target)
    {
        using var scope = provider.NewScope(new InjectionTarget(Instance: target));
        
        var type = target.GetType();
        if (!ConstructorInjector.For(type).TryInject(target, scope, out var missing) ||
            !MemberInjector.For(type).TryInject(target, scope, out missing))
            throw new InjectionFailureException(type, provider, missing);
    }

    /// <summary>
    /// Instantiate a new object of the specified type and inject all required dependencies.
    /// </summary>
    /// <param name="provider">Provider to get injections from.</param>
    /// <param name="type">Type to instantiate.</param>
    /// <returns>Instantiated object instance.</returns>
    /// <exception cref="InjectionFailureException">
    /// Throw if any required injections cannot be found within the specified provider.
    /// </exception>
    public static object NewObject(this IInjectionProvider provider, Type type)
    {
        using var scope = provider.NewScope(new InjectionTarget(Type: type));
        
        if (!ConstructorInjector.For(type).TryInject(out var instance, scope, out var missing) ||
            !MemberInjector.For(type).TryInject(instance, scope, out missing))
            throw new InjectionFailureException(type, provider, missing);
        return instance;
    }

    /// <summary>
    /// Instantiate a new object of the specified type and inject all required dependencies.
    /// </summary>
    /// <typeparam name="TObject">Type to instantiate.</typeparam>
    /// <param name="provider">Provider to get injections from.</param>
    /// <returns>Instantiated object instance.</returns>
    /// <exception cref="InjectionFailureException">
    /// Throw if any required injections cannot be found within the specified provider.
    /// </exception>
    public static TObject NewObject<TObject>(this IInjectionProvider provider)
    {
        using var scope = provider.NewScope(new InjectionTarget(Type: typeof(TObject)));
        
        return (TObject)scope.NewObject(typeof(TObject));
    }
    
    
    /// <summary>
    /// Inject the members of this object with the specified injection provider.
    /// </summary>
    /// <param name="target">Object whose members will be injected.</param>
    /// <param name="provider">Provider to get injections from.</param>
    /// <param name="onlyNullMembers">
    /// If true, only members that are null will be injected.
    /// Value types other than <see cref="Nullable{T}"/> will always be injected.
    /// </param>
    /// <typeparam name="TTarget">Type of this object.</typeparam>
    /// <returns>This object.</returns>
    /// <exception cref="InjectionFailureException">
    /// Throw if any required injections cannot be found within the specified provider.
    /// </exception>
    public static TTarget WithInjections<TTarget>(
        this TTarget target, IInjectionProvider provider, bool onlyNullMembers = true)
        where TTarget : notnull
    {
        using var scope = provider.NewScope(new InjectionTarget(Instance: target));
        
        // Get the actual type of the target object, in case TTarget is a base class of it.
        var type = target.GetType();
        if (!MemberInjector.For(type).TryInject(target, scope, out var missing, onlyNullMembers))
            throw new InjectionFailureException(type, provider, missing);
        return target;
    }
}