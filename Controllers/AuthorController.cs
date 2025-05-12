using System.Security.Claims;
using BlogApi.Data;
using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApi.Controllers
{
    [Route("api/authors")]
    [ApiController]
    public class AuthorController : ControllerBase
    {
        private readonly BlogDbContext _context;
        private readonly AuthService _authService;

        public AuthorController(BlogDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAuthorAsync([FromBody] RegisterAuthorDto registerDto)
        {
            if (registerDto == null)
            {
                return BadRequest("Invalid data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _context.Authors.AnyAsync(a => a.Email == registerDto.Email))
            {
                return BadRequest("Email already exists");
            }

            _authService.CreatePasswordHash(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var author = new Author
            {
                AuthorName = registerDto.AuthorName,
                Email = registerDto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = registerDto.Role
            };

            await _context.Authors.AddAsync(author);
            await _context.SaveChangesAsync();

            var token = _authService.CreateToken(author);
            var response = new AuthResponse
            {
                Id = author.Id,
                AuthorName = author.AuthorName,
                Email = author.Email,
                Role = author.Role,
                Token = token
            };

            return Ok(response);
        }
    }
}
