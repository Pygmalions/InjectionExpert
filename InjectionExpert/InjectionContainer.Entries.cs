using System.Collections.Concurrent;
using System.Diagnostics;
using InjectionExpert.Utilities;

namespace InjectionExpert;

public partial class InjectionContainer
{
    private abstract class InjectionEntry(InjectionLifespan lifespan)
    {
        public InjectionLifespan Lifespan { get; } = lifespan;

        public abstract object GetValue(Type type, InjectionTarget target);
    }

    [DebuggerDisplay("Constant={Value}")]
    private class ConstantEntry(object value) : InjectionEntry(InjectionLifespan.Singleton)
    {
        public object Value { get; } = value;

        public override object GetValue(Type type, InjectionTarget target) => Value;
    }

    [DebuggerDisplay("Factory={Factory}")]
    private class FactoryEntry(
        IInjectionProvider provider,
        InjectionLifespan lifespan,
        IInjectionContainer.FactoryDelegate factory)
        : InjectionEntry(lifespan)
    {
        public IInjectionContainer.FactoryDelegate Factory { get; } = factory;

        private object? _cache;

        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan == InjectionLifespan.Singleton)
                return _cache ??= Factory(provider, type, target);
            return Factory(provider, type, target);
        }
    }

    [DebuggerDisplay("Type={Implementation}")]
    private class TypeEntry(IInjectionProvider provider, InjectionLifespan lifespan, Type implementation)
        : InjectionEntry(lifespan)
    {
        public Type Implementation { get; } = implementation;

        private object? _cache;

        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan == InjectionLifespan.Singleton)
                return _cache ??= provider.NewObject(Implementation);
            return provider.NewObject(Implementation.MakeGenericType(Implementation));
        }
    }

    [DebuggerDisplay("GenericDefinition={Implementation}")]
    private class TypeDefinitionEntry(
        IInjectionProvider provider,
        InjectionLifespan lifespan,
        Type genericCategory,
        Type implementation) : InjectionEntry(lifespan)
    {
        /// <summary>
        /// Implementation type definition.
        /// </summary>
        public Type Implementation { get; } = implementation;

        /// <summary>
        /// Category type definition, defined with the generic parameters from the implementation type definition.
        /// </summary>
        public Type GenericCategory { get; } = genericCategory;

        private ConcurrentDictionary<Type, object>? _caches;

        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan != InjectionLifespan.Singleton)
                return InstantiateValue(type);

            _caches ??= new ConcurrentDictionary<Type, object>();
            return _caches.GetOrAdd(type, InstantiateValue);

            object InstantiateValue(Type targetType)
            {
                if (targetType.GetGenericTypeDefinition() == Implementation)
                    return provider.NewObject(targetType);
                var parameters = new Type[Implementation.GetGenericArguments().Length];
                GenericParameterExtractor.ExtractArguments(
                    targetType, GenericCategory, parameters);
                return provider.NewObject(Implementation.MakeGenericType(parameters));
            }
        }
    }

    [DebuggerDisplay("Redirection=({TargetType}, {TargetKey})")]
    private class RedirectionEntry(IInjectionProvider provider, Type targetType, object? targetKey)
        : InjectionEntry(InjectionLifespan.Transient)
    {
        public Type TargetType { get; } = targetType;

        public object? TargetKey { get; } = targetKey;

        public override object GetValue(Type type, InjectionTarget target)
            => provider.GetInjection(TargetType, TargetKey, target)!;
    }
}