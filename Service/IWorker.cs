using System;

namespace EIR_9209_2.Service;

/// <summary>
/// Defines methods for managing and controlling worker endpoints.
/// </summary>
public interface IWorker
{
    /// <summary>
    /// Adds a new endpoint to the worker.
    /// </summary>
    /// <param name="endpointConfig">The configuration for the endpoint to add.</param>
    /// <returns>True if the endpoint was added successfully; otherwise, false.</returns>
    Task<bool> AddEndpoint(Connection endpointConfig);
    /// <summary>
    /// Updates an existing endpoint in the worker.
    /// </summary>
    /// <param name="endpointConfig">The configuration for the endpoint to update.</param>
    /// <returns>True if the endpoint was updated successfully; otherwise, false.</returns>
    Task<bool> UpdateEndpoint(Connection endpointConfig);
    /// <summary>
    /// Removes an endpoint from the worker.
    /// </summary>
    /// <param name="endpointId">The identifier of the endpoint to remove.</param>
    /// <returns>True if the endpoint was removed successfully; otherwise, false.</returns>
    Task<bool> RemoveEndpoint(Connection endpointId);

    /// <summary>
    /// Deactivates all endpoints managed by the worker.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains true if all endpoints were deactivated successfully; otherwise, false.</returns>
    Task<bool> DeactivateAllEndpoints();

    /// <summary>
    /// Starts the worker asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the worker asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task StopAsync(CancellationToken cancellationToken);
}
