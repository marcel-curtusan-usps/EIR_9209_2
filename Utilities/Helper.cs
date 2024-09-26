using System.Reflection;

namespace EIR_9209_2.Utilities
{
    public static class Helper
    {
        public static string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
        }
        public static string GetAppName()
        {
            string name = Assembly.GetExecutingAssembly().GetName().Name?.ToString() ?? string.Empty;
            return name;
        }
    }
}
