using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Prices
{
    public class IndexModel : PageModel
    {
        private const int PageSize = 50;
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<TProduct> Products { get; set; } = new List<TProduct>();
        public List<SelectListItem> Categories { get; set; } = new();
        public int LanguageId { get; set; } = 1;
        public int TotalCount { get; set; }
        public int TotalPages { get; set; } = 1;

        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public long? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)] public bool SinPrecio { get; set; }
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync()
        {
            var es = await _context.TLanguages.FirstOrDefaultAsync(l => l.Code.ToLower() == "es");
            LanguageId = es?.Id ?? 1;

            Categories = (await _context.TCategories.Include(c => c.CategoriesTranslations).ToListAsync())
                .Select(c => new SelectListItem(
                    c.CategoriesTranslations?.FirstOrDefault(t => t.LanguageId == LanguageId)?.Name
                        ?? c.CategoriesTranslations?.FirstOrDefault()?.Name ?? $"#{c.Id}",
                    c.Id.ToString()))
                .OrderBy(x => x.Text).ToList();

            var query = _context.TProducts.AsQueryable();

            if (!string.IsNullOrWhiteSpace(Q))
            {
                var t = Q.Trim();
                query = query.Where(p => p.Code.Contains(t) || p.ProductTranslations!.Any(x => x.Name.Contains(t)));
            }
            if (CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == CategoryId);
            if (SinPrecio)
                query = query.Where(p => !p.Prices!.Any());

            TotalCount = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            Products = await query
                .Include(p => p.ProductTranslations)
                .Include(p => p.PriceUnit)
                .Include(p => p.Prices)
                .OrderBy(p => p.Code)
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}