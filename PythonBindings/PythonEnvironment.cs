using System.Diagnostics;

namespace PythonBindings;

public class PythonEnvironment
{
    private string PythonDirectory { get; }

    public PythonEnvironment(string pythonDirectory = "Python")
    {
        if (!Directory.Exists(pythonDirectory))
        {
            throw new DirectoryNotFoundException("Python directory not found.");
        }

        if (!File.Exists(Path.Combine(pythonDirectory, "python.exe")))
        {
            throw new FileNotFoundException("Python executable not found.");
        }

        PythonDirectory = pythonDirectory;
    }

    public static async Task<PythonEnvironment> SetupPython(string pythonDirectory = "python",
        string configFile = "python-setup-config.json")
    {
        if (!File.Exists(configFile))
        {
            throw new FileNotFoundException("Python environment configuration file not found.");
        }

        var settings = await SetupConfig.LoadConfigurationAsync(configFile);

        // Validate settings
        if (settings == null)
        {
            throw new InvalidDataException("Invalid configuration file.");
        }

        return await SetupPython(pythonDirectory, settings.PythonDownloadUrl, settings.PipDownloadUrl,
            settings.PythonInteriorZipFile, settings.PthFileName);
    }

    public static async Task<PythonEnvironment> SetupPython(string pythonDirectory, string pythonDownloadUrl,
        string pipDownloadUrl, string pythonInteriorZipFile, string pthFileName)
    {
        if (IsInstalled())
        {
            return new PythonEnvironment(pythonDirectory);
        }

        await PythonSetup.DownloadPython(pythonDirectory, pythonDownloadUrl, pipDownloadUrl, pythonInteriorZipFile,
            pthFileName);

        var pythonEnvironment = new PythonEnvironment(pythonDirectory);

        return pythonEnvironment;
    }


    private static bool IsInstalled()
    {
        return File.Exists("Python/python.exe");
    }


    public async Task RunScript(string arguments)
    {
        if (!IsInstalled())
        {
            throw new FileNotFoundException("Python executable not found.");
        }

        var process = new Process();
        
        var fileName = Path.Combine(PythonDirectory, "python.exe");
        
        process.StartInfo.FileName = fileName;
        process.StartInfo.Arguments = arguments;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;


        process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
        process.ErrorDataReceived += (sender, e) => Console.WriteLine(e.Data);


        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync();
    }
}