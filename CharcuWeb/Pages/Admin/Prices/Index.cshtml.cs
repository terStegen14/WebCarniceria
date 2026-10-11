using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Prices
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<TProduct> Products { get; set; } = new List<TProduct>();
        public int LanguageId { get; set; } = 1;

        public async Task OnGetAsync()
        {
            Products = await _context.TProducts
                .Include(p => p.ProductTranslations)
                .Include(p => p.PriceUnit)
                .Include(p => p.Prices)
                .OrderBy(p => p.Code)
                .ToListAsync();
        }
    }
}