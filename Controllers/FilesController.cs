using FilesAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilesAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : Controller
    {
        private readonly FileService _fileService;
        private readonly ILogger<FilesController> _logger;

        private long UserId => long.Parse(User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value);    

        public FilesController(FileService fileService, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        [HttpGet("{id}")]
        public IActionResult Get(long? id)
        {
            if (id == null)
                return BadRequest("Передано значение null");

            var result = _fileService.GetFile(id.Value);

            if (result == null || result.UserId != UserId)
            {
                _logger.LogWarning("Файл по id = {id} не найден", id);
                return NotFound($"Файл по id = {id} не найден");
            }
                
            return PhysicalFile(Path.Combine(Path.GetFullPath(result.Path), result.Name), result.ContentType, result.Name);
        }

        [HttpGet]
        public IActionResult GetAllUsersFiles()
        {
            var result = _fileService.GetAllUserFiles(UserId);

            return result.Count == 0 ? NotFound("Файлов пользователя не найдено") : Json(result);
        }

        [HttpPost]
        public IActionResult Post(IFormFile userFile)
        {
            var response = _fileService.AddNewFile(userFile, UserId);

            if (response.IsSuccess)
                return Ok(response.Message);

            return BadRequest(response.Message);
        }

        [HttpDelete]
        public IActionResult Delete(long fileId)
        {
            var response = _fileService.DeleteFile(fileId, UserId);

            if (response.IsSuccess)
                return Ok(response.Message);

            return BadRequest(response.Message);
        }
    }
}
