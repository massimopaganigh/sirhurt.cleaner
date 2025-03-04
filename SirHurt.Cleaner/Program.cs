using Serilog;

namespace SirHurt.Cleaner
{
    /// <summary>
    /// Main entry point class for the cleanup utility application.
    /// This program removes specific folders and registry keys related to Roblox and SirHurt.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The main entry point for the application. Displays a banner, initializes the cleaner,
        /// and executes the cleanup operations asynchronously.
        /// </summary>
        /// <param name="args">Command line arguments (not used in this application)</param>
        /// <returns>A task representing the asynchronous operation</returns>
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

                await SystemCleaner.RunCleanupAsync().ConfigureAwait(false);

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