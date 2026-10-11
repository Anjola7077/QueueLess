using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using QueueLess.Api.Data;
using QueueLess.Api.Models;

namespace QueueLess.Api.Services;

public class AuthService(AppDbContext db, IConfiguration config)
{
    private readonly PasswordHasher<User> hasher = new();

    public User? Validate(string email, string password)
    {
        var user = db.Users.FirstOrDefault(x => x.Email.ToLower() == email.ToLower());
        if (user == null) return null;
        var result = hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public User Create(string name, string email, string password)
    {
        var user = new User { Name = name, Email = email, Role = "Customer" };
        user.PasswordHash = hasher.HashPassword(user, password);
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    public string Token(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            config["Jwt:Issuer"], config["Jwt:Audience"], claims,
            expires: DateTime.UtcNow.AddDays(7), signingCredentials: cred);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
