using FilesAPI.Contexts;
using FilesAPI.Models;
using FilesAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace FilesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(UserService userService, ILogger<AuthController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpPost("Register")]
        public IActionResult Register(AuthModel request)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            UserDto user = new();
            user.Username = request.Username;
            user.Password = passwordHash;
            user.Email = request.Email;
            user.Role = "User";

            var response = _userService.CreateNewUser(user);

            if (response.IsSuccess)
                return Ok(user);

            return BadRequest(response.Message);
        }

        [HttpPost("Login")]
        public IActionResult Login(AuthModel request)
        {
            var user = _userService.GetUserByUsername(request.Username);

            if (user == null)
            {
                return BadRequest("Неправильный логин или пароль");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                _logger.LogWarning("Введен неправильный пароль для пользователя {user}", user);
                return BadRequest("Неправильный логин или пароль");
            }

            var token = _userService.CreateToken(user);

            return Ok(token);
        }

        
    }
}