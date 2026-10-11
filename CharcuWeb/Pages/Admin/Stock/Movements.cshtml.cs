using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Stock
{
    public class MovementsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public MovementsModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        public List<TStockMovements> Items { get; set; } = new();
        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
        public Dictionary<int, string> TypeNames { get; set; } = new();

        public async Task OnGetAsync(long? productId, long? warehouseId)
        {
            Products = await _stock.ProductOptionsAsync(onlyActive: false);
            Warehouses = await _stock.WarehouseOptionsAsync();
            foreach (var p in Products) p.Selected = p.Value == productId?.ToString();
            foreach (var w in Warehouses) w.Selected = w.Value == warehouseId?.ToString();

            TypeNames = await _context.TEnums
                .Where(e => e.Discriminator == TEnumTypes.Movement)
                .ToDictionaryAsync(e => (int)e.Id, e => e.Name);

            var q = _context.TStockMovements
                .Include(m => m.Product)
                .Include(m => m.Warehouse)
                .AsQueryable();

            if (productId.HasValue) q = q.Where(m => m.ProductId == productId);
            if (warehouseId.HasValue) q = q.Where(m => m.WarehouseId == warehouseId);

            Items = await q.OrderByDescending(m => m.Date).ThenByDescending(m => m.Id).Take(200).ToListAsync();
        }
    }
}