using ExpenseTracker.Data;
using ExpenseTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;

namespace ExpenseTracker.Services
{
    public class ExpenseService
    {
        private readonly ExpenseDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ExpenseService(ExpenseDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get all expenses for the logged-in user
        public List<Expense> GetExpenses(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        // Add a new expense
        public void AddExpense(Expense expense, ClaimsPrincipal user)
        {
            if (expense.Date == default)
                expense.Date = DateTime.Now;

            expense.UserId = _userManager.GetUserId(user);

            _context.Expenses.Add(expense);
            _context.SaveChanges();
        }

        // Delete an expense
        public void DeleteExpense(int id, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);

            if (expense != null)
            {
                _context.Expenses.Remove(expense);
                _context.SaveChanges();
            }
        }

        // Update an expense
        public void UpdateExpense(Expense expense, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var existingExpense = _context.Expenses.FirstOrDefault(e => e.Id == expense.Id && e.UserId == userId);

            if (existingExpense != null)
            {
                existingExpense.Title = expense.Title;
                existingExpense.Amount = expense.Amount;
                existingExpense.Category = expense.Category;
                existingExpense.Date = expense.Date;
                _context.SaveChanges();
            }
        }

        // Filter by category
        public List<Expense> GetExpensesByCategory(string category, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.Category == category && e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        // Filter by date
        public List<Expense> GetExpensesByDate(DateTime date, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.Date.Date == date.Date && e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        // Filter by date range
        public List<Expense> GetExpensesByRange(DateTime start, DateTime end, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.Date.Date >= start.Date && e.Date.Date <= end.Date && e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToList();
        }

        // --- Summary Queries (per user) ---

        public decimal GetMonthlyTotal(int year, int month, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return (decimal)_context.Expenses
                .Where(e => e.Date.Year == year && e.Date.Month == month && e.UserId == userId)
                .Sum(e => (double)e.Amount);
        }

        public decimal GetWeeklyTotal(int year, int weekNumber, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return (decimal)_context.Expenses
                .AsEnumerable()
                .Where(e => ISOWeek.GetWeekOfYear(e.Date) == weekNumber && e.Date.Year == year && e.UserId == userId)
                .Sum(e => (double)e.Amount);
        }

        public decimal GetCategoryTotal(string category, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return (decimal)_context.Expenses
                .Where(e => e.Category == category && e.UserId == userId)
                .Sum(e => (double)e.Amount);
        }

        // Monthly breakdown (Jan–Dec totals)
        public Dictionary<string, decimal> GetMonthlyBreakdown(int year, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var result = new Dictionary<string, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var total = _context.Expenses
                    .Where(e => e.Date.Year == year && e.Date.Month == month && e.UserId == userId)
                    .Select(e => (double)e.Amount)
                    .Sum();

                var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
                result[monthName] = (decimal)total;
            }

            return result;
        }

        // Daily breakdown for a specific month
        public Dictionary<string, decimal> GetDailyBreakdown(int year, int month, ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            var result = new Dictionary<string, decimal>();
            int daysInMonth = DateTime.DaysInMonth(year, month);

            for (int day = 1; day <= daysInMonth; day++)
            {
                var total = _context.Expenses
                    .Where(e => e.Date.Year == year && e.Date.Month == month && e.Date.Day == day && e.UserId == userId)
                    .Select(e => (double)e.Amount)
                    .Sum();

                result[$"{day} {CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month)}"] = (decimal)total;
            }

            return result;
        }

        // --- Extra Methods for Reports Page ---

        public decimal GetTotalExpenses(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return (decimal)_context.Expenses
                .Where(e => e.UserId == userId)
                .Sum(e => (double)e.Amount);
        }

        public Dictionary<string, decimal> GetTopCategories(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.UserId == userId)
                .AsEnumerable() // force client-side grouping
                .GroupBy(e => e.Category)
                .OrderByDescending(g => g.Sum(e => e.Amount))
                .Take(5)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public List<Expense> GetRecentExpenses(ClaimsPrincipal user)
        {
            var userId = _userManager.GetUserId(user);
            return _context.Expenses
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .Take(5)
                .ToList();
        }

        public Dictionary<string, decimal> GetCategoryBreakdown(int year, ClaimsPrincipal user, int? month = null)
        {
            var userId = _userManager.GetUserId(user);
            var query = _context.Expenses.Where(e => e.Date.Year == year && e.UserId == userId);

            if (month.HasValue)
                query = query.Where(e => e.Date.Month == month.Value);

            return query
                .AsEnumerable() // force client-side grouping
                .GroupBy(e => e.Category)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }
    }
}
