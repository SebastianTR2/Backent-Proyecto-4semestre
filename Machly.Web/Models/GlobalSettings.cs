using System.Collections.Generic;

namespace Machly.Web.Models
{
    public class GlobalSettings
    {
        public string? Id { get; set; }
        public List<string> AllowedCategories { get; set; } = new List<string>();
        public decimal CommissionRate { get; set; }
        public string TermsAndConditions { get; set; } = string.Empty;
        public string SupportEmail { get; set; } = string.Empty;
    }
}
