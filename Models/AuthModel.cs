using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FilesAPI.Models
{
    public class AuthModel
    {
        public string? Email { get; set; }

        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
