namespace Alba
{
    public class HttpGeneralHeaders
    {
        public static readonly string CacheControl = "Cache-Control";
        public static readonly string Connection = "Connection";
        public static readonly string Date = "Date";
        public static readonly string Pragma = "Pragma";
        public static readonly string Trailer = "Trailer";
        public static readonly string TransferEncoding = "Transfer-Encoding";
        public static readonly string Upgrade = "Upgrade";
        public static readonly string Via = "Via";
        public static readonly string Warning = "Warning";


        // Entity fields
        public static readonly string Allow = "Allow";
        public static readonly string ContentEncoding = "Content-Encoding";
        public static readonly string ContentLanguage = "Content-Language";
        public static readonly string ContentLocation = "Content-Location";
        public static readonly string ContentRange = "Content-Range";
        public static readonly string Expires = "Expires";
        public static readonly string LastModified = "Last-Modified";

        protected HttpGeneralHeaders()
        {
        }
    }
}