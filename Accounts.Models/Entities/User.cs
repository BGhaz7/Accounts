using System.ComponentModel.DataAnnotations;

namespace Accounts.Models.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        public string Username { get; set; }
        public string Fname { get; set; }
        public string Lname { get; set; }
        public string Email { get; set; }
        public string SHA256Password { get; set; }
        public bool LoggedIn { get; set; }
    }
}

