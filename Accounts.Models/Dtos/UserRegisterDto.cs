using System.ComponentModel.DataAnnotations;

namespace Accounts.Models.Dtos
{
    public class UserRegisterDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        [Required]
        public string Email { get; set; }
    }
}

