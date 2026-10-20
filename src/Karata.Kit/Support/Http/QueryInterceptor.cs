using RestSharp;
using RestSharp.Interceptors;

namespace Karata.Kit.Support.Http;

public class QueryInterceptor : Interceptor
{
    public override ValueTask BeforeHttpRequest(HttpRequestMessage message, CancellationToken cancellationToken)
    {
        message.Method = HttpMethod.Query;
        return ValueTask.CompletedTask;
    }
}