using Microsoft.AspNetCore.Mvc;

[Route("api/ping")]
[ApiController]
public class PingController : ControllerBase
{
    /// <summary>
    /// Method to ping the API.
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }
}
