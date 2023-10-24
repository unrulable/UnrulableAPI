using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NBitcoin;
using NBitcoin.RPC;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Options;

[AllowAnonymous]
[ApiController]
public class TransactionHistoryController : ControllerBase
{
    #region Private Fields
    private ExtKey extendedPrivateKey;
    private ExtPubKey extendedPublicKey;
    private int addressIndex;
    private RPCClient _rpcClient, _rpcConnection;
    private readonly NodeConfiguration _bitcoinNodeConfig;
    #endregion

    /// <summary>
    /// The recieve bitcoin constructor
    /// </summary>
    public TransactionHistoryController(IOptions<NodeConfiguration> bitcoinNodeConfig)
    {
        _bitcoinNodeConfig = bitcoinNodeConfig.Value;
        
        // Initialize wallet properties as needed
        extendedPrivateKey = new ExtKey();
        extendedPublicKey = extendedPrivateKey.Neuter();
        addressIndex = 0;
        string rpcUser = _bitcoinNodeConfig.RpcUser;
        string rpcPassword = _bitcoinNodeConfig.RpcPassword;
        string credentials = $"{rpcUser}:{rpcPassword}";
        
        // Set the RPC connection parameters
        _rpcClient = new RPCClient(
            credentials,
            "localhost", 
            Network.Main);
        _rpcConnection = _rpcClient;
    }

    /// <summary>
    /// Method to get the transaction history for a Bitcoin address
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("api/transactionhistory")]
    public async Task<ActionResult<List<TransactionInfo>>?> GetTransactionHistory(string bitcoinAddress)
    {
        var transactionHistory = new List<TransactionInfo>();

        // Get a list of all transactions in the blockchain
        var allTransactions = _rpcClient.SendCommand("listtransactions", "*");

        foreach (var tx in allTransactions.Result)
        {
            string address = tx.Value<string>("address");
            decimal amount = tx.Value<decimal>("amount");
            bool isOutgoing = amount < 0;

            if (address == bitcoinAddress)
            {
                transactionHistory.Add(new TransactionInfo
                {
                    TransactionId = tx.Value<string>("txid"),
                    Amount = amount,
                    IsOutgoing = isOutgoing
                });
            }
        }


        return await Task.FromResult(transactionHistory);
    }

    [HttpGet]
    [Route("api/monitorincomingtransactions")]
    public async Task<ActionResult<List<TransactionInfo>>?> MonitorIncomingTransactions(string bitcoinAddress)
    {
        var transactionHistory = new List<TransactionInfo>();

        // Query the Bitcoin node for new transactions
        var transactions = _rpcClient.SendCommand("listreceivedbyaddress", 0, true);

        foreach (var tx in transactions.Result)
        {
            string address = tx.Value<string>("address");
            decimal amount = tx.Value<decimal>("amount");
            if (amount > 0)
            {
                transactionHistory.Add(new TransactionInfo
                {
                    TransactionId = tx.Value<string>("txid"),
                    Amount = amount,
                    IsOutgoing = false,
                    ReceivingAddress = address
                });
            }
        }


        return await Task.FromResult(transactionHistory);
    }
}
