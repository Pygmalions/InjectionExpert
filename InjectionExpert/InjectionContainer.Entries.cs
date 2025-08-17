using System.Collections.Concurrent;
using System.Diagnostics;
using InjectionExpert.Utilities;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public partial class InjectionContainer
{
    private class FactoryEntry
    {
        public required InjectionContainer Container { get; init; }
        
        public required IInjectionContainer.FactoryDelegate Factory { get; init; }

        /// <summary>
        /// Caches for singleton injections.
        /// </summary>
        public ConcurrentKeyedDictionary<Type, object, object> Caches { get; } = new();
        
        public (object Injection, InjectionLifespan Lifespan)? Get(
            Type type, object? key, InjectionTarget target)
        {
            if (Caches.TryGetValue(type, key ?? NullKey.Value, out var value))
                return (value, InjectionLifespan.Singleton);
            var entry = Factory(Container, type, key, target);
            if (entry == null) 
                return null;
            if (entry.Value.Lifespan == InjectionLifespan.Singleton)
                Caches.SetValue(type, key ?? NullKey.Value, entry.Value.Injection);
            return entry;
        }
    }
    
    private abstract class InjectionEntry
    {
        public required InjectionContainer Container { get; init; }

        public required InjectionLifespan Lifespan { get; init; }
        
        public abstract object GetValue(Type type, InjectionTarget target);
    }
    
    [DebuggerDisplay("Constant={Value}")]
    private class ConstantInjectionEntry(object value) : InjectionEntry
    {
        public object Value { get; } = value;
        
        public override object GetValue(Type type, InjectionTarget target) => Value;
    }

    [DebuggerDisplay("Factory={Factory}")]
    private class FactoryInjectionEntry(Func<IInjectionProvider, InjectionTarget, object> factory) : InjectionEntry
    {
        public Func<IInjectionProvider, InjectionTarget, object> Factory { get; } = factory;

        private object? _cache;
        
        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan == InjectionLifespan.Singleton)
                return _cache ??= Factory(Container, target);
            return Factory(Container, target);
        }
    }

    [DebuggerDisplay("Type={Implementation}")]
    private class TypeInjectionEntry(Type implementation) : InjectionEntry
    {
        public Type Implementation { get; } = implementation;

        private object? _cache;
        
        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan == InjectionLifespan.Singleton)
                return _cache ??= Container.NewObject(Implementation);
            return Container.NewObject(Implementation.MakeGenericType(Implementation));
        }
    }
    
    [DebuggerDisplay("GenericDefinition={Definition}")]
    private class TypeDefinitionInjectionEntry(Type definition, Type matchedCategory) : InjectionEntry
    {
        public Type Definition { get; } = definition;

        public Type MatchedCategory { get; } = matchedCategory;

        private ConcurrentDictionary<Type, object>? _caches;
        
        public override object GetValue(Type type, InjectionTarget target)
        {
            if (Lifespan != InjectionLifespan.Singleton) 
                return InstantiateValue(type);
            
            _caches ??= new ConcurrentDictionary<Type, object>();
            return _caches.GetOrAdd(type, InstantiateValue);

            object InstantiateValue(Type targetType)
            {
                if (targetType.GetGenericTypeDefinition() == Definition)
                    return Container.NewObject(targetType);
                var parameters = new Type[Definition.GetGenericArguments().Length];
                GenericParameterInjector.Inject(targetType, MatchedCategory, parameters);
                return Container.NewObject(Definition.MakeGenericType(parameters));
            }
        }
    }

    private class RedirectionEntry(Type targetType, object? targetKey) : InjectionEntry
    {
        public Type TargetType { get; } = targetType;

        public object? TargetKey { get; } = targetKey;

        public override object GetValue(Type type, InjectionTarget target)
        {
            throw new Exception("Internal Error: RedirectionEntry need direct handling.");
        }
    }
}