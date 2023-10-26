using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitcoin.RPC;

[ApiController]
public class OpReturnController : ControllerBase
{
    private RPCClient _rpcClient, _rpcConnection;
    private readonly OpReturnController bitcoinService;
    private readonly NodeConfiguration _bitcoinNodeConfig;

    public OpReturnController(NodeConfiguration bitcoinNodeConfig)
    {
        _bitcoinNodeConfig = bitcoinNodeConfig;

        string rpcUser = _bitcoinNodeConfig.RpcUser;
        string rpcPassword = _bitcoinNodeConfig.RpcPassword;
        string credentials = $"{rpcUser}:{rpcPassword}";

        // Set the RPC connection parameters
        _rpcClient = new RPCClient(
            credentials,
            "localhost",
            Network.Main);
        _rpcConnection = _rpcClient;
        _bitcoinNodeConfig = bitcoinNodeConfig;
    }

    [Route("api/opreturntransactionsforaddress")]
    public ActionResult<List<string>> GetOpReturnTransactionsForAddress(string bitcoinAddress)
    {
       var opReturnTransactions = new List<string>();

        var addressBase58 = BitcoinAddress.Create(bitcoinAddress, Network.Main).ToString();

        var mempool = _rpcClient.GetRawMempool();

        foreach (var txId in mempool)
        {
            var transaction = _rpcClient.GetRawTransaction(txId);

            foreach (var output in transaction.Outputs)
            {
                if (output.ScriptPubKey.IsUnspendable)
                {
                    var opReturnData = output.ScriptPubKey.ToBytes(true);
                    opReturnTransactions.Add(System.Text.Encoding.UTF8.GetString(opReturnData));
                }
            }
        }

        return Ok(opReturnTransactions);
    }

    
    [Route("api/opreturntransactionshistory")]
    public ActionResult<List<string>> GetOpReturnTransactionHistory()
    {
       var opReturnTransactions = new List<string>();

        var blockchainInfo = _rpcClient.GetBlockchainInfo();
        var blockCount = Convert.ToDecimal(blockchainInfo.Blocks);

        for (int blockNumber = 0; blockNumber < blockCount; blockNumber++)
        {
            var blockHash = _rpcClient.GetBlockHash(blockNumber);
            var block = _rpcClient.GetBlock(blockHash);

            foreach (var tx in block.Transactions)
            {
                foreach (var output in tx.Outputs)
                {
                    if (output.ScriptPubKey.IsUnspendable)
                    {
                        var opReturnData = output.ScriptPubKey.ToBytes(true);
                        opReturnTransactions.Add(System.Text.Encoding.UTF8.GetString(opReturnData));
                    }
                }
            }
        }

        return Ok(opReturnTransactions);
    }
}