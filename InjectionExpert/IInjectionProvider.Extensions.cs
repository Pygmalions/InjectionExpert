using System.ComponentModel;
using System.Diagnostics;
using InjectionExpert.Injectors;

namespace InjectionExpert;

public static class InjectionProviderExtensions
{
    extension(IInjectionProvider provider)
    {
        /// <summary>
        /// Get a resource of the specified category for this provider.
        /// </summary>
        /// <param name="type">Category type of the resource.</param>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <returns>Requested resource, or null if not found.</returns>
        [DebuggerStepThrough, StackTraceHidden]
        public object? GetInjection(Type type, object? key = null)
            => provider.GetInjection(type, key, default)?.Instance;

        /// <summary>
        /// Get a resource of the specified category for this provider.
        /// </summary>
        /// <typeparam name="TObject">Category type of the resource.</typeparam>
        /// <param name="key">Optional key for the requested injection.</param>
        /// <returns>Requested resource, or null if not found.</returns>
        [DebuggerStepThrough, StackTraceHidden]
        public TObject? GetInjection<TObject>(object? key = null)
            => (TObject?)provider.GetInjection(typeof(TObject), key);

        public object RequireInjection(Type type, object? key = null)
            => provider.GetInjection(type, key) ??
               throw new Exception($"Failed to find required injection '{type.Name}' with key '{key}'");

        public TObject RequireInjection<TObject>(object? key = null)
            => (TObject?)provider.GetInjection(typeof(TObject), key) ??
               throw new Exception($"Failed to find required injection '{typeof(TObject).Name}' with key '{key}'");

        [EditorBrowsable(EditorBrowsableState.Advanced)]
        public void NewObject(object target)
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
        /// <param name="type">Type to instantiate.</param>
        /// <returns>Instantiated object instance.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public object NewObject(Type type)
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
        /// <returns>Instantiated object instance.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public TObject NewObject<TObject>()
        {
            using var scope = provider.NewScope(new InjectionTarget(Type: typeof(TObject)));

            return (TObject)scope.NewObject(typeof(TObject));
        }
    }

    extension<TTarget>(TTarget target) where TTarget : notnull
    {
        /// <summary>
        /// Inject the members of this object with the specified injection provider.
        /// </summary>
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
        public TTarget Autowire(IInjectionProvider provider, bool onlyNullMembers = true)
        {
            using var scope = provider.NewScope(new InjectionTarget(Instance: target));

            // Get the actual type of the target object, in case TTarget is a base class of it.
            var type = target.GetType();
            return !MemberInjector.For(type).TryInject(target, scope, out var missing, onlyNullMembers)
                ? throw new InjectionFailureException(type, provider, missing)
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
            MemberInjector.For(target.GetType()).TryUpdate(
                boxedTarget, typeof(TDependency),
                key, injection, onlyNullMembers);
            return (TTarget)boxedTarget;
        }
    }
}