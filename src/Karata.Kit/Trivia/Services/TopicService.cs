using Karata.Kit.Trivia.Models.Response;
using RestSharp;

namespace Karata.Kit.Trivia.Services;

public class TopicService(RestClient client)
{
    public async Task<List<TopicResponse>> GetTopics()
    {
        var response = await client.GetAsync<List<TopicResponse>>("topics");
        return response!;
    } 
}