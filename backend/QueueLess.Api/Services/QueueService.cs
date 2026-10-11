using Microsoft.EntityFrameworkCore;
using QueueLess.Api.Data;
using QueueLess.Api.Models;

namespace QueueLess.Api.Services;

public class QueueService(AppDbContext db)
{
    public object[] Businesses() =>
        db.Businesses.Select(b => new
        {
            b.Id, b.Name, b.Address, b.Description,
            Services = db.Services.Where(s => s.BusinessId == b.Id).Select(s => new
            {
                s.Id, s.Name, s.Description, s.AverageDuration,
                QueueId = db.Queues.Where(q => q.ServiceId == s.Id).Select(q => q.Id).First()
            }).ToArray()
        }).ToArray();

    public object? QueueStatus(int queueId, int? userId = null)
    {
        var q = db.Queues.Find(queueId);
        if (q == null) return null;

        var waiting = db.QueueTickets
            .Where(t => t.QueueId == queueId && t.Status == "Waiting")
            .OrderBy(t => t.TicketNumber).ToList();

        var my = userId == null ? null :
            waiting.FirstOrDefault(t => t.UserId == userId);

        var service = db.Services.Find(q.ServiceId);

        return new
        {
            queueId,
            status = q.Status,
            currentNumber = q.CurrentNumber,
            waiting = waiting.Count,
            myTicket = my?.TicketNumber,
            peopleAhead = my == null ? 0 : waiting.Count(t => t.TicketNumber < my.TicketNumber),
            estimatedMinutes = my == null || service == null ? 0 :
                waiting.Count(t => t.TicketNumber < my.TicketNumber) * service.AverageDuration
        };
    }

    public QueueTicket Join(int queueId, int userId)
    {
        var q = db.Queues.Find(queueId) ?? throw new InvalidOperationException("Queue not found.");
        if (q.Status != "Open") throw new InvalidOperationException("Queue is not open.");

        var existing = db.QueueTickets.FirstOrDefault(t =>
            t.QueueId == queueId && t.UserId == userId &&
            (t.Status == "Waiting" || t.Status == "Called" || t.Status == "Serving"));

        if (existing != null) return existing;

        var ticket = new QueueTicket
        {
            QueueId = queueId,
            UserId = userId,
            TicketNumber = q.NextTicketNumber++
        };
        db.QueueTickets.Add(ticket);
        db.SaveChanges();
        return ticket;
    }

    public void Leave(int queueId, int userId)
    {
        var ticket = db.QueueTickets.FirstOrDefault(t =>
            t.QueueId == queueId && t.UserId == userId && t.Status == "Waiting");
        if (ticket == null) return;
        ticket.Status = "Cancelled";
        db.SaveChanges();
    }

    public QueueTicket? CallNext(int queueId)
    {
        var q = db.Queues.Find(queueId);
        if (q == null) return null;

        var current = db.QueueTickets.FirstOrDefault(t =>
            t.QueueId == queueId && (t.Status == "Called" || t.Status == "Serving"));
        if (current != null) return current;

        var next = db.QueueTickets
            .Where(t => t.QueueId == queueId && t.Status == "Waiting")
            .OrderBy(t => t.TicketNumber).FirstOrDefault();

        if (next == null) return null;

        q.CurrentNumber = next.TicketNumber;
        next.Status = "Called";
        next.CalledAt = DateTime.UtcNow;
        db.SaveChanges();
        return next;
    }

    public void Complete(int ticketId)
    {
        var t = db.QueueTickets.Find(ticketId) ?? throw new InvalidOperationException();
        t.Status = "Completed";
        t.CompletedAt = DateTime.UtcNow;
        db.SaveChanges();
    }

    public void Skip(int ticketId)
    {
        var t = db.QueueTickets.Find(ticketId) ?? throw new InvalidOperationException();
        t.Status = "Skipped";
        db.SaveChanges();
    }
}
