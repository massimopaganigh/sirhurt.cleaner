using Microsoft.Win32;
using Serilog;
using System.Diagnostics;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Removes SirHurt and Roblox-related components from the filesystem and Windows registry.
    /// Handles cleanup operations for all user profiles on the system.
    /// </summary>
    public static class SystemCleaner
    {
        /// <summary>
        /// Executes all cleanup operations asynchronously with proper error handling.
        /// </summary>
        /// <returns>Task representing the cleanup operation</returns>
        public static async Task RunCleanupAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    CleanupSystemFolders();
                    CleanupCurrentUserFolders();
                    CleanupAllUserProfiles();
                    CleanupRegistry();
                }).ConfigureAwait(false);

                Log.Information("Operations completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during cleanup operations");
            }
        }

        /// <summary>
        /// Removes system-wide folders that require administrative privileges.
        /// </summary>
        private static void CleanupSystemFolders()
        {
            Log.Information("Checking system-wide folders");

            var systemFoldersToDelete = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rsTrust")
            };

            foreach (var folderPath in systemFoldersToDelete)
            {
                Log.Information("Attempting to delete system folder: {FolderPath}", folderPath);

                DeleteFolder(folderPath);
            }
        }

        /// <summary>
        /// Removes application folders for the current user.
        /// </summary>
        private static void CleanupCurrentUserFolders()
        {
            Log.Information("Checking folders for current user");

            var foldersToDelete = new[]
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt")
            };

            foreach (var folderPath in foldersToDelete)
            {
                DeleteFolder(folderPath);
            }
        }

        /// <summary>
        /// Removes application folders from all user profiles on the system.
        /// </summary>
        private static void CleanupAllUserProfiles()
        {
            try
            {
                Log.Information("Checking all user profiles on the system");

                string usersFolder = Path.Combine(Environment.GetEnvironmentVariable("SystemDrive") ?? "C:", "Users");

                if (!Directory.Exists(usersFolder))
                {
                    Log.Warning("Users folder not found at {UsersFolder}", usersFolder);

                    return;
                }

                var userProfiles = Directory.GetDirectories(usersFolder).Where(dir => !dir.EndsWith("Public") && !dir.EndsWith("Default") && !dir.EndsWith("Default User") && !dir.EndsWith("All Users"));

                Log.Information("Found {Count} user profiles to check", userProfiles.Count());

                foreach (var userProfile in userProfiles)
                {
                    try
                    {
                        string username = Path.GetFileName(userProfile);

                        Log.Information("Checking profile for user: {Username}", username);

                        string localAppData = Path.Combine(userProfile, "AppData", "Local");

                        if (Directory.Exists(localAppData))
                        {
                            string robloxPath = Path.Combine(localAppData, "Roblox");

                            DeleteFolder(robloxPath);
                        }

                        string roamingAppData = Path.Combine(userProfile, "AppData", "Roaming");

                        if (Directory.Exists(roamingAppData))
                        {
                            string sirhurtPath = Path.Combine(roamingAppData, "sirhurt");

                            DeleteFolder(sirhurtPath);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        Log.Warning(ex, "Access denied to user profile: {UserProfile}", userProfile);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error processing user profile: {UserProfile}", userProfile);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process user profiles");
            }
        }

        /// <summary>
        /// Removes application registry keys for all users.
        /// </summary>
        private static void CleanupRegistry()
        {
            DeleteRegistryKey(@"Software\Asshurt");

            try
            {
                using var usersKey = Registry.Users;

                if (usersKey == null)
                {
                    Log.Warning("Unable to access HKEY_USERS registry hive");

                    return;
                }

                var subKeyNames = usersKey.GetSubKeyNames();

                Log.Information("Found {Count} user registry hives", subKeyNames.Length);

                foreach (var sid in subKeyNames)
                {
                    if (sid == ".DEFAULT" || sid.EndsWith("_Classes"))
                    {
                        continue;
                    }

                    try
                    {
                        Log.Information("Checking registry for user SID: {Sid}", sid);

                        DeleteRegistryKey(sid + @"\Software\Asshurt", Registry.Users);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Error accessing registry for user SID: {Sid}", sid);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to process user registry hives");
            }
        }

        /// <summary>
        /// Deletes a folder and all its contents with error handling.
        /// </summary>
        /// <param name="path">Full path to the folder</param>
        private static void DeleteFolder(string path)
        {
            if (!Directory.Exists(path))
            {
                Log.Debug("Folder does not exist: {FolderPath}", path);

                return;
            }

            try
            {
                Log.Information("Deleting folder: {FolderPath}", path);
                Directory.Delete(path, recursive: true);
                Log.Information("Folder deleted: {FolderPath}", path);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Access denied to folder: {FolderPath}. Attempting with elevated permissions.", path);

                TryDeleteWithElevatedPermissions(path);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to delete folder {FolderPath}", path);
            }
        }

        /// <summary>
        /// Attempts to delete a folder using PowerShell with elevated permissions.
        /// </summary>
        /// <param name="path">Path to the folder</param>
        private static void TryDeleteWithElevatedPermissions(string path)
        {
            try
            {
                Log.Information("Attempting to delete folder using PowerShell: {FolderPath}", path);

                string command = $"Remove-Item -Path \"{path}\" -Force -Recurse -ErrorAction SilentlyContinue";

                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using var process = Process.Start(startInfo);

                if (process == null)
                {
                    Log.Error("Failed to start PowerShell process");

                    return;
                }

                string error = process.StandardError.ReadToEnd();

                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Log.Warning("PowerShell reported an error: {Error}", error);
                }

                if (Directory.Exists(path))
                {
                    Log.Warning("PowerShell command completed but folder still exists: {FolderPath}", path);
                }
                else
                {
                    Log.Information("Successfully deleted folder with PowerShell: {FolderPath}", path);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to delete folder using PowerShell: {FolderPath}", path);
            }
        }

        /// <summary>
        /// Deletes a registry key from HKEY_CURRENT_USER.
        /// </summary>
        /// <param name="keyPath">Registry key path to delete</param>
        private static void DeleteRegistryKey(string keyPath)
        {
            DeleteRegistryKey(keyPath, Registry.CurrentUser);
        }

        /// <summary>
        /// Deletes a registry key from the specified registry hive.
        /// </summary>
        /// <param name="keyPath">Registry key path to delete</param>
        /// <param name="registryHive">Registry hive containing the key</param>
        private static void DeleteRegistryKey(string keyPath, RegistryKey registryHive)
        {
            try
            {
                Log.Information("Checking registry key: {RegistryHive}\\{KeyPath}", registryHive == Registry.CurrentUser ? "HKEY_CURRENT_USER" : registryHive == Registry.Users ? "HKEY_USERS" : registryHive.ToString(), keyPath);

                using var key = registryHive.OpenSubKey(keyPath, writable: true);

                if (key == null)
                {
                    Log.Debug("Registry key does not exist: {RegistryHive}\\{KeyPath}", registryHive == Registry.CurrentUser ? "HKEY_CURRENT_USER" : registryHive == Registry.Users ? "HKEY_USERS" : registryHive.ToString(), keyPath);

                    return;
                }

                registryHive.DeleteSubKeyTree(keyPath);

                Log.Information("Registry key deleted: {RegistryHive}\\{KeyPath}", registryHive == Registry.CurrentUser ? "HKEY_CURRENT_USER" : registryHive == Registry.Users ? "HKEY_USERS" : registryHive.ToString(), keyPath);
            }
            catch (UnauthorizedAccessException)
            {
                Log.Warning("Access denied to registry key: {RegistryHive}\\{KeyPath}. Administrative privileges may be required.", registryHive == Registry.CurrentUser ? "HKEY_CURRENT_USER" : registryHive == Registry.Users ? "HKEY_USERS" : registryHive.ToString(), keyPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unable to delete registry key {RegistryHive}\\{KeyPath}", registryHive == Registry.CurrentUser ? "HKEY_CURRENT_USER" : registryHive == Registry.Users ? "HKEY_USERS" : registryHive.ToString(), keyPath);
            }
        }
    }
}