using Accounts.Models.Dtos;
using Accounts.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Accounts.Service.Security;
using EasyNetQ;
using Microsoft.AspNetCore.Authorization;


namespace Accounts.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtToken;
        private readonly IBus _bus;

        public AccountsController(IUserService userService, IJwtTokenService jwtToken, IBus bus)
        {
            _userService = userService;
            _jwtToken = jwtToken;
            _bus = bus;
        }

        [HttpPost("user")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var user = await _userService.RegisterUserAsync(userRegisterDto);
            await _bus.PubSub.PublishAsync(new
            {
                UserId = user.Id,
                FirstName = user.Fname,
                Email = user.Email
            });
            return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginDto userLoginDto)
        {
            var user = await _userService.LoginUserAsync(userLoginDto);
            if (user == null) return BadRequest("Invalid username or password.");
            var token = _jwtToken.GenerateJwtToken(user.Username, new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Name, user.Fname),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
            });
            return Ok(new { token });
        }

        [Authorize]
        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }
        
        [Authorize]
        [HttpPut("user/{id}")]
        public async Task<IActionResult> UpdateUserById(int id, UserRegisterDto modified)
        {
            var user = await _userService.UpdateUserAsync(id, modified);
            if (user == null) return BadRequest("No such user found");
            return NoContent();
        }

        private static string GenerateJwtToken(string username, List<Claim> claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sGQ7+cHIYRyCJoq1l0F9utfBhCG4jxDVq9DKhrWyXys="));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
