using System.Collections.Concurrent;
using System.Diagnostics;
using InjectionExpert.Utilities;

namespace InjectionExpert.Entries;

[DebuggerDisplay("GenericDefinition={Implementation}")]
public class InjectionTypeDefinitionEntry(
    IInjectionProvider provider,
    InjectionLifespan lifespan,
    Type genericCategory,
    Type implementation) : InjectionEntry(lifespan)
{
    private ConcurrentDictionary<Type, object>? _caches;

    /// <summary>
    /// Implementation type definition.
    /// </summary>
    public Type Implementation { get; } = implementation;

    /// <summary>
    /// Category type definition, defined with the generic parameters from the implementation type definition.
    /// </summary>
    public Type GenericCategory { get; } = genericCategory;

    public override object GetInjection(Type type, InjectionTarget target)
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

    public override bool InvalidateCache()
    {
        if (_caches == null)
            return false;
        _caches.Clear();
        return true;
    }
}