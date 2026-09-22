using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class ResponseService(RestClient client)
{
    public async Task<List<ResponseResponse>> CreateResponse(Guid identifier, int index, ResponseRequest request)
    {
        var result = await client.PostJsonAsync<ResponseRequest, List<ResponseResponse>>($"games/{identifier}/rounds/{index}/responses", request);
        return result!;
    } 
}