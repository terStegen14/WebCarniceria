using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradicioCarnica.Data;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Stock
{
    public class MovementModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public MovementModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        [BindProperty]
        public MovementInput Input { get; set; } = new();

        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
        public List<SelectListItem> Types { get; set; } = new();

        public class MovementInput
        {
            [Required(ErrorMessage = "Selecciona un producto")]
            public long? ProductId { get; set; }

            [Required(ErrorMessage = "Selecciona un almacén")]
            public long? WarehouseId { get; set; }

            [Required(ErrorMessage = "Selecciona un tipo")]
            public int? MovementType { get; set; }

            [Required]
            public string Direction { get; set; } = "IN";

            [Required(ErrorMessage = "Indica la cantidad")]
            [Range(0.01, 1000000, ErrorMessage = "La cantidad debe ser mayor que 0")]
            public decimal? Quantity { get; set; }
        }

        public async Task OnGetAsync() => await LoadAsync();

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadAsync();
            if (!ModelState.IsValid) return Page();

            if (!await _stock.MovementTypeExistsAsync(Input.MovementType!.Value))
            {
                ModelState.AddModelError(string.Empty, "Tipo de movimiento no válido.");
                return Page();
            }

            var qty = Input.Direction == "OUT" ? -Input.Quantity!.Value : Input.Quantity!.Value;

            var error = await _stock.ApplyAsync(Input.ProductId!.Value, Input.WarehouseId!.Value, qty, Input.MovementType.Value);
            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return Page();
            }

            await _context.SaveChangesAsync();
            return RedirectToPage("Index", new { warehouseId = Input.WarehouseId });
        }

        private async Task LoadAsync()
        {
            Products = await _stock.ProductOptionsAsync();
            Warehouses = await _stock.WarehouseOptionsAsync();
            Types = await _stock.MovementTypeOptionsAsync();
        }
    }
}