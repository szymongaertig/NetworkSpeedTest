using System.Diagnostics;
using System.Text;

namespace NetworkSpeedTest
{
    public static class SpeedTestCmdToolTest
    {
        public static bool Exists(string path)
        {
            try
            {
                using var process = new Process();
                process.StartInfo = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    FileName = path,
                    Arguments = "--version",
                    CreateNoWindow = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.ASCII
                };
                process.Start();
                var standardError = process.StandardOutput.ReadToEnd();
                return standardError.Contains("Speedtest by Ookla");
            }
            catch
            {
                return false;
            }
        }
    }
}