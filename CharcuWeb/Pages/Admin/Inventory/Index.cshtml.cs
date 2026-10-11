using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Inventory
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<TInventory> Items { get; set; } = new();

        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            Items = await _context.TInventories
                .Include(i => i.Warehouse)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Date).ThenByDescending(i => i.Id)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var inv = await _context.TInventories.FindAsync(id);
            if (inv == null) return NotFound();

            if (inv.Applied)
            {
                Error = "Un inventario aplicado no se puede eliminar (ya generó movimientos de stock).";
                return RedirectToPage();
            }

            _context.TInventories.Remove(inv);   // los items se borran en cascada
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}