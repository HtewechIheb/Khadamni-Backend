using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Project_X.Shared
{
    public static class GlobalConstants
    {
        public const string PhotoPrefix = "photo";

        public const string ResumePrefix = "resume";

        public static string GenerateFileName(string prefix, string fileName)
        {
            return $"{prefix}_{Guid.NewGuid().ToString()}{Path.GetExtension(fileName)}";
        }
    }
}
