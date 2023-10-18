using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using NBitcoin.RPC;
using Microsoft.AspNetCore.Authorization;

[AllowAnonymous] // Allow anonymous access to this action
[Route("api/bitcoin")]
[ApiController]
public class BitcoinController : ControllerBase
{
    private readonly RPCClient rpcClient;

    public BitcoinController()
    {
        var nodeUri = new Uri(""); 
        var network = Network.TestNet; 
        var credentials = new RPCCredentialString();
        rpcClient = new RPCClient(credentials, nodeUri.ToString(), network);
    }

    [Route("balance")]
    [HttpGet]
    public ActionResult<object> GetBitcoinBalance()
    {
        try
        {
            var unspentOutputs = rpcClient.ListUnspent();
            var totalBalance = unspentOutputs.Sum(output => output.Amount);
            Console.WriteLine(totalBalance);
            return new { balance = totalBalance.ToString("F8") };
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR!!!!!!!");
            return BadRequest(new { error = ex.Message });
        }
    }

    [Route("blockchaininfo")]
    [HttpGet]
    public async Task<ActionResult<object>> GetBlockchainInfo()
    {
        try
        {
            var blockchainInfo = await rpcClient.GetBlockchainInfoAsync();
            return new
            {
                chain = blockchainInfo.Chain,
                blocks = blockchainInfo.Blocks,
                headers = blockchainInfo.Headers,
                bestblockhash = blockchainInfo.BestBlockHash,
                difficulty = blockchainInfo.Difficulty
            };
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
