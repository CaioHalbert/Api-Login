using Api_Login.Data;
using Api_Login.DTOs;
using Api_Login.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static Api_Login.DTOs.LoginDTO;

namespace Api_Login.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class UserController : Controller
    {

        private User teste = new User();
        
            private readonly DataContext _context;
            private readonly IConfiguration _configuration;

            public UserController(DataContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }
            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto model)
            {
                try
                {
                    // Procure o usuário pelo nome de usuário
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                    if (user == null)
                    {
                        return BadRequest("Nome de usuário ou senha incorretos.");
                    }

                    // Verifique a senha
                    if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
                    {
                        return BadRequest("Nome de usuário ou senha incorretos.");
                    }

                    // Autenticação bem-sucedida, gere um token JWT
                    var token = GenerateToken(user);

                    return Ok(new { Token = token });
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erro interno: {ex.Message}");
                }
            }
            [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterDTO model)
            {
                try
                {
                    // Verifique se o usuário já existe
                    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);

                    if (existingUser != null)
                    {
                        return BadRequest("Nome de usuário já em uso.");
                    }

                    // Hash da senha
                    var passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

                    // Crie um novo usuário
                    var newUser = new User
                    {
                        Username = model.Username,
                        Email = model.Email,
                        PasswordHash = passwordHash,
                        AccessLevel = model.AccessLevel
                    };

                    _context.Users.Add(newUser);
                    await _context.SaveChangesAsync();

                    return Ok("Registro bem-sucedido.");
                }
                catch (Exception ex)
                {
                    return StatusCode(500, $"Erro interno: {ex.Message}");
                }
            }

            private string GenerateToken(User user)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                byte[] key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                    new Claim(type:ClaimTypes.NameIdentifier, value: user.Id.ToString()),
                    new Claim(type:ClaimTypes.Name, value: user.Username),
                    new Claim(type:ClaimTypes.Email, value: user.Email),
                    new Claim(type:ClaimTypes.Role, value: user.AccessLevel),
                }),
                    Expires = DateTime.UtcNow.AddHours(2),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                        algorithm: SecurityAlgorithms.HmacSha256),
                };
                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }


        }
}
