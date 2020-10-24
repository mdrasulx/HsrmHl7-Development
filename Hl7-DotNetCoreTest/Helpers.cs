using System;
using System.IO;
using System.Reflection;

namespace Hl7_DotNetCoreTest
{
    internal static class Helpers
    {
        public static string GetTestFileContent(string fileName, string relativePath = "test files")
        {
            return System.IO.File.ReadAllText(GetTestFilePath(fileName, relativePath));
        }

        public static string GetTestFilePath(string fileName, string relativePath = "test files")
        {
            var codeBase = Assembly.GetExecutingAssembly().CodeBase;
            if (codeBase == null)
                throw new Exception("Could not find code base");
            var codeBaseUrl = new Uri(codeBase);
            var codeBasePath = Uri.UnescapeDataString(codeBaseUrl.AbsolutePath);
            var dirPath = Path.GetDirectoryName(codeBasePath);
            if (dirPath == null)
                throw new Exception("Could not find dirPath");
            return Path.Combine(dirPath, relativePath, fileName);
        }

    }
}
