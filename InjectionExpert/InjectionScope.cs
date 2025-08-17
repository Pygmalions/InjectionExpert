using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public class InjectionScope : IInjectionProvider.IScope
{
    private static readonly ObjectPool<InjectionScope> PooledScopes = 
        new(() => new InjectionScope());

    private readonly ConcurrentKeyedDictionary<Type, object, object> _scoped = new();

    private IInjectionProvider? _provider;

    private InjectionScope? _parent;

    private InjectionTarget _target;

    private InjectionScope()
    {
    }

    public InjectionTarget Target => _target;

    public IInjectionProvider.IScope? Parent => _parent;

    public (object Injection, InjectionLifespan Lifespan)? GetInjection(
        Type type, object? key, InjectionTarget target)
    {
        if (_provider == null)
            throw new ObjectDisposedException(nameof(InjectionScope),
                "Cannot get injection from this scope: scope is already disposed.");

        var replacedKey = key ?? InjectionContainer.NullKey.Value;

        // Search in the scope chain for the scoped cache.
        for (var current = this; current != null; current = current._parent)
        {
            if (current._scoped.TryGetValue(type, replacedKey, out var value))
                return (value, InjectionLifespan.Scoped);
        }

        var entry = _provider.GetInjection(type, key, target);
        if (entry?.Lifespan == InjectionLifespan.Scoped)
            _scoped.SetValue(type, replacedKey, entry.Value);
        return entry;
    }

    /// <summary>
    /// Create a new nested scope of this scope.
    /// </summary>
    /// <returns>New nested sub-scope of current scope.</returns>
    public IInjectionProvider.IScope NewScope(InjectionTarget target)
    {
        if (_provider == null)
            throw new ObjectDisposedException(nameof(InjectionScope),
                "Cannot create a new scope from this scope: scope is already disposed.");
        return New(_provider, this, target);
    }

    public void Dispose()
    {
        _parent = null;
        _scoped.Clear();
        if (_provider == null)
            return;
        _provider = null;
        PooledScopes.Return(this);
    }
    
    public static InjectionScope New(
        IInjectionProvider provider, InjectionScope? parent, InjectionTarget target)
    {
        var scope = PooledScopes.Rent();
        scope._provider = provider;
        scope._parent = parent;
        scope._target = target;
        return scope;
    }
}