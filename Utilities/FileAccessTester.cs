using System.Security.AccessControl;
using System.Security.Principal;

namespace EIR_9209_2.Utilities
{
    public class FileAccessTester
    {
        public static bool CanCreateFilesAndWriteInFolder(string folderPath)
        {
            try
            {
                var currentUser = WindowsIdentity.GetCurrent();
                var currentPrincipal = new WindowsPrincipal(currentUser);
                var directorySecurity = new DirectoryInfo(folderPath).GetAccessControl();
                var authorizationRules = directorySecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));

                foreach (FileSystemAccessRule rule in authorizationRules)
                {
                    if (!RuleProvidesWriteAccess(rule)) continue;
                    if (rule.IdentityReference == currentUser.User || currentPrincipal.IsInRole((SecurityIdentifier)rule.IdentityReference))
                    {
                        if (rule.AccessControlType == AccessControlType.Allow)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return false;
            }

            bool RuleProvidesWriteAccess(FileSystemAccessRule rule)
            {
                return (rule.FileSystemRights & FileSystemRights.CreateFiles) != 0;
            }
        }
    }
}
