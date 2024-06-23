using Accounts.Models.Dtos;
using Accounts.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Accounts.Service.Security;
using EasyNetQ;
using Microsoft.AspNetCore.Authorization;
using Shared.Messages;


namespace Accounts.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtToken;
        private readonly IBus _bus;
        private readonly IHttpClientFactory _httpClientFactory;

        public AccountsController(IUserService userService, IJwtTokenService jwtToken, IBus bus, IHttpClientFactory httpClientFactory)
        {
            _userService = userService;
            _jwtToken = jwtToken;
            _bus = bus;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost("user")]
        public async Task<IActionResult> Register(UserRegisterDto userRegisterDto)
        {
            var user = await _userService.RegisterUserAsync(userRegisterDto);
            var message = new WalletCreateMessage
            {
                UserId = user.Id
            };
            Console.WriteLine($"Published message: {message}");
            await _bus.PubSub.PublishAsync(message);
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

        [Authorize]
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = _jwtToken.ExtractIdFromJwtToken(token);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"http://localhost:5249/v1/payments/transactions/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [Authorize]
        [HttpPost("topup")]
        public async Task<IActionResult> Topup()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = _jwtToken.ExtractIdFromJwtToken(token);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"http://localhost:5249/v1/payments/transactions/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
