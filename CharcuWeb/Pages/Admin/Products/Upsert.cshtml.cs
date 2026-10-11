using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Products
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public UpsertModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public TProduct Product { get; set; } = new();

        [BindProperty]
        public decimal Price { get; set; }

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        [BindProperty]
        public Dictionary<int, TranslationInput> Translations { get; set; } = new();

        public List<TLanguages> Languages { get; set; } = new();
        public SelectList Categories { get; set; }
        public SelectList PriceUnits { get; set; }

        public bool IsEdit => Product.Id != 0;

        public class TranslationInput
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            await LoadLookupsAsync();

            if (id == null)
            {
                foreach (var lang in Languages)
                    Translations[lang.Id] = new TranslationInput();

                return Page();
            }

            Product = await _context.TProducts
                .Include(p => p.ProductTranslations)
                .Include(p => p.Prices)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null)
                return NotFound();

            Price = Product.CurrentPrice ?? 0;

            foreach (var lang in Languages)
            {
                var tr = Product.ProductTranslations?
                    .FirstOrDefault(x => x.LanguageId == lang.Id);

                Translations[lang.Id] = new TranslationInput
                {
                    Name = tr?.Name,
                    Description = tr?.Description
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadLookupsAsync();
            ModelState.Remove("Product.Category");
            ModelState.Remove("Product.PriceUnit");
            ModelState.Remove("Product.UserCreId");
            ModelState.Remove("Product.UserAmeId");
            if (!ModelState.IsValid)
                return Page();

            var userId = User.Identity?.Name ?? "system";
            var now = DateTime.Now;

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var ext = Path.GetExtension(ImageFile.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var folder = Path.Combine(_env.WebRootPath, "images", "products");
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                Product.ImgUrl = $"/images/products/{fileName}";
            }

            if (Product.Id == 0)
            {
                Product.DateCre = now;
                Product.DateAme = now;
                Product.UserCreId = userId;
                Product.UserAmeId = userId;

                if (string.IsNullOrEmpty(Product.ImgUrl))
                    Product.ImgUrl = null;

                _context.TProducts.Add(Product);
                await _context.SaveChangesAsync();
            }
            else
            {
                var existing = await _context.TProducts.FindAsync(Product.Id);
                if (existing == null) return NotFound();

                existing.Code = Product.Code;
                existing.CategoryId = Product.CategoryId;
                existing.PriceUnitId = Product.PriceUnitId;
                existing.Active = Product.Active;
                existing.DateAme = now;
                existing.UserAmeId = userId;

                if (!string.IsNullOrEmpty(Product.ImgUrl))
                    existing.ImgUrl = Product.ImgUrl;

                await _context.SaveChangesAsync();
                Product = existing;
            }

            // Precio: solo se guarda una nueva fila si ha cambiado
            var lastPrice = await _context.TProductPrices
                .Where(p => p.ProductId == Product.Id && p.DateTo == null)
                .OrderByDescending(p => p.DateFrom)
                .FirstOrDefaultAsync();

            if (lastPrice == null || lastPrice.Price != Price)
            {
                if (lastPrice != null)
                    lastPrice.DateTo = now;

                _context.TProductPrices.Add(new TProductPrice
                {
                    ProductId = Product.Id,
                    Price = Price,
                    DateFrom = now
                });
            }

            // Traducciones
            foreach (var tr in Translations)
            {
                if (string.IsNullOrWhiteSpace(tr.Value.Name))
                    continue;

                var existingTr = await _context.TProductTranslations
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == Product.Id &&
                        x.LanguageId == tr.Key);

                if (existingTr == null)
                {
                    _context.TProductTranslations.Add(new TProductTranslations
                    {
                        ProductId = Product.Id,
                        LanguageId = tr.Key,
                        Name = tr.Value.Name,
                        Description = tr.Value.Description
                    });
                }
                else
                {
                    existingTr.Name = tr.Value.Name;
                    existingTr.Description = tr.Value.Description;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }

        private async Task LoadLookupsAsync()
        {
            Languages = await _context.TLanguages.ToListAsync();

            Categories = new SelectList(
                await _context.TCategories
                    .Include(c => c.CategoriesTranslations)
                    .ToListAsync(),
                "Id",
                "Id" // se reemplaza abajo por el nombre traducido
            );

            // SelectList con nombre traducido (idioma 1 por defecto, como en el resto del admin)
            var cats = await _context.TCategories
                .Include(c => c.CategoriesTranslations)
                .Select(c => new
                {
                    c.Id,
                    Name = c.CategoriesTranslations.FirstOrDefault(t => t.LanguageId == 1).Name ?? "Sin nombre"
                })
                .ToListAsync();
            Categories = new SelectList(cats, "Id", "Name", Product.CategoryId);

            PriceUnits = new SelectList(
                await _context.TPriceUnits.ToListAsync(),
                "Id", "Code", Product.PriceUnitId);
        }
    }
}