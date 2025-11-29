using System.Diagnostics;

namespace ModCreator.Helpers
{
    /// <summary>
    /// Helper for Python-related operations
    /// </summary>
    public static class PythonHelper
    {
        /// <summary>
        /// Find Python executable in system PATH
        /// </summary>
        /// <returns>Python command name if found, null otherwise</returns>
        public static string FindPythonExecutable()
        {
            var pythonCommands = new[] { "python", "python3", "py" };
            
            foreach (var cmd in pythonCommands)
            {
                try
                {
                    var processInfo = new ProcessStartInfo
                    {
                        FileName = cmd,
                        Arguments = "--version",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        CreateNoWindow = true
                    };
                    
                    using (var process = Process.Start(processInfo))
                    {
                        if (process != null)
                        {
                            process.WaitForExit(1000);
                            if (process.ExitCode == 0)
                                return cmd;
                        }
                    }
                }
                catch { }
            }
            
            return null;
        }
    }
}
