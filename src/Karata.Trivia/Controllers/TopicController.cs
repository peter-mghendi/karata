using Karata.Kit.Trivia.Models.Response;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Karata.Trivia.Controllers;

[Authorize]
[ApiController]
[Route("api/topics")]
public class TopicController(TriviaContext context) : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<IEnumerable<TopicResponse>> GetTopics() => await context.Topics
        .Select(t => t.AsResponse())
        .ToListAsync();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TopicResponse>> GetTopic(long id)
    {
        var topic = await context.Topics.FindAsync(id);
        if (topic is null) return NotFound();

        return topic.AsResponse();
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> PutTopic(long id, Topic topic)
    {
        if (id != topic.Id) return BadRequest();

        context.Entry(topic).State = EntityState.Modified;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException) when (!TopicExists(id))
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<TopicResponse>> PostTopic(Topic topic)
    {
        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetTopic", new { id = topic.Id }, topic.AsResponse());
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteTopic(long id)
    {
        var topic = await context.Topics.FindAsync(id);
        if (topic == null) return NotFound();

        context.Topics.Remove(topic);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool TopicExists(long id) => context.Topics.Any(e => e.Id == id);
}