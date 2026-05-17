using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Linq;

namespace ExpenseTracker.Services
{
    public class BudgetService
    {
        private readonly ExpenseDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public BudgetService(ExpenseDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get all budgets for the logged-in user
        public List<Budget> GetBudgets(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Budgets
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.DateCreated)
                .ToList();
        }

        // Add a new budget
        public void AddBudget(Budget budget, ClaimsPrincipal user)
        {
            budget.UserId = _userManager.GetUserId(user);
            _context.Budgets.Add(budget);
            _context.SaveChanges();
        }

        // Update an existing budget
        public void UpdateBudget(Budget budget, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            if (budget.UserId == userId)
            {
                _context.Budgets.Update(budget);
                _context.SaveChanges();
            }
        }

        // Delete a budget
        public void DeleteBudget(int id, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var budget = _context.Budgets.FirstOrDefault(b => b.Id == id && b.UserId == userId);
            if (budget != null)
            {
                _context.Budgets.Remove(budget);
                _context.SaveChanges();
            }
        }

        // Calculate spent amount for a category (per user)
        public decimal GetSpentForCategory(string category, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return (decimal)_context.Expenses
                .Where(e => e.UserId == userId && e.Category == category)
                .Sum(e => (double)e.Amount);
        }

        // Helper: calculate remaining budget
        public decimal GetRemainingBudget(Budget budget, ClaimsPrincipal user)
        {
            var spent = GetSpentForCategory(budget.Category, user);
            return budget.Limit - spent;
        }
    }
}
