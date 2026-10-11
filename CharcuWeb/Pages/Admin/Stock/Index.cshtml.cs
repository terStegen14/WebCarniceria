using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Stock
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public IndexModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        public List<TStock> Items { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();

        public async Task OnGetAsync(long? warehouseId)
        {
            Warehouses = await _stock.WarehouseOptionsAsync();
            foreach (var w in Warehouses) w.Selected = w.Value == warehouseId?.ToString();

            var q = _context.Stocks
                .Include(s => s.Warehouse)
                .Include(s => s.Product).ThenInclude(p => p.PriceUnit)
                .Include(s => s.Product).ThenInclude(p => p.ProductTranslations)
                .AsQueryable();

            if (warehouseId.HasValue)
                q = q.Where(s => s.WarehouseId == warehouseId);

            Items = await q.OrderBy(s => s.Warehouse.Name).ThenBy(s => s.Product.Code).ToListAsync();
        }
    }
}