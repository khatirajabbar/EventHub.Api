using EventHub.Api.Entities;
using EventHub.Api.Enums;
using System.Security.Cryptography;
using System.Text;

namespace EventHub.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.AddRange(
                new User { Username = "admin", Email = "admin@eventhub.com", PasswordHash = HashPassword("Admin@123"), Role = "Admin", CreatedAt = DateTime.UtcNow },
                new User { Username = "member", Email = "member@eventhub.com", PasswordHash = HashPassword("Member@123"), Role = "Member", CreatedAt = DateTime.UtcNow }
            );
            context.SaveChanges();
        }

        if (!context.Organizers.Any())
        {
            context.Organizers.AddRange(
                new Organizer { Name = "Tech Events Co.", Email = "info@techevents.com", Phone = "+994501234567" },
                new Organizer { Name = "Creative Minds Agency", Email = "hello@creativeminds.com", Phone = "+994557654321" },
                new Organizer { Name = "Startup Hub Baku", Email = "contact@startuphub.az", Phone = "+994709876543" }
            );
            context.SaveChanges();
        }

        if (!context.Events.Any())
        {
            context.Events.AddRange(
                new Event { Title = "Tech Conference 2026", Description = "Annual technology conference covering AI, cloud and software development.", Date = new DateTime(2026, 7, 15), Location = "Baku Convention Center, Baku", OrganizerId = 1 },
                new Event { Title = "AI & Machine Learning Summit", Description = "Deep dive into the latest trends in artificial intelligence and ML.", Date = new DateTime(2026, 9, 20), Location = "ADA University, Baku", OrganizerId = 1 },
                new Event { Title = "Design & UX Workshop", Description = "Hands-on workshop for UI/UX designers and product teams.", Date = new DateTime(2026, 6, 10), Location = "Creative Hub, Nizami Street, Baku", OrganizerId = 2 },
                new Event { Title = "Photography Exhibition 2026", Description = "Annual exhibition showcasing the best local and international photography.", Date = new DateTime(2026, 8, 5), Location = "Baku Museum of Modern Art", OrganizerId = 2 },
                new Event { Title = "Startup Pitch Night", Description = "Early stage startups pitch to investors and industry mentors.", Date = new DateTime(2026, 6, 25), Location = "Startup Hub Baku, Əliağa Vahid str.", OrganizerId = 3 },
                new Event { Title = "Entrepreneurship Bootcamp", Description = "3-day intensive bootcamp for aspiring entrepreneurs and founders.", Date = new DateTime(2026, 10, 3), Location = "SABAH Center, Baku", OrganizerId = 3 }
            );
            context.SaveChanges();
        }

        if (!context.Tickets.Any())
        {
            context.Tickets.AddRange(
                new Ticket { EventId = 1, Type = TicketType.VIP, Price = 150, QuantityAvailable = 50 },
                new Ticket { EventId = 1, Type = TicketType.Regular, Price = 50, QuantityAvailable = 200 },
                new Ticket { EventId = 2, Type = TicketType.VIP, Price = 200, QuantityAvailable = 30 },
                new Ticket { EventId = 2, Type = TicketType.Regular, Price = 75, QuantityAvailable = 150 },
                new Ticket { EventId = 3, Type = TicketType.Regular, Price = 40, QuantityAvailable = 100 },
                new Ticket { EventId = 3, Type = TicketType.Standard, Price = 20, QuantityAvailable = 50 },
                new Ticket { EventId = 4, Type = TicketType.Basic, Price = 25, QuantityAvailable = 300 },
                new Ticket { EventId = 5, Type = TicketType.VIP, Price = 100, QuantityAvailable = 20 },
                new Ticket { EventId = 5, Type = TicketType.Regular, Price = 35, QuantityAvailable = 120 },
                new Ticket { EventId = 6, Type = TicketType.Premium, Price = 60, QuantityAvailable = 40 },
                new Ticket { EventId = 6, Type = TicketType.Regular, Price = 90, QuantityAvailable = 100 },
                new Ticket { EventId = 6, Type = TicketType.Standard, Price = 30, QuantityAvailable = 60 }
            );
            context.SaveChanges();
        }
    }

    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}

