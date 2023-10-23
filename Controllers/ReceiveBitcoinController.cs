using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
[ApiController]
public class RecieveBitcoinController : ControllerBase
{
    #region Private Fields
    private ExtKey extendedPrivateKey;
    private ExtPubKey extendedPublicKey;
    private int addressIndex;
    #endregion

    /// <summary>
    /// The recieve bitcoin constructor
    /// </summary>
    public RecieveBitcoinController()
    {
        // Initialize wallet properties as needed
        extendedPrivateKey = new ExtKey();
        extendedPublicKey = extendedPrivateKey.Neuter();
        addressIndex = 0;
    }

    /// <summary>
    /// Method to generate a new receiving address
    /// </summary>
    /// <param name="existingAddress"></param>
    /// <returns></returns>
    [HttpPost]
    [Route("api/receivebitcoin")]
    public ActionResult<string> GenerateReceivingAddress([FromBody] string existingAddress)
    {
        // Generate a new receiving address based on the current address index
        BitcoinAddress address = extendedPublicKey.Derive((uint)addressIndex).PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
        addressIndex++;

        return Ok(address.ToString());
    }
}