using System;
using System.IO;
using Baseline;
 
namespace Alba
{
    internal static class DirectoryFinder
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
            
            
#pragma warning disable 8602
            while (starting.Contains(Path.DirectorySeparatorChar + "bin"))
#pragma warning restore 8602
            {
                starting = starting.ParentDirectory();
            }
            

#pragma warning disable 8604
            var candidate = starting.ParentDirectory().AppendPath(folderName);
#pragma warning restore 8604
            

            return Directory.Exists(candidate) ? candidate : null;
        }
    }
}