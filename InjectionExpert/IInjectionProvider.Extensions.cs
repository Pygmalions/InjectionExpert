using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using InjectionExpert.Injectors;

namespace InjectionExpert;

public static class InjectionProviderExtensions
{
    extension(IInjectionProvider self)
    {
        /// <summary>
        /// Retrieves an instance of the specified type from the injection provider, if available.
        /// </summary>
        /// <typeparam name="TObject">The type of object to retrieve from the injection provider.</typeparam>
        /// <param name="key">An optional key to identify the specific instance to retrieve. Defaults to null.</param>
        /// <param name="target">Information about the target that is requesting the injection. Defaults to the default value of <see cref="InjectionTarget"/>.</param>
        /// <returns>An instance of type <typeparamref name="TObject"/> if available; otherwise, null.</returns>
        [DebuggerStepThrough, StackTraceHidden]
        public TObject? GetInjection<TObject>(object? key = null, InjectionTarget target = default)
            => (TObject?)self.GetInjection(typeof(TObject), key, target);


        /// <summary>
        /// Attempts to retrieve an injection of the specified type from the provider.
        /// </summary>
        /// <param name="injection">When this method returns, contains the injected object, if found; otherwise, null.</param>
        /// <param name="type">The type of the object to retrieve.</param>
        /// <param name="key">An optional key to help identify the requested injection.</param>
        /// <param name="target">Additional information about the target requesting the injection.</param>
        /// <returns>True if the injection was successfully retrieved; otherwise, false.</returns>
        public bool TryGetInjection([MaybeNullWhen(false)] out object injection,
            Type type, object? key = null, InjectionTarget target = default)
        {
            injection = self.GetInjection(type, key, target);
            return injection != null;
        }

        /// <summary>
        /// Attempts to get an injection of the specified type and returns whether the injection was successfully retrieved.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to retrieve.</typeparam>
        /// <param name="injection">When this method returns, contains the retrieved object if successful; otherwise, the default value of <typeparamref name="TObject"/>.</param>
        /// <param name="key">An optional key to differentiate injections of the same type. Default is null.</param>
        /// <param name="target">The optional target that is requesting the injection. Default is <see cref="InjectionTarget"/>.</param>
        /// <returns>True if the injection was successfully retrieved; otherwise, false.</returns>
        public bool TryGetInjection<TObject>([MaybeNullWhen(false)] out TObject injection,
            object? key = null, InjectionTarget target = default)
        {
            injection = (TObject?)self.GetInjection(typeof(TObject), key, target);
            return injection != null;
        }

        /// <summary>
        /// Retrieves an injection of the specified type or throws an exception if no injection is available.
        /// </summary>
        /// <param name="type">The type of the object to retrieve.</param>
        /// <param name="key">An optional key used to distinguish injections of the same type.</param>
        /// <param name="target">The target requesting the injection, providing additional context for resolution.</param>
        /// <returns>The resolved injection object of the specified type.</returns>
        /// <exception cref="InjectionFailureException">
        /// Thrown if no injection could be resolved for the specified type, key, or target.
        /// </exception>
        public object RequireInjection(Type type, object? key = null, InjectionTarget target = default)
            => self.GetInjection(type, key, target) ??
               throw new InjectionFailureException(type, key, self, target);

        /// <summary>
        /// Retrieves an instance of the specified type from the injection provider.
        /// Throws an <see cref="InjectionFailureException"/> if the injection provider
        /// cannot provide an instance of the requested type.
        /// </summary>
        /// <typeparam name="TObject">The type of the object to retrieve.</typeparam>
        /// <param name="key">An optional key to identify the specific instance to retrieve.</param>
        /// <param name="target">The target information specifying the request context.</param>
        /// <returns>The instance of the requested type.</returns>
        /// <exception cref="InjectionFailureException">
        /// Thrown when the requested injection is unavailable from the provider.
        /// </exception>
        public TObject RequireInjection<TObject>(object? key = null, InjectionTarget target = default)
            => self.GetInjection<TObject>(key, target) ??
               throw new InjectionFailureException(typeof(TObject), key, self, target);

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
            var type = target.GetType();
            if (!ConstructorInjector.For(type).TryInject(target, self))
                throw new InjectionFailureException(type, null, self,
                    message: "Cannot find a constructor that " +
                             "the injection provider can provide all of its parameters.");
            MemberInjector.For(type).Inject(target, self, options);
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
            if (!ConstructorInjector.For(type).TryInject(out var instance, self))
                throw new InjectionFailureException(type, null, self, 
                    message: "Cannot find a constructor that " +
                             "the injection provider can provide all of its parameters.");
            MemberInjector.For(type).Inject(instance, self, options);
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
            return (TObject)self.NewObject(typeof(TObject), options);
        }
    }

    extension<TTarget>(TTarget target) where TTarget : notnull
    {
        /// <summary>
        /// Inject the members of this object with the specified injection provider.
        /// </summary>
        /// <param name="provider">Provider to get injections from.</param>
        /// <param name="options">
        /// Injection options. If this parameter is null,
        /// default options with enabling <see cref="InjectorOptions.OnlyNullMembers"/> will be used.
        /// </param>
        /// <typeparam name="TTarget">Type of this object.</typeparam>
        /// <returns>This object.</returns>
        /// <exception cref="InjectionFailureException">
        /// Throw if any required injections cannot be found within the specified provider.
        /// </exception>
        public TTarget Autowire(IInjectionProvider provider, InjectorOptions? options = null)
        {
            options ??= InjectorOptions.Default with
            {
                OnlyNullMembers = true
            };
            
            // Get the actual type of the target object, in case TTarget is a base class of it.
            var type = target.GetType();
            return !MemberInjector.For(type).Inject(target, provider, options)
                ? throw new InjectionFailureException(type, null, provider)
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