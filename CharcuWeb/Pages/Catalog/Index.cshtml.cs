using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.ViewModels;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Catalog
{
    public class IndexModel : PageModel
    {
        private const int PageSize = 24;
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context) => _context = context;

        public CatalogViewModel Catalog { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; } = 1;
        public Dictionary<long, OfferInfo> Offers { get; set; } = new();
        [BindProperty(SupportsGet = true)] public long? CategoryId { get; set; }
        [BindProperty(SupportsGet = true)] public string? Q { get; set; }
        [BindProperty(SupportsGet = true)] public string? Sort { get; set; }
        [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;

        public async Task OnGetAsync() => await LoadCatalogAsync();

        // Devuelve solo el bloque de resultados (para el fetch de JavaScript)
        public async Task<IActionResult> OnGetResultsAsync()
        {
            await LoadCatalogAsync();
            return Partial("_ProductResults", this);
        }

        private async Task LoadCatalogAsync()
        {
            int lang = await ResolveLanguageIdAsync();

            var categories = await _context.TCategories
                .Include(c => c.CategoriesTranslations.Where(t => t.LanguageId == lang))
                .ToListAsync();

            var query = _context.TProducts.Where(p => p.Active);

            if (CategoryId.HasValue)
                query = query.Where(p => p.CategoryId == CategoryId);

            if (!string.IsNullOrWhiteSpace(Q))
            {
                var term = Q.Trim();
                if (term.Length > 50) term = term[..50];

                query = query.Where(p =>
                    p.Code.Contains(term) ||
                    p.ProductTranslations!.Any(t =>
                        EF.Functions.Collate(t.Name, "SQL_Latin1_General_CP1_CI_AI").Contains(term)));
            }

            TotalCount = await query.CountAsync();
            TotalPages = Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
            PageNumber = Math.Clamp(PageNumber, 1, TotalPages);

            query = Sort switch
            {
                "price_asc" => query
                    .OrderBy(p => p.Prices!.Any() ? 0 : 1)
                    .ThenBy(p => p.Prices!.OrderByDescending(x => x.DateFrom).Select(x => x.Price).FirstOrDefault()),
                "price_desc" => query
                    .OrderBy(p => p.Prices!.Any() ? 0 : 1)
                    .ThenByDescending(p => p.Prices!.OrderByDescending(x => x.DateFrom).Select(x => x.Price).FirstOrDefault()),
                _ => query
                    .OrderBy(p => p.ProductTranslations!.Where(t => t.LanguageId == lang).Select(t => t.Name).FirstOrDefault() ?? p.Code)
            };

            var products = await query
                .Include(p => p.ProductTranslations.Where(t => t.LanguageId == lang))
                .Include(p => p.PriceUnit)
                .Include(p => p.Prices)
                .Include(p => p.Category).ThenInclude(c => c.CategoriesTranslations.Where(t => t.LanguageId == lang))
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
            // ---- Ofertas vigentes de los productos de esta página ----
            Offers = new();
            if (products.Count > 0)
            {
                var ids = products.Select(p => p.Id).ToList();
                var today = DateTime.Today;

                var types = await _context.TEnums
                    .Where(e => e.Discriminator == TEnumTypes.Offer)
                    .ToDictionaryAsync(e => (int)e.Id, e => e.Code);

                var rows = await _context.TOffersProducts
                    .Where(op => ids.Contains(op.ProductId)
                              && op.Offer.Active
                              && op.Offer.DateFrom.Date <= today
                              && op.Offer.DateTo.Date >= today)
                    .Select(op => new { op.ProductId, op.Offer })
                    .ToListAsync();

                foreach (var p in products)
                {
                    var basePrice = p.CurrentPrice;
                    OfferInfo? best = null;

                    foreach (var o in rows.Where(r => r.ProductId == p.Id).Select(r => r.Offer))
                    {
                        types.TryGetValue(o.OfferType, out var code);

                        OfferInfo? info = code switch
                        {
                            "FIXED_PRICE" when o.Price > 0 =>
                                new OfferInfo { NewPrice = o.Price, Badge = "Oferta" },

                            "PERCENT" when basePrice.HasValue && o.DiscountPercent > 0 =>
                                new OfferInfo
                                {
                                    NewPrice = Math.Round(basePrice.Value * (1 - o.DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero),
                                    Badge = $"-{o.DiscountPercent:0.##}%"
                                },

                            "BUY_X_GET_Y" when o.BuyQty > 0 && o.GetQty > 0 =>
                                new OfferInfo { Badge = $"{o.BuyQty:0.##}+{o.GetQty:0.##} gratis" },

                            _ => null
                        };

                        if (info == null) continue;

                        // Un precio rebajado no puede ser mayor o igual que el normal
                        if (info.NewPrice.HasValue && basePrice.HasValue && info.NewPrice >= basePrice) continue;

                        // Gana la oferta que deja el precio más bajo; las de precio ganan a las de "compra X"
                        if (best == null
                            || (info.NewPrice.HasValue && (!best.NewPrice.HasValue || info.NewPrice < best.NewPrice)))
                            best = info;
                    }

                    if (best != null) Offers[p.Id] = best;
                }
            }
            Catalog = new CatalogViewModel
            {
                Categories = categories,
                Products = products,
                SelectedCategoryId = CategoryId
            };
        }

        // Sugerencias del buscador: /Catalog?handler=Suggest&q=...&categoryId=...
        public async Task<IActionResult> OnGetSuggestAsync(string? q, long? categoryId)
        {
            var term = q?.Trim() ?? "";
            if (term.Length < 2) return new JsonResult(Array.Empty<string>());
            if (term.Length > 50) term = term[..50];

            int lang = await ResolveLanguageIdAsync();

            var query = _context.TProducts.Where(p => p.Active);
            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var names = await query
                .SelectMany(p => p.ProductTranslations!.Where(t => t.LanguageId == lang))
                .Where(t => EF.Functions.Collate(t.Name, "SQL_Latin1_General_CP1_CI_AI").Contains(term))
                .Select(t => t.Name)
                .Distinct()
                .OrderBy(n => n)
                .Take(8)
                .ToListAsync();

            return new JsonResult(names);
        }

        private async Task<int> ResolveLanguageIdAsync()
        {
            var cultureCode = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; // "es" o "ca"

            var language = await _context.TLanguages
                .FirstOrDefaultAsync(l => l.Code.ToUpper() == cultureCode.ToUpper());

            // Fallback: si no encuentra el idioma actual, coge el primero disponible
            return language?.Id ?? await _context.TLanguages.Select(l => l.Id).FirstOrDefaultAsync();
        }
    }
}