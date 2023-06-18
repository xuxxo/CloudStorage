using FilesAPI.Contexts;
using FilesAPI.Models;
using FilesAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace FilesAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UsersController : Controller
    {
        private readonly UserService _usersService;
        private readonly ILogger<UsersController> _logger;
        private long UserId 
        {
            get
            {
                return Int64.Parse(User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value); ;    // возвращаем значение свойства
            }
        }

        public UsersController(UserService usersService, ILogger<UsersController> logger)
        {
            _usersService = usersService;
            _logger = logger;
        }

        [HttpGet("{id}", Name ="GetUserById"), Authorize(Roles = "Admin")]
        public IActionResult GetUserById(long? id)
        {
            if (id == null)
                return BadRequest();

            var user = _usersService.GetUser(id.Value);
            if (User.Claims.First(i => i.Type == "Role").Value == "User" && (user?.Username ?? string.Empty) != User.Claims.First(i => i.Type == "Name").Value)
                return Forbid();
            if (user is null)
                return NotFound();

            return Json(user);
            
        }

        [HttpGet(Name = "GetMyUser"), Authorize(Roles = "Admin,User")]
        public IActionResult GetMyUser()
        {
            var user = _usersService.GetUser(UserId);

            if (user is null)
                return NotFound();

            _logger.LogInformation("Пользователь {user} запросил информацию о себе", user);

            return Json(user);

        }

        [HttpGet(Name = "GetAllUsers"), Authorize(Roles = "Admin")]
        public IActionResult GetAllUsers()
        {
            return Json(_usersService.GetAllUsers());
        }

        [HttpPost(Name = "CreateNewUser"), Authorize(Roles = "Admin")]
        public IActionResult CreateNewUser(User user)
        {
            if (user == null)
                return BadRequest();
            
            var response = _usersService.CreateNewUser(user);

            if (response.IsSuccess)
                return Ok(response.Message);

            return StatusCode((int)HttpStatusCode.InternalServerError, response.Message);
        }

        [HttpPut(Name = "ChangePassword"), Authorize(Roles = "Admin,User")]
        public IActionResult ChangePassword(string oldPassword, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
                return BadRequest("Новый пароль не может быть пустым");

            var user = _usersService.GetUser(UserId);

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.Password))
            {
                _logger.LogWarning("Введен неправильный пароль для пользователя {user}", user);
                return BadRequest("Старый пароль введен неверно");
            }

            var response = _usersService.ChangePassword(UserId, newPassword);

            if (response.IsSuccess)
                return Ok(response.Message);

            return NotFound(response.Message);
        }

        [HttpDelete(Name = "DeleteUser"), Authorize(Roles = "Admin")]
        public IActionResult Delete(long? id)
        {
            if (id is null)
                return BadRequest();

            var response = _usersService.DeleteUser(id.Value);

            if (response.IsSuccess)
                return Ok(response.Message);

            return NotFound(response.Message);
        }
    }
}
