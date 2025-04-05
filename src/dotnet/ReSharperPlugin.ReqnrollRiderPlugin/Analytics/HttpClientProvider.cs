using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using JetBrains.Application;
using JetBrains.Util;

namespace ReSharperPlugin.ReqnrollRiderPlugin.Analytics;

[ShellComponent]
public class HttpClientProvider(ILogger logger) : IHttpClientProvider
{
    private static readonly HttpClient Client = new HttpClient();
    static HttpClientProvider()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
    }

    public async Task<HttpResultString> PostStringAsync(Uri requestUri, string content)
    {
        try
        {
            var response = await Client.PostAsync(requestUri, new StringContent(content));

            return new HttpResultString
            {
                Success = response.IsSuccessStatusCode,
                StatusCode = response.StatusCode,
                Value = response.IsSuccessStatusCode ? await response.Content.ReadAsStringAsync() : null
            };
        }
        catch (HttpRequestException e) when (e.InnerException != null)
        {
            logger.Verbose(e.InnerException.Message);
            return null;
        }
        catch (Exception e)
        {
            logger.Verbose(e.Message);
            return null;
        }
    }
}