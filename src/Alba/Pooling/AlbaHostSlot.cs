using Alba.Internal;

namespace Alba.Pooling;

/// <summary>
/// The place a host occupies in an AlbaHostPool. A host that replaces a discarded host gets
/// the same index, so per host resources such as a database name can be derived from it.
/// </summary>
public sealed class AlbaHostSlot
{
    private readonly List<IAsyncDisposable> _resources = new();

    internal AlbaHostSlot(int index)
    {
        Index = index;
    }

    /// <summary>
    /// From 0 to one less than the pool's MaxHosts
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// A name such as "alba-2", for prefixing per host resources like queues or schemas
    /// </summary>
    public string Name => $"alba-{Index}";

    /// <summary>
    /// Dispose a resource created for this host, such as its database, after the host is
    /// disposed. Resources are disposed in the reverse order they were registered.
    /// </summary>
    /// <param name="resource"></param>
    public void RegisterForDispose(IAsyncDisposable resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        lock (_resources) _resources.Add(resource);
    }

    internal async Task DisposeResourcesAsync()
    {
        IAsyncDisposable[] resources;
        lock (_resources)
        {
            resources = _resources.AsEnumerable().Reverse().ToArray();
            _resources.Clear();
        }

        await Teardown.RunAllAsync($"One or more failures while disposing the resources of {Name}",
            resources.Select(x => (Func<Task>)(() => x.DisposeAsync().AsTask())));
    }

    public override string ToString() => Name;
}
