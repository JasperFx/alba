using System.Collections.Generic;
using System.Net;

namespace Alba.StaticFiles
{
    public class WriteStatusCodeContinuation : WriterContinuation
    {
        public WriteStatusCodeContinuation(IDictionary<string, object> env, int status, string reason)
            : base(env, DoNext.Stop)
        {
            Status = status;
            Reason = reason;
        }

        public override void Write(IDictionary<string, object> response)
        {
            response.StatusCode(Status, Reason);
        }

        public int Status { get; }

        public string Reason { get; }

        protected bool Equals(WriteStatusCodeContinuation other)
        {
            return Status == other.Status && string.Equals(Reason, other.Reason);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((WriteStatusCodeContinuation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Status*397) ^ (Reason != null ? Reason.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return string.Format("Stopping with Code: {0}, Reason: {1}", Status, Reason);
        }
    }
}