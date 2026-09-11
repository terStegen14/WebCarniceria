using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Categories
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<TCategories> Categories { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.TCategories
                .Include(x => x.CategoriesTranslations)
                .ToListAsync();
        }
    }
}