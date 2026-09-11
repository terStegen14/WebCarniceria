using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Categories
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UpsertModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TCategories Category { get; set; } = new();

        [BindProperty]
        public Dictionary<int, TranslationInput> Translations { get; set; } = new();

        public List<TLanguages> Languages { get; set; }

        public bool IsEdit => Category.Id != 0;

        public class TranslationInput
        {
            public string? Name { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            Languages = await _context.TLanguages.ToListAsync();

            if (id == null)
            {
                foreach (var lang in Languages)
                    Translations[lang.Id] = new TranslationInput();

                return Page();
            }

            Category = await _context.TCategories
                .Include(x => x.CategoriesTranslations)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (Category == null)
                return NotFound();

            foreach (var lang in Languages)
            {
                var tr = Category.CategoriesTranslations?
                    .FirstOrDefault(x => x.LanguageId == lang.Id);

                Translations[lang.Id] = new TranslationInput
                {
                    Name = tr?.Name
                };
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Languages = await _context.TLanguages.ToListAsync();

            if (Category.Id == 0)
            {
                _context.TCategories.Add(Category);
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.TCategories.Update(Category);
                await _context.SaveChangesAsync();
            }

            foreach (var tr in Translations)
            {
                if (string.IsNullOrWhiteSpace(tr.Value.Name))
                    continue;

                var existing = await _context.TCategoriesTranslations
                    .FirstOrDefaultAsync(x =>
                        x.CategoryId == Category.Id &&
                        x.LanguageId == tr.Key);

                if (existing == null)
                {
                    _context.TCategoriesTranslations.Add(new TCategoriesTranslations
                    {
                        CategoryId = Category.Id,
                        LanguageId = tr.Key,
                        Name = tr.Value.Name
                    });
                }
                else
                {
                    existing.Name = tr.Value.Name;
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}