using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Inventory
{
    public class CountModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public CountModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        public TInventory Inventory { get; set; } = null!;

        [BindProperty]
        public List<Row> Rows { get; set; } = new();

        [TempData] public string? Message { get; set; }
        [TempData] public string? Error { get; set; }

        public class Row
        {
            public long ProductId { get; set; }
            public decimal? Counted { get; set; }

            // Solo para mostrar
            public string Code { get; set; } = "";
            public string Name { get; set; } = "";
            public string? Unit { get; set; }
            public decimal SystemQty { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            return await LoadAsync(id) ? Page() : NotFound();
        }

        public async Task<IActionResult> OnPostSaveAsync(long id)
        {
            var inv = await _context.TInventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.Applied) { Error = "Este inventario ya está aplicado."; return RedirectToPage(new { id }); }

            await SaveCountsAsync(id);
            await _context.SaveChangesAsync();

            Message = "Conteo guardado.";
            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostApplyAsync(long id)
        {
            var inv = await _context.TInventories.FindAsync(id);
            if (inv == null) return NotFound();
            if (inv.Applied) { Error = "Este inventario ya está aplicado."; return RedirectToPage(new { id }); }

            var adjustment = await _context.TEnums.FirstOrDefaultAsync(e =>
                e.Discriminator == TEnumTypes.Movement && e.Code == "ADJUSTMENT");
            if (adjustment == null)
            {
                Error = "Falta el tipo de movimiento ADJUSTMENT. Créalo en Tipos (MovementType) antes de aplicar.";
                return RedirectToPage(new { id });
            }

            await using var tx = await _context.Database.BeginTransactionAsync();

            await SaveCountsAsync(id);
            await _context.SaveChangesAsync();

            var items = await _context.TInventoryItems.Where(i => i.InventoryId == id).ToListAsync();
            var stocks = await _context.Stocks
                .Where(s => s.WarehouseId == inv.WarehouseId)
                .ToDictionaryAsync(s => s.ProductId, s => s.Quantity);

            foreach (var item in items)
            {
                var current = stocks.GetValueOrDefault(item.ProductId);
                var diff = item.Quantity - current;
                if (diff == 0) continue;

                var error = await _stock.ApplyAsync(item.ProductId, inv.WarehouseId, diff, (int)adjustment.Id);
                if (error != null)
                {
                    await tx.RollbackAsync();
                    Error = error;
                    return RedirectToPage(new { id });
                }
            }

            inv.Applied = true;
            inv.DateApplied = DateTime.Now;
            await _context.SaveChangesAsync();
            await tx.CommitAsync();

            Message = "Ajustes aplicados correctamente.";
            return RedirectToPage(new { id });
        }

        private async Task SaveCountsAsync(long id)
        {
            var existing = await _context.TInventoryItems.Where(i => i.InventoryId == id).ToListAsync();

            foreach (var r in Rows)
            {
                var item = existing.FirstOrDefault(i => i.ProductId == r.ProductId);

                if (r.Counted == null || r.Counted < 0)
                {
                    if (item != null) _context.TInventoryItems.Remove(item);
                    continue;
                }

                if (item == null)
                    _context.TInventoryItems.Add(new TInventoryItems
                    {
                        InventoryId = id,
                        ProductId = r.ProductId,
                        Quantity = r.Counted.Value
                    });
                else
                    item.Quantity = r.Counted.Value;
            }
        }

        private async Task<bool> LoadAsync(long id)
        {
            var inv = await _context.TInventories
                .Include(i => i.Warehouse)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (inv == null) return false;
            Inventory = inv;

            var stocks = await _context.Stocks
                .Where(s => s.WarehouseId == inv.WarehouseId)
                .ToDictionaryAsync(s => s.ProductId, s => s.Quantity);

            var products = await _context.TProducts
                .Include(p => p.PriceUnit)
                .Include(p => p.ProductTranslations)
                .Where(p => p.Active || p.Stocks!.Any(s => s.WarehouseId == inv.WarehouseId && s.Quantity != 0))
                .OrderBy(p => p.Code)
                .ToListAsync();

            Rows = products.Select(p => new Row
            {
                ProductId = p.Id,
                Code = p.Code,
                Name = p.ProductTranslations?.FirstOrDefault(t => t.LanguageId == 1)?.Name
                       ?? p.ProductTranslations?.FirstOrDefault()?.Name ?? "-",
                Unit = p.PriceUnit?.Code,
                SystemQty = stocks.GetValueOrDefault(p.Id),
                Counted = inv.Items?.FirstOrDefault(i => i.ProductId == p.Id)?.Quantity
            }).ToList();

            return true;
        }
    }
}