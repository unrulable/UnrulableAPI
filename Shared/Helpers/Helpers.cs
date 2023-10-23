using NBitcoin;

/// <summary>
/// Bitcoin address validator.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Validates a Bitcoin address.
    /// </summary>
    /// <param name="address"></param>
    /// <returns>Returns true or false</returns>
    public static bool IsValidBitcoinAddress(string address)
    {
        try
        {
            var bitcoinAddress = BitcoinAddress.Create(address, Network.Main);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}