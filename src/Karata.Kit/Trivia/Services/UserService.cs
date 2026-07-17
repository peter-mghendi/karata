using Karata.Kit.Cards.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class UserService(RestClient client)
{
    public async Task<List<UserData>> GetUsers()
    {
        var response = await client.GetAsync<List<UserData>>("users");
        return response!;
    } 
}