using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using NBitcoin;
using NBitcoin.RPC;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

[AllowAnonymous]
[ApiController]
public class BackupController : ControllerBase
{
    #region Private Fields
    private ExtKey extendedPrivateKey;
    private ExtPubKey extendedPublicKey;
    private int addressIndex;
    private RPCClient rpcClient, rpcConnection;
    private readonly NodeConfiguration _bitcoinNodeConfig;
    #endregion

    /// <summary>
    /// The backup controller constructor
    /// </summary>
    public BackupController(IOptions<NodeConfiguration> bitcoinNodeConfig)
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
        rpcClient = new RPCClient(
            credentials,
            "localhost", 
            Network.Main);
        rpcConnection = rpcClient;
    }

    /// <summary>
    /// Method to backup a Bitcoin wallet
    /// </summary>
    /// <param name="backupFilePath"></param>
    /// <param name="password"></param>
    /// <param name="walletData"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/backupwallet")]
    public bool BackupWallet(string backupFilePath, string password, WalletInfo walletData)
    {
        try
        {
            // Serialize the wallet data into JSON
            string jsonWalletData = JsonConvert.SerializeObject(walletData);

            // Encrypt the data with a password using AES encryption
            string encryptedData = EncryptData(jsonWalletData, password);

            // Save the encrypted backup to a file
            byte[] fileBytes = Encoding.UTF8.GetBytes(encryptedData);
            var fileContentResult = File(fileBytes, "application/octet-stream", backupFilePath);
            return fileContentResult != null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during wallet backup: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Method to restore a Bitcoin wallet
    /// </summary>
    /// <param name="backupFilePath"></param>
    /// <param name="password"></param>
    /// <param name="walletData"></param>
    /// <returns></returns>
    [HttpGet]
    [Route("api/restorewallet")]
    public async Task<ActionResult<FileContentResult>> RestoreWallet(string backupFilePath, string password, WalletInfo walletData)
    {
        if (!System.IO.File.Exists(backupFilePath))
        {
            Console.WriteLine("Backup file not found.");
            return null;
        }

        string encryptedData = System.IO.File.ReadAllText(backupFilePath);

        string decryptedData = DecryptData(encryptedData, password);
        
        walletData = JsonConvert.DeserializeObject<WalletInfo>(decryptedData);

        // Return the decrypted wallet data as a file
        byte[] fileBytes = Encoding.UTF8.GetBytes(decryptedData);
        var fileContentResult = new FileContentResult(fileBytes, "application/octet-stream");
        fileContentResult.FileDownloadName = "wallet.dat";
        return await Task.FromResult(fileContentResult);
    }

    /// <summary>
    /// Method to decrypt data with a password using AES encryption
    /// </summary>
    /// <param name="encryptedData"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private string DecryptData(string encryptedData, string password)
    {
        using (Aes aesAlg = Aes.Create())
        {
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("YourSaltHere"));

            aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = keyDerivation.GetBytes(aesAlg.BlockSize / 8);

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msDecrypt = new MemoryStream(Convert.FromBase64String(encryptedData)))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
        }
    }

    /// <summary>
    /// Method to encrypt data with a password using AES encryption
    /// </summary>
    /// <param name="data"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    private string EncryptData(string data, string password)
    {
        using (Aes aesAlg = Aes.Create())
        {
            Rfc2898DeriveBytes keyDerivation = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("YourSaltHere"));

            aesAlg.Key = keyDerivation.GetBytes(aesAlg.KeySize / 8);
            aesAlg.IV = keyDerivation.GetBytes(aesAlg.BlockSize / 8);

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(data);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }
}
