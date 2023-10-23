using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

[AllowAnonymous]
[ApiController]
public class InitialSetupController : ControllerBase
{
    #region Private Fields
    private readonly IConfiguration _configuration;
    private readonly IOptions<NodeConfiguration> _bitcoinNodeConfig;
    #endregion

    /// <summary>
    /// The initial setup constructor
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="bitcoinNodeConfig"></param>
    public InitialSetupController(IConfiguration configuration, IOptions<NodeConfiguration> bitcoinNodeConfig)
    {
        _configuration = configuration;
        _bitcoinNodeConfig = bitcoinNodeConfig;
    }

    /// <summary>
    /// Method to configure the Bitcoin node
    /// </summary>
    /// <param name="bitcoinNodeConfig"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/initialsetup")]
    public IActionResult ConfigureBitcoinNode([FromBody] NodeConfiguration bitcoinNodeConfig)
    {
        try
        {
            // Update the configuration settings.
            _configuration.GetSection("BitcoinCoreNode").Bind(bitcoinNodeConfig);

            // Save the updated configuration to the appsettings.json file.
            var configurationPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            System.IO.File.WriteAllText(configurationPath, _configuration.ToString());

            return Ok("Bitcoin node configuration updated successfully.");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error updating Bitcoin node configuration: {ex.Message}");
        }
    }
}