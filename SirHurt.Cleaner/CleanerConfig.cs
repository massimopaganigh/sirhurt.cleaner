using System;
using System.Linq;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Configuration settings for system cleaning operations
    /// </summary>
    public class CleanerConfig
    {
        /// <summary>
        /// Files that require confirmation before deletion, relative to their base folders
        /// </summary>
        public IReadOnlyList<string> FilesRequiringConfirmation { get; }

        /// <summary>
        /// System folder paths to delete
        /// </summary>
        public IReadOnlyList<string> SystemFolders { get; }

        /// <summary>
        /// Registry keys to delete
        /// </summary>
        public IReadOnlyList<string> RegistryKeys { get; }

        /// <summary>
        /// Process names that must be closed before cleaning
        /// </summary>
        public IReadOnlyList<string> ProcessesToClose { get; }

        public CleanerConfig()
        {
            FilesRequiringConfirmation = new List<string>
            {
                Path.Combine("sirhui", "sirhurta.dat"),
                Path.Combine("sirhui", "sirhurtp.dat")
            };

            SystemFolders = new List<string>
            {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "rsTrust")
            };

            RegistryKeys = new List<string>
            {
                @"Software\Asshurt"
            };

            ProcessesToClose = new List<string>
            {
                "RobloxPlayerBeta",
                "SirHurtUI"
            };
        }
    }
}