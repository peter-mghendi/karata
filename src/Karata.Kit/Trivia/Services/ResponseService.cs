using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class ResponseService(RestClient client)
{
    public async Task<List<ResponseData>> CreateResponse(Guid identifier, int index, ResponseRequest request)
    {
        var result = await client.PostJsonAsync<ResponseRequest, List<ResponseData>>($"games/{identifier}/rounds/{index}/responses", request);
        return result!;
    } 
}