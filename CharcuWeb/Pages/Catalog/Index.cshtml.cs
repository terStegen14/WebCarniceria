using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.ViewModels;

namespace TradicioCarnica.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public CatalogViewModel Catalog { get; set; }

        public async Task OnGetAsync(long? categoryId)
        {
            var cultureCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; // "es" o "ca"

            var language = await _context.TLanguages
                .FirstOrDefaultAsync(l => l.Code.ToUpper() == cultureCode.ToUpper());

            // Fallback: si no encuentra el idioma actual, coge el primero disponible
            int lang = language?.Id ?? (await _context.TLanguages.Select(l => l.Id).FirstOrDefaultAsync());

            var categories = await _context.TCategories
                .Include(c => c.CategoriesTranslations
                    .Where(t => t.LanguageId == lang))
                .ToListAsync();

            var productsQuery = _context.TProducts
                .Where(p => p.Active)
                .Include(p => p.ProductTranslations
                    .Where(t => t.LanguageId == lang))
                .Include(p => p.PriceUnit)
                .Include(p => p.Prices)
                .AsQueryable();

            if (categoryId.HasValue)
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);

            var products = await productsQuery.ToListAsync();

            Catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryId = categoryId
            };
        }
    }
}