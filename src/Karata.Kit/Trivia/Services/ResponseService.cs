using Karata.Kit.Trivia.Models.Request;
using Karata.Kit.Trivia.Models.Response;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class ResponseService(RestClient client)
{
    public async Task<List<ResponseResponse>> CreateResponse(Guid identifier, int index, CreateResponseRequest request)
    {
        var result = await client.PostJsonAsync<CreateResponseRequest, List<ResponseResponse>>($"games/{identifier}/rounds/{index}/responses", request);
        return result!;
    } 
}