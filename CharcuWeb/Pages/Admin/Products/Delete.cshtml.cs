using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Products
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TProduct Product { get; set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            Product = await _context.TProducts
                .Include(p => p.ProductTranslations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var product = await _context.TProducts
                .FirstOrDefaultAsync(p => p.Id == Product.Id);

            if (product == null) return NotFound();

            product.Active = false;
            product.DateAme = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}