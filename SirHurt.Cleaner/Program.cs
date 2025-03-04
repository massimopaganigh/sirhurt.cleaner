using Serilog;

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