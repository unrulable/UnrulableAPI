using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous]
[ApiController]
public class CreateBitcoinWalletController : ControllerBase
{
    /// <summary>
    /// Method to create a new Bitcoin wallet
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [Route("api/createbitcoinwallet")]
    public async Task<ActionResult<CreateWalletResponse>> CreateNewWallet([FromBody]CreateWalletRequest createWalletRequest)
    {
        // Generate a new BIP39 mnemonic phrase with a secure entropy
        Mnemonic mnemonic = new Mnemonic(Wordlist.English, WordCount.Twelve);
        string generatedMnemonic = mnemonic.ToString();
        // Derive the root key from the mnemonic and passphrase
        ExtKey extendedKey = mnemonic.DeriveExtKey(createWalletRequest.Password);

        // Derive a Bitcoin address from the extended key
        Key privateKey = extendedKey.PrivateKey;
        BitcoinAddress bitcoinAddress = privateKey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network.Main);
        var response = new CreateWalletResponse
        {
            Mnemonic = generatedMnemonic,
            BitcoinAddress = bitcoinAddress.ToString()
        };

        return await Task.FromResult(response);
    }
}