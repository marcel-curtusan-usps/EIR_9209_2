namespace EIR_9209_2.Utilities.Extensions
{
    public static class ConfigurationExtensions
    {
        public static string GetRequiredValue(this IConfiguration config, string key)
        {
            var value = config[key];
            return value ?? throw new InvalidOperationException($"The value for [{key}] was not found in the configuration.");
        }

        public static T GetRequiredValue<T>(this IConfiguration config, string key)
        {
            _ = config.GetRequiredValue(key); //try to see if the value exists
            return config.GetValue<T>(key)!;
        }
    }
}
