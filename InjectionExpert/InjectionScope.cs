using InjectionExpert.Utilities.Internal;
using Microsoft.Extensions.ObjectPool;

namespace InjectionExpert;

public class InjectionScope : IInjectionProvider.IScope
{
    private class ScopePooledPolicy : IPooledObjectPolicy<InjectionScope>
    {
        public InjectionScope Create() => new();

        public bool Return(InjectionScope instance)
        {
            instance._target = default;
            instance._parent = null!;
            instance._provider = null;
            instance._scopedKeyedInjections?.Clear();
            instance._scopedUnkeyedInjections?.Clear();
            return true;
        }
    }

    private static readonly DefaultObjectPool<InjectionScope> PooledScopes =
        new(new ScopePooledPolicy());

    private InjectionScope? _parent;

    private IInjectionProvider? _provider;

    private KeyedDictionary<Type, object, object>? _scopedKeyedInjections;

    private Dictionary<Type, object>? _scopedUnkeyedInjections;

    private InjectionTarget _target;

    private InjectionScope()
    {
    }

    public InjectionTarget Target => _target;

    public IInjectionProvider.IScope? Parent => _parent;

    public InjectionItem? GetInjectionItem(Type type, object? key, InjectionTarget target)
    {
        if (_provider == null)
            throw new ObjectDisposedException(nameof(InjectionScope),
                "Cannot get injection from this scope: scope is already disposed.");

        // Search in the scope chain for the scoped cache.
        for (var current = this; current != null; current = current._parent)
        {
            if (key is null)
            {
                if (current._scopedUnkeyedInjections?.TryGetValue(type, out var value) == true)
                    return new InjectionItem(value, InjectionLifespan.Scoped);
            }
            else
            {
                if (current._scopedKeyedInjections?.TryGetValue(type, key, out var value) == true)
                    return new InjectionItem(value, InjectionLifespan.Scoped);
            }
        }

        var entry = _provider.GetInjectionItem(type, key, target);
        if (entry?.Lifespan != InjectionLifespan.Scoped)
            return entry;

        if (key is null)
        {
            _scopedUnkeyedInjections ??= new Dictionary<Type, object>();
            _scopedUnkeyedInjections[type] = entry.Value.Instance;
        }
        else
        {
            _scopedKeyedInjections ??= new KeyedDictionary<Type, object, object>();
            _scopedKeyedInjections.SetValue(type, key, entry.Value.Instance);
        }

        return entry;
    }

    /// <summary>
    /// Create a new nested scope of this scope.
    /// </summary>
    /// <returns>New nested sub-scope of the current scope.</returns>
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
        _scopedKeyedInjections?.Clear();
        _scopedUnkeyedInjections?.Clear();
        if (_provider == null)
            return;
        _provider = null;
        PooledScopes.Return(this);
    }

    public static InjectionScope New(
        IInjectionProvider provider, InjectionScope? parent, InjectionTarget target)
    {
        var scope = PooledScopes.Get();
        scope._provider = provider;
        scope._parent = parent;
        scope._target = target;
        return scope;
    }
}