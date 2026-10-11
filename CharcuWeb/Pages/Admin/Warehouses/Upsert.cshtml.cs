using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.Warehouses
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public UpsertModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public TWarehouse Warehouse { get; set; } = new();

        public bool IsEdit => Warehouse.Id != 0;

        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null) return Page();

            var w = await _context.TWarehouses.FindAsync(id);
            if (w == null) return NotFound();
            Warehouse = w;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("Warehouse.Stocks");
            if (!ModelState.IsValid) return Page();

            var duplicate = await _context.TWarehouses
                .AnyAsync(w => w.Code == Warehouse.Code && w.Id != Warehouse.Id);
            if (duplicate)
            {
                ModelState.AddModelError("Warehouse.Code", "Ya existe un almacén con ese código.");
                return Page();
            }

            if (Warehouse.Id == 0)
                _context.TWarehouses.Add(Warehouse);
            else
            {
                var existing = await _context.TWarehouses.FindAsync(Warehouse.Id);
                if (existing == null) return NotFound();
                existing.Code = Warehouse.Code;
                existing.Name = Warehouse.Name;
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}