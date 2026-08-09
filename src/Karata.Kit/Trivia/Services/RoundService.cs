using Karata.Kit.Trivia.Models.Response;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class RoundService(RestClient client)
{
    public async Task<List<RoundResponse>> GetRounds(Guid identifier)
    {
        var response = await client.GetAsync<List<RoundResponse>>($"games/{identifier}/rounds");
        return response!;
    }
    
    public async Task<RoundResponse> GetRound(Guid identifier, int index)
    {
        var response = await client.GetAsync<RoundResponse>($"games/{identifier}/rounds/{index}");
        return response!;
    } 
}