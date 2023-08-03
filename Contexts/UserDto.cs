using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FilesAPI.Contexts
{
    public class UserDto
    {
        public long? Id { get; set; }
        [Required]
        public string? Email { get; set; }
        [Required]
        public string? Username { get; set; }
        [Required]
        public string? Password { get; set; }
        public string Role { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}
