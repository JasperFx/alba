namespace Alba.Pooling;

internal sealed class PooledHost(AlbaHost host, object? state, AlbaHostSlot slot)
{
    private AlbaHost.HookCheckpoint _hooks;

    public AlbaHost Host { get; } = host;

    public object? State { get; } = state;

    public AlbaHostSlot Slot { get; } = slot;

    public IResettableAlbaExtension[] Resettable { get; set; } = [];

    // Each return resets the host to how it was once it started and OnCreated ran
    public void Checkpoint()
    {
        _hooks = Host.CheckpointHooks();
        foreach (var extension in Resettable) extension.Checkpoint();
    }

    public async Task ResetAsync()
    {
        Host.RestoreHooks(_hooks);
        foreach (var extension in Resettable) await extension.ResetAsync();
    }
}
