using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainStateManager.Helpers
{
    public class ShellHelper
    {
        public async static Task<bool> PerformShellCommandAndLeave(string commandName, string commandParams,
            Action<Process> processStartedCallback, string workingDiectory = null, string waitForString = null, bool redirectOutput = true)
        {
            bool exitFromMethod = false;

            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            if (!string.IsNullOrEmpty(waitForString))
            {
                processStartInfo.RedirectStandardOutput = true;
                processStartInfo.RedirectStandardError = true;
                processStartInfo.UseShellExecute = false;
            }
            processStartInfo.FileName = commandName;
            processStartInfo.Arguments = commandParams;
            if (!string.IsNullOrEmpty(workingDiectory))
            {
                processStartInfo.WorkingDirectory = workingDiectory;
            }

            Process p = new Process();
            if (!string.IsNullOrEmpty(waitForString))
            {
                using (var ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms);

                    p.OutputDataReceived += (
                        (object sender, DataReceivedEventArgs e) =>
                        {
                            if (e.Data != null && e.Data.Equals(waitForString))
                            {
                                exitFromMethod = true;
                            }
                        });

                    p.ErrorDataReceived += (
                        (object sender, DataReceivedEventArgs e) =>
                        {
                            if (e.Data != null && e.Data.Equals(waitForString))
                            {
                                exitFromMethod = true;
                            }
                        });
                }
            }

            p.StartInfo = processStartInfo;
            p.Start();

            processStartedCallback(p);

            if (!string.IsNullOrEmpty(waitForString))
            {
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                var counter = 0;
                while (!exitFromMethod)
                {
                    await Task.Delay(1000);
                    counter++;
                    if (counter > 30)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return true;
            }
        }

        public static bool PerformShellCommandAndExit(string commandName, string commandParams)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo();
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.FileName = commandName;
            processStartInfo.Arguments = commandParams;

            Process p = new Process();
            using (var ms = new MemoryStream())
            {
                var sw = new StreamWriter(ms);

                p.OutputDataReceived += (
                    (object sender, DataReceivedEventArgs e) =>
                    {
                        sw.WriteLine(e.Data);
                    });

                p.ErrorDataReceived += (
                    (object sender, DataReceivedEventArgs e) =>
                    {
                        sw.Write(e.Data);
                    });

                p.StartInfo = processStartInfo;

                p.Start();
                p.BeginErrorReadLine();
                p.BeginOutputReadLine();
                p.WaitForExit();
                sw.Flush();

                ms.Position = 0;
                var sr = new StreamReader(ms);
                var myStr = sr.ReadToEnd();

                if (p.ExitCode == 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
