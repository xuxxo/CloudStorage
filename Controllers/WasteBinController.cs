using FilesAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilesAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class WasteBinController : Controller
    {
        private readonly FileService _fileService;
        private long UserId => long.Parse(User.Claims.First(i => i.Type == ClaimTypes.NameIdentifier).Value);        

        public WasteBinController(FileService fileService)
        {
            _fileService = fileService;
        }

        [HttpGet("{id}")]
        public IActionResult Get(long? id)
        {
            if (id == null)
                return BadRequest("Передано значение null");

            var result = _fileService.GetDeletedFile(id.Value);

            if (result == null || result.UserId != UserId)
                return NotFound($"Файл по id = {id} не найден");

            return PhysicalFile(Path.Combine(Path.GetFullPath(result.Path), result.Name), result.ContentType, result.Name);
        }

        [HttpGet]
        public IActionResult GetAllUsersFiles()
        {
            var result = _fileService.GetAllDeletedUserFiles(UserId);

            return result.Count == 0 ? NotFound() : Json(result);
        }

        [HttpDelete]
        public IActionResult Delete(long fileId)
        {
            var response = _fileService.DeleteFileAtAll(fileId, UserId);

            return response.IsSuccess ? Ok(response.Message) : BadRequest(response.Message);
        }
    }
}
