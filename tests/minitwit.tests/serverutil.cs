using System.Diagnostics;

namespace minitwit.tests;

public class serverutil
{
    public static async Task<Process> StartApp()
    {
        var serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = "docker compose up --build",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        serverProcess.Start();
        await Task.Delay(10000);

        return serverProcess;

    }
}