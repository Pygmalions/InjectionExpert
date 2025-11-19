using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InjectionExpert.Injectors;

namespace InjectionExpert;

public static class InjectionProviderExtensions
{
    extension(IInjectionProvider provider)
    {
        /// <summary>
        /// Get an injection of the specified category for this provider.
        /// </summary>
        /// <param name="type">Type of the injection.</param>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>
        /// Requested injection if it is found;
        /// otherwise, null for reference types or default value for value types.
        /// </returns>
        [DebuggerStepThrough, StackTraceHidden]
        public object? GetInjection(Type type, object? key = null, InjectionTarget target = default)
            => provider.GetInjectionItem(type, key, target)?.Instance;

        /// <summary>
        /// Get an injection of the specified category for this provider.
        /// </summary>
        /// <typeparam name="TObject">Type of the injection.</typeparam>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>
        /// Requested injection, or null (for reference types) and default value (for value types)
        /// if not found.
        /// </returns>
        [DebuggerStepThrough, StackTraceHidden]
        public TObject? GetInjection<TObject>(object? key = null, InjectionTarget target = default)
            => (TObject?)provider.GetInjectionItem(typeof(TObject), key, target)?.Instance;

        /// <summary>
        /// Get an injection of the specified category for this provider.
        /// </summary>
        /// <param name="type">Type of the injection.</param>
        /// <param name="injection">Injection with the specified type and key.</param>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>True if the injection is found, otherwise false.</returns>
        public bool TryGetInjection([MaybeNullWhen(false)] out object injection,
            Type type, object? key = null, InjectionTarget target = default)
        {
            injection = provider.GetInjectionItem(type, key, target)?.Instance;
            return injection != null;
        }

        /// <summary>
        /// Get an injection of the specified category for this provider.
        /// </summary>
        /// <typeparam name="TObject">Type of the injection.</typeparam>
        /// <param name="injection">Injection with the specified type and key.</param>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>True if the injection is found, otherwise false.</returns>
        public bool TryGetInjection<TObject>([MaybeNullWhen(false)] out TObject injection,
            object? key = null, InjectionTarget target = default)
        {
            injection = (TObject?)provider.GetInjectionItem(typeof(TObject), key, target)?.Instance;
            return injection != null;
        }

        /// <summary>
        /// Get an injection of the specified category for this provider
        /// or throw an exception if not found.
        /// </summary>
        /// <param name="type">Type of the injection.</param>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>Requested injection.</returns>
        /// <exception cref="Exception">Throw if the requested injection is not found.</exception>
        public object RequireInjection(Type type, object? key = null, InjectionTarget target = default)
            => provider.GetInjectionItem(type, key, target) ??
               throw new Exception($"Failed to find required injection '{type.Name}' with key '{key}'");

        /// <summary>
        /// Get an injection of the specified category for this provider
        /// or throw an exception if not found.
        /// </summary>
        /// <typeparam name="TObject">Type of the injection.</typeparam>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <param name="target">Target that requests the injection.</param>
        /// <returns>Requested injection.</returns>
        /// <exception cref="Exception">Throw if the requested injection is not found.</exception>
        public TObject RequireInjection<TObject>(object? key = null, InjectionTarget target = default)
            => (TObject?)provider.GetInjection(typeof(TObject), key, target) ??
               throw new Exception($"Failed to find required injection '{typeof(TObject).Name}' with key '{key}'");

        /// <summary>
        /// Re-instantiate an object and inject all required dependencies.
        /// </summary>
        /// <param name="target">Target instance to re-construct.</param>
        /// <param name="options">Options for member injector.</param>
        /// <returns>Instantiated object instance.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void NewObject(object target, InjectorOptions? options = null)
        {
            using var scope = provider.NewScope(new InjectionTarget(target));

            var type = target.GetType();
            var missing = default(InjectionTarget);
            if (!ConstructorInjector.For(type).TryInject(target, scope) ||
                !MemberInjector.For(type).Inject(target, scope, options))
                throw new InjectionFailureException(type, provider, missing);
        }

        /// <summary>
        /// Instantiate a new object of the specified type and inject all required dependencies.
        /// </summary>
        /// <param name="type">Type to instantiate.</param>
        /// <param name="options">Options for member injector.</param>
        /// <returns>Instantiated object instance.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public object NewObject(Type type, InjectorOptions? options = null)
        {
            using var scope = provider.NewScope(new InjectionTarget(type));

            var missing = default(InjectionTarget);
            if (!ConstructorInjector.For(type).TryInject(out var instance, scope) ||
                !MemberInjector.For(type).Inject(instance, scope, options))
                throw new InjectionFailureException(type, provider, missing);
            return instance;
        }

        /// <summary>
        /// Instantiate a new object of the specified type and inject all required dependencies.
        /// </summary>
        /// <param name="options">Options for member injector.</param>
        /// <typeparam name="TObject">Type to instantiate.</typeparam>
        /// <returns>Instantiated object instance.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public TObject NewObject<TObject>(InjectorOptions? options = null)
        {
            using var scope = provider.NewScope(new InjectionTarget(typeof(TObject)));
            return (TObject)scope.NewObject(typeof(TObject), options);
        }
    }

    extension<TTarget>(TTarget target) where TTarget : notnull
    {
        /// <summary>
        /// Inject the members of this object with the specified injection provider.
        /// </summary>
        /// <param name="provider">Provider to get injections from.</param>
        /// <param name="options">
        /// Injection options.
        /// If this parameter is null,
        /// modified default options will be used: 
        /// <see cref="InjectorOptions.OnlyNullMembers"/> is true
        /// and <see cref="InjectorOptions.FailFast"/> is false.
        /// </param>
        /// <typeparam name="TTarget">Type of this object.</typeparam>
        /// <returns>This object.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public TTarget Autowire(IInjectionProvider provider, InjectorOptions? options = null)
        {
            using var scope = provider.NewScope(new InjectionTarget(target));

            options ??= InjectorOptions.Default with
            {
                FailFast = false,
                OnlyNullMembers = true
            };
            
            // Get the actual type of the target object, in case TTarget is a base class of it.
            var type = target.GetType();
            return !MemberInjector.For(type).Inject(target, scope, options)
                ? throw new InjectionFailureException(type, provider)
                : target;
        }

        /// <summary>
        /// Update the dependency members of this object with the specified dependency instance.
        /// </summary>
        /// <param name="injection">Injection instance.</param>
        /// <param name="key">Optional key of the dependency.</param>
        /// <param name="onlyNullMembers">If true, only members that are null will be injected.</param>
        /// <typeparam name="TTarget">Type of the target.</typeparam>
        /// <typeparam name="TDependency">Type of the dependency.</typeparam>
        /// <returns>Injected target instance.</returns>
        public TTarget Inject<TDependency>(TDependency injection,
            object? key = null, bool onlyNullMembers = true)
        {
            object boxedTarget = target;
            MemberInjector.For(target.GetType()).Update(
                boxedTarget, typeof(TDependency),
                key, injection, onlyNullMembers);
            return (TTarget)boxedTarget;
        }
    }
}