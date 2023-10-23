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
    public async Task<ActionResult<CreateWalletResponse>> CreateNewWallet(string passphrase)
    {
        // Generate a new BIP39 mnemonic phrase with a secure entropy
        Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
        string generatedMnemonic = mnemonic.ToString();

        // Derive the root key from the mnemonic and passphrase
        ExtKey extendedKey = mnemonic.DeriveExtKey(passphrase);

        // Derive a Bitcoin address from the extended key
        Key privateKey = extendedKey.PrivateKey;
        BitcoinAddress bitcoinAddress = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);

        var response = new CreateWalletResponse
        {
            Mnemonic = generatedMnemonic,
            BitcoinAddress = bitcoinAddress
        };
        
        return await Task.FromResult(response);
    }
}