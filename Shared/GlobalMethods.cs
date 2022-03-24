using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
