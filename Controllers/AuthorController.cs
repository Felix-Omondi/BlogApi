using System.Security.Claims;
using BlogApi.Data;
using BlogApi.Dtos;
using BlogApi.Models;
using BlogApi.Services;
using Microsoft.AspNetCore.Authorization;
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

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginAuthorDto loginDto)
        {
            if (loginDto == null)
            {
                return BadRequest("Invalid data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var author = await _context.Authors.SingleOrDefaultAsync(a => a.Email.ToLower() == loginDto.Email.ToLower());

            if (author == null)
            {
                return Unauthorized("Invalid email or password");
            }

            if (!_authService.VerifyPasswordHash(loginDto.Password, author.PasswordHash, author.PasswordSalt))
            {
                return Unauthorized("Invalid email or password");
            }

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

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<Author>>> GetAllAuthorsAsync()
        {
            var authors = await _context.Authors.Include(a => a.Blogs).Select(a => new
            {
                a.Id,
                a.AuthorName,
                a.Email,
                a.Role,
                a.CreatedAt,
                a.UpdatedAt
            }).ToListAsync();

            return Ok(authors);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "AuthenticatedOnly")]
        public async Task<ActionResult<Author>> GetAuthorAsync(int id)
        {
            var author = await _context.Authors.Include(a => a.Blogs).Select(a => new
            {
                a.Id,
                a.AuthorName,
                a.Email,
                a.Role,
                a.CreatedAt,
                a.UpdatedAt
            }).FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                return NotFound();
            }

            // Only allow access if user is an admin or the author themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            return Ok(author);
        }

        [HttpPatch("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAuthorAsync(int id, [FromBody] UpdateAuthorDto updateDto)
        {
            if (updateDto == null)
            {
                return BadRequest("Invalid data");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            // Only allow update if user is an admin or the author themselves
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            if (id != currentUserId && !isAdmin)
            {
                return Forbid();
            }

            // Check if updating to an email that already exists (and isn't the current author's)
            if (_context.Authors.Any(a => a.Email.ToLower() == updateDto.Email.ToLower() && a.Id != id))
            {
                return BadRequest("Email already exists");
            }

            author.AuthorName = updateDto.AuthorName;
            author.Email = updateDto.Email;

            // Only admins can change roles
            if (isAdmin)
            {
                author.Role = updateDto.Role;
            }

            // Update password if provided
            _authService.CreatePasswordHash(updateDto.Password, out byte[] passwordHash, out byte[] passwordSalt);
            author.PasswordHash = passwordHash;
            author.PasswordSalt = passwordSalt;

            author.UpdatedAt = DateTime.UtcNow;
            _context.Authors.Update(author);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuthorAsync(int id)
        {
            var author = await _context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            _context.Authors.Remove(author);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
