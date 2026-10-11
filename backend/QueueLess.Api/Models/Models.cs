namespace QueueLess.Api.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string Role { get; set; } = "Customer";
}

public class Business
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Address { get; set; } = "";
    public string Description { get; set; } = "";
    public int OwnerId { get; set; }
}

public class Service
{
    public int Id { get; set; }
    public int BusinessId { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int AverageDuration { get; set; } = 10;
    public bool IsActive { get; set; } = true;
}

public class Queue
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string Status { get; set; } = "Open";
    public int CurrentNumber { get; set; } = 0;
    public int NextTicketNumber { get; set; } = 1;
}

public class QueueTicket
{
    public int Id { get; set; }
    public int QueueId { get; set; }
    public int UserId { get; set; }
    public int TicketNumber { get; set; }
    public string Status { get; set; } = "Waiting";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CalledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}
