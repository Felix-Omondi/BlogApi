using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BlogApi.Data;
using BlogApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlogApi.Services;

public class AuthService
{
    private readonly IConfiguration _configuration;
    private readonly BlogDbContext _context;

    public AuthService(IConfiguration configuration, BlogDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512(passwordSalt);
        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        return computedHash.SequenceEqual(passwordHash);
    }

    public string CreateToken(Author author)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, author.Id.ToString()),
            new Claim(ClaimTypes.Name, author.AuthorName),
            new Claim(ClaimTypes.Email, author.Email),
            new Claim(ClaimTypes.Role, author.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:Token").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
