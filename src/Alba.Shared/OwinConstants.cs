namespace Alba
{
    public static class OwinConstants
    {
        public const string OwinVersion = "1.0";
        public const string CallCancelledKey = "owin.CallCancelled";

        public const string RequestBodyKey = "owin.RequestBody";
        public const string VersionKey = "owin.Version";
        public const string RequestHeadersKey = "owin.RequestHeaders";
        public const string RequestMethodKey = "owin.RequestMethod";
        public const string RequestPathBaseKey = "owin.RequestPathBase";
        public const string RequestPathKey = "owin.RequestPath";
        public const string RequestProtocolKey = "owin.RequestProtocol";
        public const string RequestQueryStringKey = "owin.RequestQueryString";
        public const string RequestSchemeKey = "owin.RequestScheme";
        public const string ResponseBodyKey = "owin.ResponseBody";
        public const string ResponseHeadersKey = "owin.ResponseHeaders";
        public const string ResponseProtocolKey = "owin.ResponseProtocol";
        public const string ResponseStatusCodeKey = "owin.ResponseStatusCode";
        public const string ResponseReasonPhraseKey = "owin.ReasonPhrase";

        public const string ClientCertifiateKey = "ssl.ClientCertificate";

        public const string ServerRemoteIpAddressKey = "server.RemoteIpAddress";
        public const string ServerRemotePortKey = "server.RemotePort";
        public const string ServerLocalIpAddressKey = "server.LocalIpAddress";
        public const string ServerLocalPortKey = "server.LocalPort";
        public const string ServerUserKey = "server.User";
        public const string ServerIsLocalKey = "server.IsLocal";

        public const string WebSocketSupportKey = "websocket.Support";
        public const string WebSocketFuncKey = "websocket.Func";
        public const string WebSocketSupport = "WebSocketFunc";

        public const string WwwAuthenticateHeader = "WWW-Authenticate";
        public const string AcceptHeader = "Accept";
        public const string ContentTypeHeader = "Content-Type";
        public const string ContentLengthHeader = "Content-Length";
        public const string TransferEncodingHeader = "Transfer-Encoding";
        public const string KeepAliveHeader = "Keep-Alive";
        public const string ConnectionHeader = "Connection";
        public const string HostHeader = "Host";

        //Fubu specific keys until owin has equivalents
        public const string MediaTypeKey = "fubu.MediaType";

        public const string AppMode = "host.AppMode";
        public const string HeaderSettings = "fubu.headerSettings";
    }
}