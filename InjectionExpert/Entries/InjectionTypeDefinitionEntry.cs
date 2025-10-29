using System.Collections.Concurrent;
using System.Diagnostics;
using InjectionExpert.Utilities;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert.Entries;

[DebuggerDisplay("GenericDefinition={ImplementationDefinition}")]
public class InjectionTypeDefinitionEntry : InjectionEntry
{
    private ConcurrentDictionary<Type, object>? _caches;

    private readonly IInjectionProvider _provider;
    
    /// <summary>
    /// Category type definition, defined with the generic parameters from the implementation type definition.
    /// </summary>
    public Type CategoryDefinition { get; }
    
    /// <summary>
    /// Implementation type definition.
    /// </summary>
    public Type ImplementationDefinition { get; }

    public InjectionTypeDefinitionEntry(
        IInjectionProvider provider, InjectionLifespan lifespan,
        Type category, Type implementation) : base(lifespan)
    {
        _provider = provider;
        if (category.IsInterface)
        {
            if (!implementation.TryMatchInterface(category, out var matchedCategory))
                throw new ArgumentException(
                    "The implementation type definition does not implement the category type definition.", 
                    nameof(implementation));
            CategoryDefinition = matchedCategory;
        }
        else
        {
            if (!implementation.TryMatchGenericBaseType(category, out var matchedCategory))
                throw new ArgumentException(
                    "The implementation type definition does not implement the category type definition.", 
                    nameof(implementation));
            CategoryDefinition = matchedCategory;
        }
        ImplementationDefinition = implementation;
    }

    public override object GetInjection(Type type, object? key, InjectionTarget target)
    {
        if (Lifespan != InjectionLifespan.Singleton)
            return InstantiateValue(type);

        _caches ??= new ConcurrentDictionary<Type, object>();
        return _caches.GetOrAdd(type, InstantiateValue);

        object InstantiateValue(Type targetType)
        {
            if (targetType.GetGenericTypeDefinition() == ImplementationDefinition)
                return _provider.NewObject(targetType);
            var parameters = new Type[ImplementationDefinition.GetGenericArguments().Length];
            GenericParameterExtractor.ExtractArguments(
                targetType, CategoryDefinition, parameters);
            return _provider.NewObject(ImplementationDefinition.MakeGenericType(parameters));
        }
    }

    public override bool InvalidateCache()
    {
        if (_caches == null)
            return false;
        _caches.Clear();
        return true;
    }

    public override string ToString() => $"({Lifespan}, Generic Definition: {ImplementationDefinition})";
}