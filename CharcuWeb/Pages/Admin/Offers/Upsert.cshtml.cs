using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Offers
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public UpsertModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public TOffers Offer { get; set; } = new()
        {
            DateFrom = DateTime.Today,
            DateTo = DateTime.Today.AddMonths(1),
            Active = true
        };

        [BindProperty]
        public List<long> SelectedProductIds { get; set; } = new();

        public List<SelectListItem> Types { get; set; } = new();
        public Dictionary<int, string> TypeCodes { get; set; } = new();
        public List<(long Id, string Label)> Products { get; set; } = new();

        public bool IsEdit => Offer.Id != 0;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            await LoadAsync();
            if (id == null) return Page();

            var offer = await _context.TOffers.Include(o => o.OfferProducts).FirstOrDefaultAsync(o => o.Id == id);
            if (offer == null) return NotFound();

            Offer = offer;
            SelectedProductIds = offer.OfferProducts?.Select(op => op.ProductId).ToList() ?? new();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadAsync();
            ModelState.Remove("Offer.OfferProducts");

            var code = TypeCodes.GetValueOrDefault(Offer.OfferType);
            if (!TypeCodes.ContainsKey(Offer.OfferType))
                ModelState.AddModelError(string.Empty, "Selecciona un tipo de oferta válido.");

            switch (code)
            {
                case "FIXED_PRICE":
                    if (Offer.Price <= 0) ModelState.AddModelError(string.Empty, "El precio debe ser mayor que 0.");
                    Offer.DiscountPercent = 0; Offer.BuyQty = 0; Offer.GetQty = 0;
                    break;
                case "PERCENT":
                    if (Offer.DiscountPercent <= 0 || Offer.DiscountPercent > 100)
                        ModelState.AddModelError(string.Empty, "El descuento debe estar entre 0 y 100.");
                    Offer.Price = 0; Offer.BuyQty = 0; Offer.GetQty = 0;
                    break;
                case "BUY_X_GET_Y":
                    if (Offer.BuyQty <= 0 || Offer.GetQty <= 0)
                        ModelState.AddModelError(string.Empty, "Indica las unidades que compra y las que se lleva gratis.");
                    Offer.Price = 0; Offer.DiscountPercent = 0;
                    break;
            }

            if (Offer.DateTo.Date < Offer.DateFrom.Date)
                ModelState.AddModelError(string.Empty, "La fecha final no puede ser anterior a la inicial.");

            if (SelectedProductIds.Count == 0)
                ModelState.AddModelError(string.Empty, "Selecciona al menos un producto.");

            if (!ModelState.IsValid) return Page();

            if (Offer.Id == 0)
            {
                _context.TOffers.Add(Offer);
            }
            else
            {
                var existing = await _context.TOffers.FindAsync(Offer.Id);
                if (existing == null) return NotFound();

                existing.OfferType = Offer.OfferType;
                existing.Price = Offer.Price;
                existing.DiscountPercent = Offer.DiscountPercent;
                existing.BuyQty = Offer.BuyQty;
                existing.GetQty = Offer.GetQty;
                existing.DateFrom = Offer.DateFrom;
                existing.DateTo = Offer.DateTo;
                existing.Active = Offer.Active;
            }
            await _context.SaveChangesAsync();

            // Sincronizar productos
            var offerId = Offer.Id;
            var current = await _context.TOffersProducts.Where(op => op.OfferId == offerId).ToListAsync();
            var selected = SelectedProductIds.Distinct().ToHashSet();

            _context.TOffersProducts.RemoveRange(current.Where(op => !selected.Contains(op.ProductId)));

            var already = current.Select(op => op.ProductId).ToHashSet();
            foreach (var pid in selected.Where(pid => !already.Contains(pid)))
                _context.TOffersProducts.Add(new TOffersProducts { OfferId = offerId, ProductId = pid });

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }

        private async Task LoadAsync()
        {
            var types = await _context.TEnums
                .Where(e => e.Discriminator == TEnumTypes.Offer)
                .OrderBy(e => e.Name).ToListAsync();

            Types = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
            TypeCodes = types.ToDictionary(t => (int)t.Id, t => t.Code);

            var products = await _context.TProducts
                .Include(p => p.ProductTranslations)
                .Where(p => p.Active)
                .OrderBy(p => p.Code).ToListAsync();

            Products = products.Select(p => (p.Id,
                $"{p.Code} - {p.ProductTranslations?.FirstOrDefault(t => t.LanguageId == 1)?.Name ?? p.ProductTranslations?.FirstOrDefault()?.Name}"))
                .ToList();
        }
    }
}