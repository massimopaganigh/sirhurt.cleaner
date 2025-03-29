﻿using Serilog;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Removes SirHurt and Roblox-related components from the filesystem and Windows registry.
    /// Handles cleanup operations for the current user and other user profiles on the system.
    /// Also cleans temporary folders for the system and all users.
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
                // Create dependencies
                var logger = Log.Logger;
                var fileSystem = new StandardFileSystem();
                var registry = new StandardRegistry();
                var userInteraction = new ConsoleUserInteraction();
                var processManager = new StandardProcessManager(logger);
                var config = new CleanerConfig();

                // Create component cleaners
                var cleanerCore = new CleanerCore(logger, fileSystem, userInteraction, processManager, config);
                var registryCleaner = new RegistryCleaner(logger, registry, config);
                var tempCleaner = new TempFolderCleaner(logger, fileSystem, cleanerCore);

                await Task.Run(() =>
                {
                    // Ensure Roblox and SirHurt are closed before cleaning
                    if (!cleanerCore.EnsureApplicationsClosed())
                    {
                        logger.Warning("Proceeding with cleanup, but some operations may fail because applications are still running");
                    }

                    // Clean system folders
                    logger.Information("Checking system-wide folders");
                    foreach (var folderPath in config.SystemFolders)
                    {
                        logger.Information("Attempting to delete system folder: {FolderPath}", folderPath);
                        cleanerCore.DeleteFolder(folderPath);
                    }

                    // Clean current user folders
                    logger.Information("Checking folders for current user");
                    string robloxFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Roblox");
                    cleanerCore.DeleteFolder(robloxFolder);

                    string sirhurtFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "sirhurt");
                    cleanerCore.DeleteFolderContentsWithConfirmation(sirhurtFolder);

                    // Clean all user profiles
                    CleanupAllUserProfiles(logger, fileSystem, cleanerCore);

                    // Clean registry
                    registryCleaner.CleanCurrentUserRegistry();
                    registryCleaner.CleanAllUsersRegistry();

                    // Clean temporary folders
                    logger.Information("Starting temporary folder cleanup");
                    tempCleaner.CleanCurrentUserTempFolders();
                    tempCleaner.CleanAllUsersTempFolders();
                    tempCleaner.CleanSystemTempFolders();

                }).ConfigureAwait(false);

                logger.Information("Operations completed successfully.");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred during cleanup operations");
            }
        }

        /// <summary>
        /// Removes application folders from all other user profiles on the system.
        /// </summary>
        private static void CleanupAllUserProfiles(ILogger logger, IFileSystem fileSystem, CleanerCore cleanerCore)
        {
            try
            {
                logger.Information("Checking other user profiles on the system");

                string usersFolder = Path.Combine(Environment.GetEnvironmentVariable("SystemDrive") ?? "C:", "Users");

                if (!fileSystem.DirectoryExists(usersFolder))
                {
                    logger.Warning("Users folder not found at {UsersFolder}", usersFolder);
                    return;
                }

                // Get current username to skip it (already processed by CleanupCurrentUserFolders)
                string currentUsername = Environment.UserName;
                logger.Information("Current user is: {Username}, will skip this profile", currentUsername);

                var userProfiles = fileSystem.GetDirectories(usersFolder)
                    .Where(dir =>
                        !fileSystem.GetFileName(dir).Equals(currentUsername, StringComparison.OrdinalIgnoreCase) &&
                        !dir.EndsWith("Public") &&
                        !dir.EndsWith("Default") &&
                        !dir.EndsWith("Default User") &&
                        !dir.EndsWith("All Users"));

                logger.Information("Found {Count} other user profiles to check", userProfiles.Count());

                foreach (var userProfile in userProfiles)
                {
                    try
                    {
                        string username = fileSystem.GetFileName(userProfile);
                        logger.Information("Checking profile for user: {Username}", username);

                        string localAppData = Path.Combine(userProfile, "AppData", "Local");
                        if (fileSystem.DirectoryExists(localAppData))
                        {
                            string robloxPath = Path.Combine(localAppData, "Roblox");
                            cleanerCore.DeleteFolder(robloxPath);
                        }

                        string roamingAppData = Path.Combine(userProfile, "AppData", "Roaming");
                        if (fileSystem.DirectoryExists(roamingAppData))
                        {
                            string sirhurtPath = Path.Combine(roamingAppData, "sirhurt");
                            cleanerCore.DeleteFolderContentsWithConfirmation(sirhurtPath);
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        logger.Warning(ex, "Access denied to user profile: {UserProfile}", userProfile);
                    }
                    catch (Exception ex)
                    {
                        logger.Error(ex, "Error processing user profile: {UserProfile}", userProfile);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Failed to process user profiles");
            }
        }
    }
}