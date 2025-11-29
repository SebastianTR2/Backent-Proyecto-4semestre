using System;

namespace Machly.Api.DTOs
{
    public class ApiAdminUserSummaryDto
    {
        public string? Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsVerifiedProvider { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
