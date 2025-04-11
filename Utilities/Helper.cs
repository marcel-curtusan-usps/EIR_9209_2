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
        /// <summary>
        /// Converts the first letter of each word to uppercase and the rest to lowercase.
        /// </summary>
        /// <param name="input">The input string to convert.</param>
        /// <returns>The input string with each word capitalized.</returns>
        public static string ConvertToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            return string.Join(" ", input
          .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) // Remove extra spaces
          .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
        }
    }
}
