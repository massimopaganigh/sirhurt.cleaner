﻿using Serilog;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Entry point for the SirHurt cleanup utility.
    /// Removes Roblox and SirHurt-related folders and registry keys.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main application entry point. Initializes logging, displays a banner,
        /// and executes the system cleanup process.
        /// </summary>
        /// <returns>Asynchronous task</returns>
        static async Task Main()
        {
            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Console().CreateLogger();

            try
            {
                Log.Information(@"
   ▄████████  ▄█     ▄████████    ▄█    █▄    ███    █▄     ▄████████     ███      ▄████████  ▄█          ▄████████    ▄████████ ███▄▄▄▄      ▄████████    ▄████████ 
  ███    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███ ▀█████████▄ ███    ███ ███         ███    ███   ███    ███ ███▀▀▀██▄   ███    ███   ███    ███ 
  ███    █▀  ███▌   ███    ███   ███    ███   ███    ███   ███    ███    ▀███▀▀██ ███    █▀  ███         ███    █▀    ███    ███ ███   ███   ███    █▀    ███    ███ 
  ███        ███▌  ▄███▄▄▄▄██▀  ▄███▄▄▄▄███▄▄ ███    ███  ▄███▄▄▄▄██▀     ███   ▀ ███        ███        ▄███▄▄▄       ███    ███ ███   ███  ▄███▄▄▄      ▄███▄▄▄▄██▀ 
▀███████████ ███▌ ▀▀███▀▀▀▀▀   ▀▀███▀▀▀▀███▀  ███    ███ ▀▀███▀▀▀▀▀       ███     ███        ███       ▀▀███▀▀▀     ▀███████████ ███   ███ ▀▀███▀▀▀     ▀▀███▀▀▀▀▀   
         ███ ███  ▀███████████   ███    ███   ███    ███ ▀███████████     ███     ███    █▄  ███         ███    █▄    ███    ███ ███   ███   ███    █▄  ▀███████████ 
   ▄█    ███ ███    ███    ███   ███    ███   ███    ███   ███    ███     ███     ███    ███ ███▌    ▄   ███    ███   ███    ███ ███   ███   ███    ███   ███    ███ 
 ▄████████▀  █▀     ███    ███   ███    █▀    ████████▀    ███    ███    ▄████▀   ████████▀  █████▄▄██   ██████████   ███    █▀   ▀█   █▀    ██████████   ███    ███ 
                    ███    ███                             ███    ███                        ▀                                                            ███    ███ 
");

                Log.Information("Starting cleanup program...");

                // Ask the user if they want to clean temp folders
                var config = new CleanerConfig();
                Console.Write("Would you like to clean temporary folders? (Y/N, default: Y): ");
                var response = Console.ReadLine();
                
                config.CleanTempFolders = response == null || 
                                         response.Trim().Equals("", StringComparison.OrdinalIgnoreCase) || 
                                         response.Trim().Equals("y", StringComparison.OrdinalIgnoreCase) ||
                                         response.Trim().Equals("yes", StringComparison.OrdinalIgnoreCase);

                await SystemCleaner.RunCleanupAsync(config).ConfigureAwait(false);

                Log.Information("Press any key to exit...");
                Console.ReadKey();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred in the main program");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}