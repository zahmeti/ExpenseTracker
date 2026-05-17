using Microsoft.AspNetCore.Identity;

namespace ExpenseTracker.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Now;
         public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
