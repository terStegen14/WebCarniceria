using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Prices
{
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public HistoryModel(ApplicationDbContext context) => _context = context;

        public TProduct Product { get; set; } = null!;
        public string ProductName { get; set; } = "";
        public List<TProductPrice> History { get; set; } = new();

        [BindProperty]
        [Required(ErrorMessage = "Indica un precio")]
        [Range(0.01, 100000, ErrorMessage = "El precio debe ser mayor que 0")]
        public decimal? NewPrice { get; set; }

        [TempData]
        public string? Message { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            return await LoadAsync(id) ? Page() : NotFound();
        }

        public async Task<IActionResult> OnPostAsync(long id)
        {
            if (!await LoadAsync(id)) return NotFound();
            if (!ModelState.IsValid) return Page();

            var now = DateTime.Now;
            var current = History.FirstOrDefault(h => h.DateTo == null);

            if (current != null && current.Price == NewPrice)
            {
                Message = "El precio introducido es igual al vigente; no se ha cambiado nada.";
                return RedirectToPage(new { id });
            }

            if (current != null)
                current.DateTo = now;

            _context.TProductPrices.Add(new TProductPrice
            {
                ProductId = id,
                Price = NewPrice!.Value,
                DateFrom = now
            });

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id, long priceId)
        {
            var prices = await _context.TProductPrices
                .Where(p => p.ProductId == id)
                .OrderByDescending(p => p.DateFrom)
                .ToListAsync();

            var target = prices.FirstOrDefault(p => p.Id == priceId);
            if (target == null) return NotFound();

            _context.TProductPrices.Remove(target);
            prices.Remove(target);

            // Si se borra el vigente, el anterior vuelve a quedar abierto
            if (target.DateTo == null && prices.Count > 0)
                prices[0].DateTo = null;

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }

        private async Task<bool> LoadAsync(long id)
        {
            var product = await _context.TProducts
                .Include(p => p.PriceUnit)
                .Include(p => p.ProductTranslations)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return false;

            Product = product;
            ProductName = product.ProductTranslations?.FirstOrDefault(t => t.LanguageId == 1)?.Name
                          ?? product.ProductTranslations?.FirstOrDefault()?.Name
                          ?? product.Code;

            History = await _context.TProductPrices
                .Where(p => p.ProductId == id)
                .OrderByDescending(p => p.DateFrom)
                .ToListAsync();
            return true;
        }
    }
}