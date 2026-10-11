using Microsoft.EntityFrameworkCore;
using QueueLess.Api.Models;

namespace QueueLess.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Queue> Queues => Set<Queue>();
    public DbSet<QueueTicket> QueueTickets => Set<QueueTicket>();
}
