using System.Diagnostics;

namespace minitwit.tests;

public class serverutil
{
    private static Process? _serverProcess;

    public static async Task<Process> StartApp()
    {
        _serverProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = "compose up --build -d",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        _serverProcess.Start();
        await Task.Delay(10000);

        return _serverProcess;
    }

    public static void StopApp()
    {
        if (_serverProcess != null && !_serverProcess.HasExited)
        {
            var stopProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = "compose down",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            stopProcess.Start();
            stopProcess.WaitForExit();
        }
    }
}