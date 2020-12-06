using System;
using System.IO;
using Baseline;

namespace Alba
{
    public static class DirectoryFinder
    {
        /// <summary>
        /// Tries to find the correct content path for a project that is parallel to the 
        /// testing project
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public static string? FindParallelFolder(string? folderName)
        {
            var starting = AppContext.BaseDirectory.ToFullPath();
            
            // HACK ALERT!! but it does work
            if (starting.Contains("dotnet-xunit"))
            {
                starting = Directory.GetCurrentDirectory();
            }
            
            
            while (starting.Contains(Path.DirectorySeparatorChar + "bin"))
            {
                starting = starting.ParentDirectory();
            }
            

            var candidate = starting.ParentDirectory().AppendPath(folderName);
            

            return Directory.Exists(candidate) ? candidate : null;
        }
    }
}