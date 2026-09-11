using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public TProduct Product { get; set; } = default!;

        [BindProperty]
        public Dictionary<int, ProductTranslationInput> Translations { get; set; } = new();

        [BindProperty]
        public IFormFile? ImageFile { get; set; }

        [BindProperty]
        public decimal Price { get; set; }

        public List<TLanguages> Languages { get; set; } = new();

        public SelectList Categories { get; set; }
        public SelectList PriceUnits { get; set; }

        public bool IsEdit => Product.Id != 0;

        public class ProductTranslationInput
        {
            public string? Name { get; set; }
            public string? Description { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            Languages = await _context.TLanguages.ToListAsync();

            // ID -> Text para categorías usando traducciones (ejemplo LanguageId = 1)
            var currentLangId = 1; // cambiar según el idioma de la vista/admin
            Categories = new SelectList(
                _context.TCategories
                    .Include(c => c.CategoriesTranslations)
                    .ToList()
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.CategoriesTranslations
                                .FirstOrDefault(t => t.LanguageId == currentLangId)?.Name ?? "Sin nombre"
                    }),
                "Id",
                "Name"
            );

            // PriceUnits normal
            PriceUnits = new SelectList(_context.TPriceUnits, "Id", "Code");

            if (id == null)
            {
                Product = new TProduct
                {
                    Active = true
                };

                foreach (var lang in Languages)
                    Translations[lang.Id] = new ProductTranslationInput();

                return Page();
            }

            Product = await _context.TProducts
                .Include(p => p.ProductTranslations)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (Product == null)
                return NotFound();

            foreach (var lang in Languages)
            {
                var tr = Product.ProductTranslations?
                    .FirstOrDefault(t => t.LanguageId == lang.Id);

                Translations[lang.Id] = new ProductTranslationInput
                {
                    Name = tr?.Name,
                    Description = tr?.Description
                };
            }
            if (Product.Id != 0 && Price > 0)
            {
                var price = new TProductPrice
                {
                    ProductId = Product.Id,
                    Price = Price,
                    DateFrom = DateTime.Now
                };

                _context.TProductPrices.Add(price);
            }
            if (ImageFile != null)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "images", "products");

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);

                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                Product.ImgUrl = "/images/products/" + fileName;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Languages = await _context.TLanguages.ToListAsync();

            if (!ModelState.IsValid)
                return Page();

            if (Product.Id == 0)
            {
                Product.DateCre = DateTime.Now;
                Product.DateAme = DateTime.Now;
                Product.UserCreId = "admin";
                Product.UserAmeId = "admin";

                _context.TProducts.Add(Product);
                await _context.SaveChangesAsync();
            }
            else
            {
                Product.DateAme = DateTime.Now;
                _context.TProducts.Update(Product);
                await _context.SaveChangesAsync();
            }

            foreach (var tr in Translations)
            {
                if (string.IsNullOrWhiteSpace(tr.Value.Name))
                    continue;

                var existing = await _context.TProductTranslations
                    .FirstOrDefaultAsync(x =>
                        x.ProductId == Product.Id &&
                        x.LanguageId == tr.Key);

                if (existing == null)
                {
                    _context.TProductTranslations.Add(new TProductTranslations
                    {
                        ProductId = Product.Id,
                        LanguageId = tr.Key,
                        Name = tr.Value.Name!,
                        Description = tr.Value.Description
                    });
                }
                else
                {
                    existing.Name = tr.Value.Name!;
                    existing.Description = tr.Value.Description;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}