using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QueueLess.Api.Hubs;
using QueueLess.Api.Services;

namespace QueueLess.Api.Controllers;

[ApiController]
[Authorize]
[Route("api")]
public class QueueController(QueueService service, IHubContext<QueueHub> hub) : ControllerBase
{
    int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("businesses")]
    public IActionResult Businesses() => Ok(service.Businesses());

    [HttpGet("queues/{id}")]
    public IActionResult Status(int id) => Ok(service.QueueStatus(id, UserId));

    [HttpPost("queues/{id}/join")]
    public async Task<IActionResult> Join(int id)
    {
        try
        {
            var ticket = service.Join(id, UserId);
            await hub.Clients.Group($"queue-{id}").SendAsync("QueueUpdated", service.QueueStatus(id));
            return Ok(new { ticket.Id, ticket.TicketNumber });
        }
        catch (Exception e) { return BadRequest(new { message = e.Message }); }
    }

    [HttpDelete("queues/{id}/leave")]
    public async Task<IActionResult> Leave(int id)
    {
        service.Leave(id, UserId);
        await hub.Clients.Group($"queue-{id}").SendAsync("QueueUpdated", service.QueueStatus(id));
        return NoContent();
    }

    [Authorize(Roles = "Staff")]
    [HttpPost("queues/{id}/next")]
    public async Task<IActionResult> Next(int id)
    {
        var ticket = service.CallNext(id);
        await hub.Clients.Group($"queue-{id}").SendAsync("QueueUpdated", service.QueueStatus(id));
        return Ok(ticket == null ? null : new { ticket.Id, ticket.TicketNumber });
    }

    [Authorize(Roles = "Staff")]
    [HttpPost("tickets/{id}/complete")]
    public async Task<IActionResult> Complete(int id)
    {
        service.Complete(id);
        return Ok();
    }

    [Authorize(Roles = "Staff")]
    [HttpPost("tickets/{id}/skip")]
    public async Task<IActionResult> Skip(int id)
    {
        service.Skip(id);
        return Ok();
    }
}
