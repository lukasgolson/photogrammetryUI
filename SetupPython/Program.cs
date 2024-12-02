using System.Text.Json;

namespace SetupPython;

public class Program
{
    static async Task Main(string[] args)
    {
        // Load configuration
        string configFile = args.Length > 0 ? args[0] : "python-setup-config.json";

        if (!File.Exists(configFile))
        {
            Console.WriteLine($"Configuration file not found: {configFile}");
            return;
        }

        var settings = await LoadConfigurationAsync(configFile);

        // Validate settings
        if (settings == null)
        {
            Console.WriteLine("Invalid configuration file.");
            return;
        }

        Console.WriteLine("Starting Python setup...");

        // Run the Python preparation tool


        await PythonSetup.PreparePython(settings);


        Console.WriteLine("Python setup completed successfully.");
    }

    private static async Task<PythonSetupSettings?> LoadConfigurationAsync(string configFilePath)
    {
        await using var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read);
        var config = await JsonSerializer.DeserializeAsync<PythonSetupSettings>(stream) ?? null;

        // validate the configuration
        if (config == null)
        {
            Console.WriteLine("Invalid configuration file.");
            return null;
        }

        var missingFields = new List<string>();

        if (string.IsNullOrEmpty(config.PythonExtractDir)) missingFields.Add(nameof(config.PythonExtractDir));
        if (string.IsNullOrEmpty(config.PythonDownloadZip)) missingFields.Add(nameof(config.PythonDownloadZip));
        if (string.IsNullOrEmpty(config.PythonDownloadURL)) missingFields.Add(nameof(config.PythonDownloadURL));
        if (string.IsNullOrEmpty(config.PipDownloadURL)) missingFields.Add(nameof(config.PipDownloadURL));
        if (string.IsNullOrEmpty(config.PythonInteriorZip)) missingFields.Add(nameof(config.PythonInteriorZip));
        if (string.IsNullOrEmpty(config.PthFile)) missingFields.Add(nameof(config.PthFile));

        if (missingFields.Any())
        {
            Console.WriteLine("Invalid configuration file: missing required fields:");
            foreach (var field in missingFields)
            {
                Console.WriteLine($" - {field}");
            }
            return null;
        }

        return config;
    }
}