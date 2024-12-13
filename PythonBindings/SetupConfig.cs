using System.Text.Json;

namespace PythonBindings;

public class SetupConfig
{
    public required string PythonDownloadUrl { get; init; }
    public required string PipDownloadUrl { get; init; }
    public required string PythonInteriorZipFile { get; init; }
    public required string PthFileName { get; init; }


    public static async Task<SetupConfig?> LoadConfigurationAsync(string configFilePath)
    {
        await using var stream = new FileStream(configFilePath, FileMode.Open, FileAccess.Read);
        return await JsonSerializer.DeserializeAsync<SetupConfig>(stream, options: new()
        {
            PropertyNameCaseInsensitive = true,
        });
    }
}