using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BackgroundServiceController : ControllerBase
{
    private readonly BackgroundServiceManager _serviceManager;

    public BackgroundServiceController(BackgroundServiceManager serviceManager)
    {
        _serviceManager = serviceManager;
    }
    [HttpGet("/list_of_Service")]
    public IActionResult GetServices()
    {
        var serviceIds = _serviceManager.GetServiceIds();
        return Ok(serviceIds);
    }

    [HttpPost("{id}/start")]
    public async Task<IActionResult> Start(string id)
    {
        await _serviceManager.StartServiceAsync(id, new CancellationToken());
        return Ok($"Background service {id} started");
    }

    [HttpPost("{id}/stop")]
    public async Task<IActionResult> Stop(string id)
    {
        await _serviceManager.StopServiceAsync(id, new CancellationToken());
        return Ok($"Background service {id} stopped");
    }
}
