using Accounts.Models.Dtos;
using Accounts.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Accounts.Service.Security;
using EasyNetQ;
using Investment.Models.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.VisualBasic.CompilerServices;
using Newtonsoft.Json;
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
            var walletCreateMessage = new WalletCreateMessage
            {
                UserId = user.Id
            };
            var portfolioCreateMessage = new CreatePortfolioMessage
            {
                userId = user.Id
            };
            await _bus.PubSub.PublishAsync(walletCreateMessage);
            Console.WriteLine($"Published message: {walletCreateMessage}");
            await _bus.PubSub.PublishAsync(portfolioCreateMessage);
            Console.WriteLine($"Published message: {portfolioCreateMessage}");
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
            var response = await client.GetAsync($"http://payment:8080/v1/payments/transactions/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [Authorize]
        [HttpPost("topup")]
        public async Task<IActionResult> Topup([FromBody] TopUpDto topUpDto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = _jwtToken.ExtractIdFromJwtToken(token);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var requestContent =
                new StringContent(JsonConvert.SerializeObject(new { UserId = userId, topUpDto.Amount }), Encoding.UTF8,
                    "application/json");
            var response = await client.PostAsync($"http://payment:8080/v1/payments/topup", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        
        [Authorize]
        [HttpGet("balance")]
        public async Task<IActionResult> Balance()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var userId = _jwtToken.ExtractIdFromJwtToken(token);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync($"http://payment:8080/v1/payments/wallet/{userId}");
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }


        [Authorize]
        [HttpPost("project")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectDto projectDto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var requestContent =
                new StringContent(JsonConvert.SerializeObject(projectDto), Encoding.UTF8,
                    "application/json");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync("http://investment:8080/v1/project",requestContent);
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
        
        [Authorize]
        [HttpGet("projects")]
        public async Task<IActionResult> GetProjects()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.GetAsync("http://investment:8080/v1/projects");
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                return Ok(transactions);
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        [Authorize]
        [HttpPost("invest")]
        public async Task<IActionResult> Invest([FromBody] InvestTransactDto investTransactDto)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Substring("Bearer ".Length).Trim();
            var requestContent =
                new StringContent(JsonConvert.SerializeObject(investTransactDto), Encoding.UTF8,
                    "application/json");
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await client.PostAsync("http://investment:8080/v1/invest", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var transactions = await response.Content.ReadAsStringAsync();
                var addInvestmentMessage = new AddInvestmentMessage
                {
                    userId = _jwtToken.ExtractIdFromJwtToken(token),
                    amount = investTransactDto.Amount,
                    projectId = investTransactDto.ProjectId
                };
                await _bus.PubSub.PublishAsync(addInvestmentMessage);
                Console.WriteLine($"Published message: {addInvestmentMessage}");
                return Ok(transactions);
            }
            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }
}
