using System.Diagnostics;
using System.IO;

namespace ResearchUI;

public class Python
{
    public Python()
    {
        // Double-check that the Python directory exists
        if (!Directory.Exists("Python"))
        {
            throw new DirectoryNotFoundException("Python directory not found.");
        }
        
        // Double-check that the Python executable exists
        if (!File.Exists("Python/python.exe"))
        {
            throw new FileNotFoundException("Python executable not found.");
        }
        
        // Run a test to ensure that Python is installed correctly
        //RunProcess("--version").Wait();
    }
    
    public async Task RunProcess(string arguments)
    {
        Process process = new Process();
        process.StartInfo.FileName = "Python/python.exe";
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