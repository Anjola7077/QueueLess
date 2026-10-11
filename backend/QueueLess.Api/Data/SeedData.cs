using Microsoft.AspNetCore.Identity;
using QueueLess.Api.Models;

namespace QueueLess.Api.Data;

public static class SeedData
{
    public static void Initialize(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var hasher = new PasswordHasher<User>();

        var customer = new User { Name = "Demo Customer", Email = "customer@queueless.local", Role = "Customer" };
        customer.PasswordHash = hasher.HashPassword(customer, "Password123!");

        var staff = new User { Name = "Demo Staff", Email = "staff@queueless.local", Role = "Staff" };
        staff.PasswordHash = hasher.HashPassword(staff, "Password123!");

        db.Users.AddRange(customer, staff);
        db.SaveChanges();

        var business = new Business
        {
            Name = "City Clinic",
            Address = "Main Street",
            Description = "Demo queue-enabled clinic",
            OwnerId = staff.Id
        };
        db.Businesses.Add(business);
        db.SaveChanges();

        var services = new[]
        {
            new Service { BusinessId = business.Id, Name = "General Consultation", Description = "General medical consultation", AverageDuration = 8 },
            new Service { BusinessId = business.Id, Name = "Laboratory", Description = "Laboratory services", AverageDuration = 5 },
            new Service { BusinessId = business.Id, Name = "Dental", Description = "Dental consultation", AverageDuration = 15 }
        };
        db.Services.AddRange(services);
        db.SaveChanges();

        foreach (var s in services)
            db.Queues.Add(new Queue { ServiceId = s.Id });

        db.SaveChanges();
    }
}
