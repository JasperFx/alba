using System;
using System.Collections.Generic;
using System.IO;
using Baseline;

namespace Alba.StaticFiles
{
    public class AssetSettings
    {
        /// <summary>
        /// The default maximum age in seconds to cache an asset in production mode. 1 day.
        /// </summary>
        public static readonly int MaxAgeInSeconds = 24 * 60 * 60;

        public AssetSettings()
        {
            var cacheHeader = "private, max-age={0}".ToFormat(MaxAgeInSeconds);

            Headers[HttpGeneralHeaders.CacheControl] = () => cacheHeader;
            Headers[HttpGeneralHeaders.Expires] = () => DateTime.UtcNow.AddSeconds(MaxAgeInSeconds).ToString("R");
        }

        /// <summary>
        /// The Http headers to be written when serving up static files
        /// </summary>
        public readonly Cache<string, Func<string>> Headers = new Cache<string, Func<string>>();


        /// <summary>
        /// Add additional file extensions as allowable assets
        /// </summary>
        public IList<string> AllowableExtensions = new List<string> { ".eot", ".ttf", ".woff", ".woff2", ".svg", ".map" };


        public bool IsAllowed(IStaticFile file)
        {
            var extension = Path.GetExtension(file.Path);
            if (extension.EqualsIgnoreCase(".config")) return false;

            var mimetype = MimeType.MimeTypeByFileName(file.Path);
            if (mimetype == null) return false;

            if (mimetype == MimeType.Javascript) return true;

            if (mimetype == MimeType.Css) return true;

            if (mimetype == MimeType.Html) return true;

            if (mimetype.Value.StartsWith("image/")) return true;

            
            if (AllowableExtensions.Contains(extension)) return true;

            return false;
        }
    }
}