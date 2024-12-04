using System.IO.Compression;

namespace SetupPython;

public static class PythonSetup
{
    public static async Task PreparePython(PythonSetupSettings settings)
    {
        
        
        
        CleanDirectory(settings);

        // CREATE THE EXTRACTION DIRECTORY
        Directory.CreateDirectory(settings.PythonExtractDir);

        // DOWNLOAD PYTHON ZIP FILE
        await DownloadFile(settings.PythonDownloadURL, settings.PythonDownloadZip);

        // DOWNLOAD PIP FILE
        string pipPath = Path.Combine(settings.PythonExtractDir, "pip.pyz");
        await DownloadFile(settings.PipDownloadURL, pipPath);

        // CREATE BASE PYTHON INSTALLATION
        await CreateBasePythonInstallation(settings, settings.PythonDownloadZip);

        // REMOVE DOWNLOADED ZIP FILE
        File.Delete(settings.PythonDownloadZip);
    }

    private static async Task CreateBasePythonInstallation(PythonSetupSettings settings, string pythonZip)
    {
        // EXTRACT THE Python ZIP FILE
        await ExtractZip(pythonZip, settings.PythonExtractDir);

        // EXTRACT THE EMBEDDED PYTHON INTERIOR ZIP FILE
        string interiorZip = Path.Combine(settings.PythonExtractDir, settings.PythonInteriorZip);
        if (File.Exists(interiorZip))
        {
            await ExtractZip(interiorZip, settings.PythonExtractDir);
            File.Delete(interiorZip);
        }

        // UPDATE ._PTH FILE
        await UpdatePthFile(settings);

        // CREATE SITECUSTOMIZE.PY FILE
        await CreateSiteCustomFile(settings);

        // MAKE EMPTY DLLs FOLDER
        Directory.CreateDirectory(Path.Combine(settings.PythonExtractDir, "DLLs"));
    }

    private static void CleanDirectory(PythonSetupSettings settings)
    {
        if (Directory.Exists(settings.PythonExtractDir))
            Directory.Delete(settings.PythonExtractDir, recursive: true);

        if (File.Exists(settings.PythonDownloadZip))
            File.Delete(settings.PythonDownloadZip);

        Console.WriteLine("Directory cleaned.");
    }

    private static async Task DownloadFile(string url, string destination)
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        await using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None);
        await response.Content.CopyToAsync(fileStream);
    }

    private static async Task ExtractZip(string zipPath, string extractPath)
    {
        await Task.Run(() => ZipFile.ExtractToDirectory(zipPath, extractPath));
    }

    private static async Task CreateSiteCustomFile(PythonSetupSettings settings)
    {
        string siteCustomizePath = Path.Combine(settings.PythonExtractDir, "sitecustomize.py");
        await File.WriteAllTextAsync(siteCustomizePath, "import sys\nsys.path.append('.')\n");
    }

    private static async Task UpdatePthFile(PythonSetupSettings settings)
    {
        string pthFilePath = Path.Combine(settings.PythonExtractDir, settings.PthFile);
        await File.WriteAllTextAsync(pthFilePath,
            $".\\{settings.PythonExtractDir}\n.\\Scripts\n.\n.\\Lib\\site-packages\nimport site\n");
    }
}