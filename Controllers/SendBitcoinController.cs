using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitcoin.RPC;
using System.Net;

[Route("api/bitcointransaction")]
[ApiController]
public class SendBitcoinController : ControllerBase
{
    private readonly NodeConfiguration _bitcoinNodeConfig;
    private readonly Key _longTermPrivateKey;
    private readonly Network _network; 

    /// <summary>
    /// The send Bitcoin transaction controller.
    /// </summary>
    /// <param name="bitcoinNodeConfig"></param>
    /// <param name="longTermPrivateKey"></param>
    public SendBitcoinController(NodeConfiguration bitcoinNodeConfig, Key longTermPrivateKey, Network network)
    {
        _bitcoinNodeConfig = bitcoinNodeConfig;
        _longTermPrivateKey = longTermPrivateKey;
        _network = network;
    }

    /// <summary>
    /// Method to send a Bitcoin transaction.
    /// </summary>
    /// <param name="transactionRequest"></param>
    /// <returns></returns>
    [HttpPost("send")]
    public async Task<IActionResult> SendBitcoinTransaction([FromBody] TransactionRequest transactionRequest)
    {
        try
        {

            // Create an instance of RPCClient.
            var rpcCred = new NetworkCredential(_bitcoinNodeConfig.RpcUser, _bitcoinNodeConfig.RpcPassword);
            var uri = new Uri(_bitcoinNodeConfig.RpcUrl);
            var client = new RPCClient(rpcCred, uri);

            // Fetch unspent coins.
            var unspentCoins = await client.ListUnspentAsync();
            var coins = unspentCoins.Select(uc => (ICoin)uc).ToList();

            var recipientAddress = BitcoinAddress.Create(transactionRequest.RecipientAddress, _network);

            // Create a transaction.
            var transaction = Transaction.Create(_network);

            // Add inputs (coins).
            foreach (var coin in coins)
            {
                transaction.Inputs.Add(new TxIn(coin.Outpoint, scriptSig: Script.Empty));
            }

            // Add output to the recipient address.
            var amount = new Money(transactionRequest.Amount, MoneyUnit.BTC);
            var output = new TxOut(amount, recipientAddress);
            transaction.Outputs.Add(output);

            // Set change address.
            var changeAddress = await client.GetRawChangeAddressAsync();
            transaction.Outputs.Add(new TxOut(Money.Zero, changeAddress));

            // Create a Bitcoin secret key from RPC credentials.
            var privateKey = _longTermPrivateKey;
            var bitcoinSecret = privateKey.GetBitcoinSecret(_network);

            // Sign the transaction inputs.
            foreach (var i in Enumerable.Range(0, coins.Count))
            {
                transaction.Inputs[i].ScriptSig = coins[i].TxOut.ScriptPubKey;
                transaction.Sign(bitcoinSecret, coins.ToArray());
            }

            // Send the signed transaction.
            var txHash = await client.SendRawTransactionAsync(transaction);

            return Ok($"Transaction sent with hash: {txHash}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error sending Bitcoin transaction: {ex.Message}");
        }
    }
}
