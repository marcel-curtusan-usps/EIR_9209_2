using System.Reflection;

namespace EIR_9209_2.Utilities
{
    public static class Helper
    {
        /// <summary>
        /// get the name of the Application
        /// </summary>
        /// <returns></returns>
        public static string GetAppName()
        {
            string name = Assembly.GetExecutingAssembly().GetName().Name?.ToString() ?? string.Empty;
            return name;
        }
        /// <summary>
        ///  get the version of the application
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? string.Empty;
        }
        /// <summary>
        /// get the root log file path
        /// </summary>
        /// <returns></returns>
        public static string GetLogFilePath()
        {
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            return filePath;
        }
    }
}
