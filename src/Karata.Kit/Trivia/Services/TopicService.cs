using Karata.Kit.Trivia.Models;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class TopicService(RestClient client)
{
    public async Task<List<TopicData>> GetTopics()
    {
        var response = await client.GetAsync<List<TopicData>>("topics");
        return response!;
    } 
}