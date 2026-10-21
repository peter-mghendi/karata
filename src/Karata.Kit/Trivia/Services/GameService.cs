using Karata.Kit.Common.Models;
using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class GameService(RestClient client)
{
    public async Task<Page<GameData>> QueryAsync(GameQuery query, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync<Page<GameData>>("games",  query, cancellationToken);
        return response!;
    }
    
    public async Task<GameData> CreateAsync(GameRequest request, CancellationToken cancellationToken = default)
    {
        var response = await client.PostJsonAsync<GameRequest, GameData>("games", request, cancellationToken);
        return response!;
    } 
    
    public async Task<GameData> FetchAsync(Guid identifier, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync<GameData>($"games/{identifier}", cancellationToken);
        return response!;
    } 
}