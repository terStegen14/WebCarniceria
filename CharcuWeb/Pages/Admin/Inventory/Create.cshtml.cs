using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Inventory
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public CreateModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        [BindProperty]
        public TInventory Inventory { get; set; } = new() { Date = DateTime.Today };

        public List<SelectListItem> Warehouses { get; set; } = new();

        public async Task OnGetAsync() => Warehouses = await _stock.WarehouseOptionsAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            Warehouses = await _stock.WarehouseOptionsAsync();

            ModelState.Remove("Inventory.Warehouse");
            ModelState.Remove("Inventory.Items");
            if (!ModelState.IsValid) return Page();

            Inventory.Applied = false;
            _context.TInventories.Add(Inventory);
            await _context.SaveChangesAsync();

            return RedirectToPage("Count", new { id = Inventory.Id });
        }
    }
}