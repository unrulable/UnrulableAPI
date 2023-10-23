public class AppConfig
{
    private readonly IConfiguration _configuration;

    public AppConfig(IConfiguration configuration)
    {
        _configuration = configuration;
        _configuration.GetSection("BitcoinCoreNode").Get<NodeConfiguration>();
    }

    // Define properties to access configuration values.
    public string SomeSetting => _configuration["SomeSetting"];
    public int SomeIntegerSetting => _configuration.GetValue<int>("SomeIntegerSetting");

    // Add more properties as needed.

    // You can add methods or properties for more complex configuration values.

    // For example:
    // public DatabaseConfiguration DatabaseConfig => _configuration.Get<DatabaseConfiguration>();
}
