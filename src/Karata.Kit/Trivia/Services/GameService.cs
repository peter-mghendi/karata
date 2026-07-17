using Karata.Kit.Trivia.Models.Request;
using Karata.Kit.Trivia.Models.Response;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class GameService(RestClient client)
{
    public async Task<GameResponse> GetGame(Guid identifier)
    {
        var response = await client.GetAsync<GameResponse>($"games/{identifier}");
        return response!;
    } 
    
    public async Task<GameResponse> CreateGame(CreateGameRequest request)
    {
        var response = await client.PostJsonAsync<CreateGameRequest, GameResponse>("games", request);
        return response!;
    } 
}