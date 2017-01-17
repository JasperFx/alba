using System.Collections.Generic;

namespace Alba.StaticFiles
{
    public class WriteFileContinuation : WriterContinuation
    {
        private readonly AssetSettings _settings;

        public WriteFileContinuation(IDictionary<string, object> env, IStaticFile file, AssetSettings settings)
            : base(env, DoNext.Stop)
        {
            File = file;
            _settings = settings;
        }

        public IStaticFile File { get; }

        public override void Write(IDictionary<string, object> response)
        {
            response.WriteFile(File.Path);

            WriteFileHeadContinuation.WriteHeaders(response, File);

            var headers = response.ResponseHeaders();
            _settings.Headers.Each((key, source) => { headers.Append(key, source()); });

            response.StatusCode(200);
        }

        public override string ToString()
        {
            return $"Write file: {File}";
        }

        protected bool Equals(WriteFileContinuation other)
        {
            return Equals(File, other.File);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((WriteFileContinuation) obj);
        }

        public override int GetHashCode()
        {
            return (File != null ? File.GetHashCode() : 0);
        }
    }
}