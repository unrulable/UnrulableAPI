using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NBitcoin;
using NBitcoin.RPC;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;

[AllowAnonymous]
[ApiController]
public class InitialSetupController : ControllerBase
{
    #region Private Fields
    private ExtKey extendedPrivateKey;
    private ExtPubKey extendedPublicKey;
    private int addressIndex;
    private RPCClient rpcClient, rpcConnection;
    #endregion

    /// <summary>
    /// The recieve bitcoin constructor
    /// </summary>
    public InitialSetupController()
    {
        // Initialize wallet properties as needed
        extendedPrivateKey = new ExtKey();
        extendedPublicKey = extendedPrivateKey.Neuter();
        addressIndex = 0;
        string rpcUser = "your_rpc_username";
        string rpcPassword = "your_rpc_password";
        string credentials = $"{rpcUser}:{rpcPassword}";
        
        // Set the RPC connection parameters
        rpcClient = new RPCClient(
            credentials,
            "localhost", 
            Network.Main);
        rpcConnection = rpcClient;
    }

    /// <summary>
    /// Method to create a new Bitcoin wallet
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [Route("api/aggregateaddresses")]
    public async Task<ActionResult<decimal>?> AggregateAddresses(string targetAddress)
    {
        // Replace this with actual Bitcoin addresses and their corresponding amounts.
        var addressesAndAmounts = RetrieveBitcoinAddressesAndAmounts(rpcConnection, targetAddress);

        decimal totalAmount = 0;

        foreach (var (address, amount) in addressesAndAmounts)
        {
            // Validate Bitcoin addresses (in real-world usage, validate against a Bitcoin node).
            if (BitcoinAddressValidator.IsValidBitcoinAddress(address))
            {
                // Add the amount to the total.
                totalAmount += amount;
            }
            else
            {
                // Handle invalid addresses.
                Console.WriteLine($"Invalid Bitcoin address: {address}");
            }
        }

        return await Task.FromResult(totalAmount);
    }

    /// <summary>
    /// Method to retrieve Bitcoin addresses and amounts.
    /// </summary>
    /// <param name="rpcConnection"></param>
    /// <param name="targetAddress"></param>
    /// <returns></returns>
    public static List<(string BitcoinAddress, decimal AmountReceived)> RetrieveBitcoinAddressesAndAmounts(RPCClient rpcConnection, string targetAddress)
    {
        var result = new List<(string BitcoinAddress, decimal AmountReceived)>();

            try
            {
                // List all Bitcoin addresses associated with the wallet.
                var transactions = rpcConnection.SendCommand("listtransactions", "*");

                foreach (JObject transaction in transactions.Result)
                {
                    var address = transaction["address"].ToString();
                    if (address == targetAddress)
                    {
                        decimal amountReceived = Money.Coins(transaction["amount"].Value<decimal>()).ToUnit(MoneyUnit.BTC);
                        result.Add((targetAddress, amountReceived));
                    }
                }
            }
            catch (RPCException rpcEx)
            {
                Console.WriteLine($"RPC Error: {rpcEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

        return result;
    }
}