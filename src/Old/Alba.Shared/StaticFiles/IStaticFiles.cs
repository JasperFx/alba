using System.IO;
using Baseline;

namespace Alba.StaticFiles
{
    public interface IStaticFiles
    {
        IStaticFile Find(string relativeUrl);
    }

    public class StaticFiles : IStaticFiles
    {
        private readonly string _root;

        public StaticFiles(string root)
        {
            _root = root;
        }

        public IStaticFile Find(string relativeUrl)
        {
            var path = _root.AppendPath(relativeUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(path))
            {
                return new StaticFile(path)
                {
                    RelativePath = relativeUrl
                };
            }

            return null;
        }
    }
}