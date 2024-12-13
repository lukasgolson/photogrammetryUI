using System.IO.Compression;

namespace PythonBindings;

public static class PythonSetup
{
    public static async Task DownloadPython(string destinationDir, string pythonDownloadUrl,
        string pipDownloadUrl, string interiorZipName = "python311.zip",
        string pthFileName = "python311._pth")
    {
        if (Directory.Exists(destinationDir))
        {
            throw new IOException("The provided Python directory already exists.");
        }

        // CREATE THE EXTRACTION DIRECTORY
        Directory.CreateDirectory(destinationDir);

        // DOWNLOAD PYTHON ZIP FILE
        const string pythonZip = "Python.zip";
        await DownloadFile(pythonDownloadUrl, pythonZip);

        await ExtractZip(pythonZip, destinationDir);

        File.Delete(pythonZip);

        // DOWNLOAD PIP FILE
        var pipPath = Path.Combine(destinationDir, "pip.pyz");
        await DownloadFile(pipDownloadUrl, pipPath);

        // CREATE BASE PYTHON INSTALLATION
        await CreateBasePythonInstallation(destinationDir, interiorZipName, pthFileName);
    }

    private static async Task CreateBasePythonInstallation(string pythonDir, string pythonInteriorZip,
        string pthFileName)
    {
        // EXTRACT THE EMBEDDED PYTHON INTERIOR ZIP FILE
        var interiorZip = Path.Combine(pythonDir, pythonInteriorZip);
        if (File.Exists(interiorZip))
        {
            await ExtractZip(interiorZip, pythonDir);
            File.Delete(interiorZip);
        }

        // UPDATE ._PTH FILE TO INCLUDE SITE PACKAGES
        await UpdatePthFile(pythonDir, pthFileName);

        // CREATE SITECUSTOMIZE.PY FILE
        await CreateSiteCustomFile(pythonDir);

        // MAKE EMPTY DLLs FOLDER
        Directory.CreateDirectory(Path.Combine(pythonDir, "DLLs"));
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

    private static async Task CreateSiteCustomFile(string pythonDir)
    {
        var siteCustomizePath = Path.Combine(pythonDir, "sitecustomize.py");
        await File.WriteAllTextAsync(siteCustomizePath, "import sys\nsys.path.append('.')\n");
    }

    private static async Task UpdatePthFile(string pythonDir, string pthFile)
    {
        var pthFilePath = Path.Combine(pythonDir, pthFile);
        await File.WriteAllTextAsync(pthFilePath,
            $".\\{pythonDir}\n.\\Scripts\n.\n.\\Lib\\site-packages\nimport site\n");
    }
}