using FilesAPI.Contexts;
using FilesAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace FilesAPI.Services
{
    public class UserService
    {
        private readonly AppContext _appContext;
        public UserService(AppContext appContext)
        {
            _appContext = appContext;
        }

        public User? GetUser(long id) => _appContext.Users.Where(u => u.Id == id).FirstOrDefault();
        public User? GetUserByUsername(string username) => _appContext.Users.Where(u => u.Username == username).FirstOrDefault();
        public List<User> GetAllUsers() => _appContext.Users.ToList();

        public Response CreateNewUser(User user)
        {
            try
            {
                if (!IsUsernameAvailable(user.Username))
                    return new Response() { IsSuccess = false, Message = "Имя уже занято" };
                _appContext.Add(user);
                _appContext.SaveChanges();
                return new Response() { IsSuccess = true, Message = "Пользователь успешно добавлен" };
            }
            catch (Exception ex)
            {
                return new Response() { IsSuccess = false, Message = ex.Message, StatusCode = HttpStatusCode.InternalServerError };
            }

        }

        public Response ChangePassword(long id, string newPassword)
        {
            var oldUser = _appContext.Users.Where(x => x.Id == id).FirstOrDefault();

            if (oldUser is null)
                return new Response() { IsSuccess = false, Message = "Пользователь не найден", StatusCode = HttpStatusCode.NotFound };

            oldUser.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);


            _appContext.SaveChanges();
            return new Response() { IsSuccess = true, Message = "Пароль успешно изменен" };
        }

        public Response DeleteUser(long id)
        {
            var user = GetUser(id);
            if (user is null)
                return new Response() { IsSuccess = false, Message = "Пользователь не найден", StatusCode = HttpStatusCode.NotFound };
            _appContext.Users.Attach(user);
            _appContext.Users.Remove(user);
            _appContext.SaveChanges();
            return new Response() { IsSuccess = true, Message = "Пользователь успешно удалён" };
        }

        private bool IsUsernameAvailable(string username)
        {
            using (AppContext db = new AppContext())
            {
                var user = db.Users.Where(u => u.Username == username).FirstOrDefault();
                return user == null;
            }
        }
    }
}
