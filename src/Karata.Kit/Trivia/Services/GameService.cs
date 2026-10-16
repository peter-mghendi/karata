using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class GameService(RestClient client)
{
    public async Task<GameResponse> GetGame(Guid identifier)
    {
        var response = await client.GetAsync<GameResponse>($"games/{identifier}");
        return response!;
    } 
    
    public async Task<GameResponse> CreateGame(GameRequest request)
    {
        var response = await client.PostJsonAsync<GameRequest, GameResponse>("games", request);
        return response!;
    } 
}