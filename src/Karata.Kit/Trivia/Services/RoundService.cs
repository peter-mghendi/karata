using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class RoundService(RestClient client)
{
    public async Task<List<RoundData>> GetRounds(Guid identifier)
    {
        var response = await client.GetAsync<List<RoundData>>($"games/{identifier}/rounds");
        return response!;
    }
    
    public async Task<RoundData> GetRound(Guid identifier, int index)
    {
        var response = await client.GetAsync<RoundData>($"games/{identifier}/rounds/{index}");
        return response!;
    } 
}