using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Warehouses
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<TWarehouse> Warehouses { get; set; } = new List<TWarehouse>();

        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync()
        {
            Warehouses = await _context.TWarehouses.OrderBy(w => w.Code).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var warehouse = await _context.TWarehouses.FindAsync(id);
            if (warehouse == null) return NotFound();

            var inUse = await _context.Stocks.AnyAsync(s => s.WarehouseId == id)
                     || await _context.TStockMovements.AnyAsync(m => m.WarehouseId == id)
                     || await _context.TInventories.AnyAsync(i => i.WarehouseId == id);

            if (inUse)
            {
                Error = "No se puede eliminar: el almacén tiene stock, movimientos o inventarios asociados.";
                return RedirectToPage();
            }

            _context.TWarehouses.Remove(warehouse);
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}