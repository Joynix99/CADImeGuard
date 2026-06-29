using System;
using System.IO;

namespace CadIME.Common
{
    public static class PathHelper
    {
        public static string GetConfigPath()
        {
            try
            {
                string codeBase = typeof(PathHelper).Assembly.Location;
                if (string.IsNullOrWhiteSpace(codeBase))
                {
                    codeBase = AppDomain.CurrentDomain.BaseDirectory;
                }
                string? dllDir = Path.GetDirectoryName(codeBase);
                return !string.IsNullOrWhiteSpace(dllDir) ? Path.Combine(dllDir, "CadIME.json") : "C:\\CadIME.json";
            }
            catch
            {
                return "C:\\CadIME.json";
            }
        }

        public static string GetLogFilePath()
        {
            try
            {
                string systemTemp = Environment.GetEnvironmentVariable("TEMP") ?? Path.GetTempPath();
                return Path.Combine(systemTemp, $"CadIME_2027_{DateTime.Now:yyyyMMdd}.log");
            }
            catch
            {
                return "C:\\CadIME_Fallback.log";
            }
        }
    }
}
