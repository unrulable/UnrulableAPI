using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
[ApiController]
public class CreateBitcoinWalletController : ControllerBase
{
    #region Private Fields
    private ExtKey extendedPrivateKey;
    private ExtPubKey extendedPublicKey;
    private int addressIndex;
    #endregion

    /// <summary>
    /// The recieve bitcoin constructor
    /// </summary>
    public CreateBitcoinWalletController()
    {
        // Initialize wallet properties as needed
        extendedPrivateKey = new ExtKey();
        extendedPublicKey = extendedPrivateKey.Neuter();
        addressIndex = 0;
    }

    /// <summary>
    /// Method to create a new Bitcoin wallet
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("api/createbitcoinwallet")]
    public async Task<ActionResult<string>?> CreateNewWallet()
    {
        Key privateKey = new Key();
        BitcoinAddress address = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

        string privateKeyWIF = privateKey.ToString(Network.Main);
        string addressString = address.ToString();

        return await Task.FromResult(addressString);
    }
}