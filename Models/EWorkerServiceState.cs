
/// <summary>
/// Defines the various states of the EWorker service.
/// </summary>
public enum EWorkerServiceState
{
    /// <summary>
    /// Indicates that the service is stopped.
    /// </summary>
    Stopped,
    /// <summary>
    /// Indicates that the service is starting.
    /// </summary>
    Starting,
    /// <summary>
    /// Indicates that the service failed to start and is waiting to restart.
    /// </summary>
    StartFailedWaitingToRestart,
    /// <summary>
    /// Indicates that the service is currently running.
    /// </summary>
    Running,
    /// <summary>
    /// Indicates that the service is stopping.
    /// </summary>
    Stopping,
    /// <summary>
    /// Indicates that there was an error pulling data.
    /// </summary>
    ErrorPullingData,
    /// <summary>
    /// Indicates that the service is inactive.
    /// </summary>
    InActive,
    /// <summary>
    /// Idle state indicates that the service is not currently processing any tasks.
    /// </summary>
    Idle
}
