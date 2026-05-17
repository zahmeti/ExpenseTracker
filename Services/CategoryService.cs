using ExpenseTracker.Data;
using ExpenseTracker.Models;

namespace ExpenseTracker.Services
{
    public class CategoryService
    {
        private readonly ExpenseDbContext _context;

        public CategoryService(ExpenseDbContext context)
        {
            _context = context;
        }

        public List<Category> GetCategories() => _context.Categories.ToList();

        public void AddCategory(Category category)
        {
            _context.Categories.Add(category);
            _context.SaveChanges();
        }
    }
}
