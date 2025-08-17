using System.Collections.Concurrent;

namespace InjectionExpert.Utilities.Internal;

internal class ObjectPool<TObject> where TObject : class
{
    private readonly ConcurrentBag<TObject> _pool = [];

    /// <summary>
    /// Factory to create new instances.
    /// </summary>
    public Func<TObject> Factory { get; }

    /// <summary>
    /// Capacity of the pooled objects,
    /// or null if it is not limited.
    /// </summary>
    public int Capacity { get; set; } = Environment.ProcessorCount;

    public ObjectPool(Func<TObject> factory)
    {
        Factory = factory;
    }

    /// <summary>
    /// Rent an instance from the pool.
    /// </summary>
    /// <returns>Reused or created instance.</returns>
    public TObject Rent()
    {
        if (!_pool.TryTake(out var instance))
            instance = Factory();
        return instance;
    }

    /// <summary>
    /// Return an instance to the pool.
    /// </summary>
    /// <param name="instance">Instance to return to the pool.</param>
    public void Return(TObject instance)
    {
        if (_pool.Count < Capacity)
            _pool.Add(instance);
    }

    public void Shrink()
    {
        var count = _pool.Count / 2;
        while (_pool.Count > count && _pool.TryTake(out _)) { }
    }

    public void Clear() => _pool.Clear();
}