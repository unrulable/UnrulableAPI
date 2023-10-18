using Microsoft.AspNetCore.Mvc;
using NBitcoin;
using Microsoft.AspNetCore.Authorization;
using NBitcoin.RPC;
using System;
using System.Net;

[AllowAnonymous] // Allow anonymous access to this action
[Route("api/bitcoin")]
[ApiController]
public class BitcoinController : ControllerBase
{
    private readonly RPCClient rpcClient, rpcConnection;

    /// <summary>
    /// The Bitcoin Constructor
    /// </summary>
    public BitcoinController()
    {
        string rpcUser = "your_rpc_username";
        string rpcPassword = "your_rpc_password";
        string credentials = $"{rpcUser}:{rpcPassword}";
        
        // Set the RPC connection parameters
        var rpcConnection = new RPCClient(
            credentials,
            "localhost", 
            Network.Main);
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
            var blockchainInfo = rpcConnection.GetBlockchainInfo();
            Console.WriteLine("Blockchain Info:");
            Console.WriteLine(blockchainInfo);
            return new
            {
                chain = blockchainInfo.Chain,
                blocks = blockchainInfo.Blocks,
                headers = blockchainInfo.Headers,
                bestblockhash = blockchainInfo.BestBlockHash,
                difficulty = blockchainInfo.Difficulty
            };
        }
        catch (RPCException ex)
        {
            return $"RPC Error: {ex.Message}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
