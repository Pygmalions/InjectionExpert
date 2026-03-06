using System.ComponentModel;
using InjectionExpert.Utilities.Internal;

namespace InjectionExpert;

public class InjectionScope : IInjectionScope
{
    private readonly IInjectionProvider _provider;

    private readonly InjectionScope? _upstream;

    private readonly InjectionScope _root;

    private List<object>? _disposableInjections;

    private bool _disposed;

    /// <summary>
    /// Currently active injection requests.
    /// </summary>
    private HashSet<InjectionRequest>? _requests = [];

    private KeyedDictionary<Type, object, object>? _scopedInjections;

    private KeyedDictionary<Type, object, object>? _singletonInjections;

    public InjectionScope(IInjectionProvider provider) : this(provider, null)
    {
    }

    private InjectionScope(IInjectionProvider provider, InjectionScope? upstream)
    {
        _provider = provider;
        _upstream = upstream;
        _root = upstream?._root ?? this;
    }

    public IInjectionScope NewScope()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return new InjectionScope(_provider, this);
    }

    InjectionEntry? IInjectionProvider.GetEntry(Type type, object? key)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _provider.GetEntry(type, key);
    }

    public bool HasEntry(Type type, object? key = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return _provider.HasEntry(type, key);
    }

    private object? SearchCachedInjection(Type type, object key)
    {
        if (_scopedInjections?.GetValueOrDefault(type, key) is { } scoped)
            return scoped;

        if (_upstream?.SearchCachedInjection(type, key) is { } inherited)
            return inherited;

        return _singletonInjections?.GetValueOrDefault(type, key);
    }

    public object? GetInjection(Type type, object? key = null, InjectionTarget target = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        key ??= IInjectionProvider.NullKey.Instance;

        if (SearchCachedInjection(type, key) is { } cached)
            return cached;

        if (_provider.GetEntry(type, key) is not { } entry)
            return null;

        var request = new InjectionRequest(type, key, target);

        _requests ??= [];
        if (!_requests.Add(request))
            throw new InjectionFailureException(type, key, this, target,
                "Circular dependency detected.");

        var instance = entry.GetInjection(this, type, key, target);

        switch (entry.Lifespan)
        {
            case InjectionLifespan.Scoped:
                (_scopedInjections ??= []).SetValue(type, key, instance);
                break;
            case InjectionLifespan.Singleton:
                (_root._singletonInjections ??= []).SetValue(type, key, instance);
                break;
            case InjectionLifespan.Transient:
                break;
            default:
                throw new InvalidEnumArgumentException(
                    nameof(entry.Lifespan), (int)entry.Lifespan, typeof(InjectionLifespan));
        }

        if (instance is IDisposable or IAsyncDisposable)
            (_disposableInjections ??= []).Add(instance);

        _requests.Remove(request);

        return instance;
    }

    public async ValueTask DisposeAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _disposed = true;

        GC.SuppressFinalize(this);

        if (_disposableInjections == null)
            return;

        foreach (var instance in ((IEnumerable<object>)_disposableInjections).Reverse())
        {
            switch (instance)
            {
                case IAsyncDisposable disposable:
                    await disposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        _scopedInjections?.Clear();
        _disposableInjections.Clear();
        _disposableInjections = null;
    }

    private record struct InjectionRequest(Type Type, object? Key, InjectionTarget Target);
}