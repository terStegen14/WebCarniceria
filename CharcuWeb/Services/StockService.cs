using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Services
{
    public class StockService
    {
        private readonly ApplicationDbContext _context;
        public StockService(ApplicationDbContext context) => _context = context;

        /// Añade el movimiento y actualiza TStock. No hace SaveChanges (lo hace quien llama).
        /// Devuelve un mensaje de error o null si todo bien.
        public async Task<string?> ApplyAsync(long productId, long warehouseId, decimal quantity, int movementType)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);

            var current = stock?.Quantity ?? 0;
            if (current + quantity < 0)
                return $"Stock insuficiente en el almacén (disponible: {current:0.##}).";

            if (stock == null)
                _context.Stocks.Add(new TStock { ProductId = productId, WarehouseId = warehouseId, Quantity = quantity });
            else
                stock.Quantity += quantity;

            _context.TStockMovements.Add(new TStockMovements
            {
                ProductId = productId,
                WarehouseId = warehouseId,
                Quantity = quantity,
                MovementType = movementType,
                Date = DateTime.Now
            });

            return null;
        }

        public async Task<List<SelectListItem>> ProductOptionsAsync(bool onlyActive = true)
        {
            var q = _context.TProducts.Include(p => p.ProductTranslations).AsQueryable();
            if (onlyActive) q = q.Where(p => p.Active);

            var products = await q.OrderBy(p => p.Code).ToListAsync();
            return products.Select(p => new SelectListItem(
                $"{p.Code} - {p.ProductTranslations?.FirstOrDefault(t => t.LanguageId == 1)?.Name ?? p.ProductTranslations?.FirstOrDefault()?.Name}",
                p.Id.ToString())).ToList();
        }

        public async Task<List<SelectListItem>> WarehouseOptionsAsync()
        {
            return (await _context.TWarehouses.OrderBy(w => w.Name).ToListAsync())
                .Select(w => new SelectListItem($"{w.Code} - {w.Name}", w.Id.ToString())).ToList();
        }

        public async Task<List<SelectListItem>> MovementTypeOptionsAsync()
        {
            return (await _context.TEnums.Where(e => e.Discriminator == TEnumTypes.Movement)
                    .OrderBy(e => e.Name).ToListAsync())
                .Select(e => new SelectListItem(e.Name, e.Id.ToString())).ToList();
        }

        public Task<bool> MovementTypeExistsAsync(int id) =>
            _context.TEnums.AnyAsync(e => e.Id == id && e.Discriminator == TEnumTypes.Movement);
    }
}