using System;
using Baseline.Testing;

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

    }
}