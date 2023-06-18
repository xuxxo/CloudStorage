using FilesAPI.Contexts;
using System.Net;

namespace FilesAPI.Services
{
    public class FileService
    {
        private readonly AppContext _appContext;
        public FileService(AppContext appContext)
        {
            _appContext = appContext;
        }
        public UserFile? GetFile(long id) => _appContext.Files.FirstOrDefault(x => x.Id == id && x.IsDeleted == false);
        public UserFile? GetDeletedFile(long id) => _appContext.Files.FirstOrDefault(x => x.Id == id && x.IsDeleted == true);
        public List<UserFile> GetAllUserFiles(long id) => _appContext.Files.Where(x => x.UserId == id && x.IsDeleted == false).ToList();
        public List<UserFile> GetAllDeletedUserFiles(long id) => _appContext.Files.Where(x => x.UserId == id && x.IsDeleted == true).ToList();
        public Response DeleteFile(long id, long userId)
        {
            var file = _appContext.Files.FirstOrDefault(x => x.Id == id);
            
            if (file == null || file.UserId != userId)
                return new Response() { IsSuccess = false, Message = "Файл не найден", StatusCode = HttpStatusCode.NotFound };
            file.IsDeleted = true;
            file.LastTimeChanged = DateTime.UtcNow;
            _appContext.SaveChanges();
            return new Response() { IsSuccess = true, Message = "Файл перемещен в корзину"};
            
        }
        public Response DeleteFileAtAll(long id, long userId)
        {
            var file = _appContext.Files.FirstOrDefault(x => x.Id == id && x.IsDeleted == true);

            if (file == null || file.UserId != userId)
                return new Response() { IsSuccess = false, Message = "Файл не найден", StatusCode = HttpStatusCode.NotFound };
            File.Delete(Path.Combine(Path.GetFullPath(file.Path), file.Name));
            _appContext.Remove(file);
            _appContext.SaveChanges();

            return new Response() { IsSuccess = true, Message = "Файл удален" };
        }
        public Response AddNewFile(IFormFile userFile, long userId)
        {
            const string filePath = @"..\FilesAPI\Files\";
            var fullName = filePath + userFile.FileName;
            if (userFile.Length == 0)
                return new Response() { IsSuccess = false, Message = "Получен пустой файл", StatusCode = HttpStatusCode.BadRequest };

            using (var stream = new FileStream(fullName, FileMode.Create))
            {
                userFile.CopyTo(stream);
            }

            using (var db = new AppContext())
            {
                var newFile = new UserFile()
                {
                    ContentType = userFile.ContentType,
                    Name = userFile.FileName,
                    Path = filePath,
                    Size = userFile.Length,
                    UserId = userId,
                    IsDeleted = false,
                    LastTimeChanged = DateTime.UtcNow,
                };
                db.Files.Add(newFile);
                db.SaveChanges();
            }
            return new Response() { IsSuccess = true, Message = "Файл успешно добавлен" };
        }
    }
}
