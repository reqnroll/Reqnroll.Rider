using System.Net;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public class HttpResultString
    {
        public bool Success { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string Value { get; set; }
    }
}