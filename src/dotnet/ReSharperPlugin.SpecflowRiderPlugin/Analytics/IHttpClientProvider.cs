using System;
using System.Threading.Tasks;

namespace ReSharperPlugin.SpecflowRiderPlugin.Analytics
{
    public interface IHttpClientProvider
    {
        Task<HttpResultString> PostStringAsync(Uri requestUri, string content);
    }
}