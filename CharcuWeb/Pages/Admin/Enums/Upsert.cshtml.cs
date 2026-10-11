using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Enums
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public UpsertModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public TEnums Item { get; set; } = new();

        public bool IsEdit => Item.Id != 0;

        public async Task<IActionResult> OnGetAsync(long? id, string? discriminator)
        {
            if (id == null)
            {
                Item = new TEnums { Discriminator = discriminator ?? TEnumTypes.Movement };
                return Page();
            }

            var e = await _context.TEnums.FindAsync(id);
            if (e == null) return NotFound();
            Item = e;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var duplicate = await _context.TEnums.AnyAsync(e =>
                e.Discriminator == Item.Discriminator && e.Code == Item.Code && e.Id != Item.Id);
            if (duplicate)
            {
                ModelState.AddModelError("Item.Code", "Ya existe un tipo con ese código.");
                return Page();
            }

            if (Item.Id == 0)
                _context.TEnums.Add(Item);
            else
            {
                var existing = await _context.TEnums.FindAsync(Item.Id);
                if (existing == null) return NotFound();
                existing.Code = Item.Code;
                existing.Name = Item.Name;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index", new { discriminator = Item.Discriminator });
        }
    }
}