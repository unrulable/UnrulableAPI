using NBitcoin;

public class CreateWalletResponse
{
    // Define properties for the transaction parameters.
    public BitcoinAddress BitcoinAddress { get; set; }
    public string Mnemonic { get; set; }
}
