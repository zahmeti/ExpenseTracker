using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models
{
    public class Budget
    {
        public int Id { get; set; }
        public string Category { get; set; } = "";
        public decimal Limit { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.Now;
        public string? UserId { get; set; } = string.Empty;
        public IdentityUser? User { get; set; }
        
    }
}
