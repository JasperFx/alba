namespace Alba.Pooling;

/// <summary>
/// An Alba extension that holds state for one host. An AlbaHostPool resets it each time a
/// test returns the host, so each host in a pool needs its own instance.
/// </summary>
public interface IResettableAlbaExtension : IAlbaExtension
{
    /// <summary>
    /// Record the state that ResetAsync() returns to. An AlbaHostPool calls this once the
    /// host has started and OnCreated has run.
    /// </summary>
    void Checkpoint();

    /// <summary>
    /// Return to the state recorded by Checkpoint()
    /// </summary>
    /// <returns></returns>
    Task ResetAsync();
}
