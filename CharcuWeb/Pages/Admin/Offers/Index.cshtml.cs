using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Offers
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public List<TOffers> Items { get; set; } = new();
        private Dictionary<int, TEnums> _types = new();

        public async Task OnGetAsync()
        {
            _types = await _context.TEnums
                .Where(e => e.Discriminator == TEnumTypes.Offer)
                .ToDictionaryAsync(e => (int)e.Id);

            Items = await _context.TOffers
                .Include(o => o.OfferProducts)
                .OrderByDescending(o => o.DateFrom)
                .ToListAsync();
        }

        public string TypeName(int type) => _types.TryGetValue(type, out var t) ? t.Name : type.ToString();

        public string Summary(TOffers o)
        {
            var code = _types.TryGetValue(o.OfferType, out var t) ? t.Code : "";
            return code switch
            {
                "FIXED_PRICE" => $"{o.Price:0.00} €",
                "PERCENT" => $"{o.DiscountPercent:0.##} % de descuento",
                "BUY_X_GET_Y" => $"Compra {o.BuyQty:0.##}, llévate {o.GetQty:0.##} gratis",
                _ => "-"
            };
        }

        public async Task<IActionResult> OnPostDeleteAsync(long id)
        {
            var offer = await _context.TOffers.FindAsync(id);
            if (offer == null) return NotFound();

            _context.TOffers.Remove(offer);   // OfferProducts se borra en cascada
            await _context.SaveChangesAsync();
            return RedirectToPage();
        }
    }
}