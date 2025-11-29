using System;

namespace Machly.Web.Models
{
    public class User
    {
        public string? Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "";
        public bool IsVerifiedProvider { get; set; }
        public string? PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

