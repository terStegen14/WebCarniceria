using System;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Pages.Admin.Products
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<TProduct> Products { get; set; } = default!;

        [BindProperty]
        public long ProductId { get; set; }
        [BindProperty]
        public bool NewState { get; set; }

        public async Task OnGetAsync()
        {
            Products = await _context.TProducts
                .Include(p => p.PriceUnit)
                .Include(p => p.ProductTranslations)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostToggleActiveAsync()
        {
            var product = await _context.TProducts.FindAsync(ProductId);
            if (product == null) return NotFound();

            product.Active = NewState;
            product.DateAme = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}