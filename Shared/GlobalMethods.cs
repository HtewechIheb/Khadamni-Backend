using System;
using System.IO;

namespace Project_X.Shared
{
    public static class GlobalMethods
    {
        public static string GenerateFileName(string prefix, string fileName)
        {
            return $"{prefix}_{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        }
    }
}
