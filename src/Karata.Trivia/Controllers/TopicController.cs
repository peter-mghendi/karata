using Karata.Kit.Trivia.Models;
using Karata.Trivia.Data;
using Karata.Trivia.Extensions;
using Karata.Trivia.Models;
using Karata.Trivia.Security;
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
    public async Task<IEnumerable<TopicData>> GetTopics() => await context.Topics
        .Select(t => t.ToData())
        .ToListAsync();

    [HttpGet("{id:long}")]
    public async Task<ActionResult<TopicData>> GetTopic(long id)
    {
        var topic = await context.Topics.FindAsync(id);
        if (topic is null) return NotFound();

        return topic.ToData();
    }

    [HttpPut("{id:long}")]
    [Authorize(Policy = nameof(TopicPolicy.ManageTopics))]
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
    [Authorize(Policy = nameof(TopicPolicy.ManageTopics))]
    public async Task<ActionResult<TopicData>> PostTopic(Topic topic)
    {
        context.Topics.Add(topic);
        await context.SaveChangesAsync();

        return CreatedAtAction("GetTopic", new { id = topic.Id }, topic.ToData());
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = nameof(TopicPolicy.ManageTopics))]
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