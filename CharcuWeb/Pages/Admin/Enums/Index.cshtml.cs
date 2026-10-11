using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Enums
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public string Discriminator { get; set; } = TEnumTypes.Movement;
        public List<string> Discriminators { get; set; } = new();
        public List<TEnums> Items { get; set; } = new();

        [TempData]
        public string? Error { get; set; }

        public async Task OnGetAsync(string? discriminator)
        {
            await LoadAsync(discriminator);
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id, string discriminator)
        {
            var item = await _context.TEnums.FindAsync(id);
            if (item == null) return NotFound();

            var intId = (int)item.Id;
            bool inUse = item.Discriminator switch
            {
                TEnumTypes.Movement => await _context.TStockMovements.AnyAsync(m => m.MovementType == intId),
                TEnumTypes.Offer => await _context.TOffers.AnyAsync(o => o.OfferType == intId),
                _ => false
            };

            if (inUse)
            {
                Error = "No se puede eliminar: hay registros que usan este tipo.";
                return RedirectToPage(new { discriminator });
            }

            _context.TEnums.Remove(item);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { discriminator });
        }

        private async Task LoadAsync(string? discriminator)
        {
            var inDb = await _context.TEnums.Select(e => e.Discriminator).Distinct().ToListAsync();
            Discriminators = TEnumTypes.All.Union(inDb).ToList();
            Discriminator = discriminator ?? Discriminators.First();

            Items = await _context.TEnums
                .Where(e => e.Discriminator == Discriminator)
                .OrderBy(e => e.Name)
                .ToListAsync();
        }
    }
}