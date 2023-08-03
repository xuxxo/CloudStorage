using FilesAPI.Contexts;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace FilesAPI.Services
{
    public class UserService
    {
        private readonly AppContext _appContext;
        public UserService(AppContext appContext)
        {
            _appContext = appContext;
        }

        public UserDto? GetUser(long id) => _appContext.Users.Where(u => u.Id == id).FirstOrDefault();
        public UserDto? GetUserByUsername(string username) => _appContext.Users.Where(u => u.Username == username).FirstOrDefault();
        public List<UserDto> GetAllUsers() => _appContext.Users.ToList();

        public Response CreateNewUser(Contexts.UserDto user)
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

        public string CreateToken(UserDto user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username ?? String.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("JQHWDy1u2h3b87!*&$&!$hdff786"));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds
                );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }

        private static bool IsUsernameAvailable(string username)
        {
            using (AppContext db = new AppContext())
            {
                var user = db.Users.Where(u => u.Username == username).FirstOrDefault();
                return user == null;
            }
        }
    }
}
