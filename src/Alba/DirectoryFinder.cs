using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
 
namespace Alba
{
    // Delete this all in next semver
    internal static class DirectoryFinder
    {
        /// <summary>
        /// Tries to find the correct content path for a project that is parallel to the 
        /// testing project
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [Obsolete]
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

        public static string ToFullPath(this string path) => Path.GetFullPath(path);

        public static string? ParentDirectory(this string path) => Path.GetDirectoryName(path.TrimEnd(Path.DirectorySeparatorChar));

        public static string AppendPath(this string path, params string[] parts)
        {
            var stringList = new List<string> { path };
            stringList.AddRange(parts);
            return Combine(stringList.ToArray());
        }
        public static string Combine(params string[] paths) => (paths).Aggregate(Path.Combine); }
}