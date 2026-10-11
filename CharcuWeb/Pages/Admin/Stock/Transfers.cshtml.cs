using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using TradicioCarnica.Services;

namespace TradicioCarnica.Pages.Admin.Stock
{
    public class TransferModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StockService _stock;

        public TransferModel(ApplicationDbContext context, StockService stock)
        {
            _context = context;
            _stock = stock;
        }

        [BindProperty]
        public TransferInput Input { get; set; } = new();

        public List<SelectListItem> Products { get; set; } = new();
        public List<SelectListItem> Warehouses { get; set; } = new();
        public List<SelectListItem> Types { get; set; } = new();

        public class TransferInput
        {
            [Required(ErrorMessage = "Selecciona un producto")]
            public long? ProductId { get; set; }

            [Required(ErrorMessage = "Selecciona el almacén de origen")]
            public long? FromWarehouseId { get; set; }

            [Required(ErrorMessage = "Selecciona el almacén de destino")]
            public long? ToWarehouseId { get; set; }

            [Required(ErrorMessage = "Selecciona un tipo")]
            public int? MovementType { get; set; }

            [Required(ErrorMessage = "Indica la cantidad")]
            [Range(0.01, 1000000, ErrorMessage = "La cantidad debe ser mayor que 0")]
            public decimal? Quantity { get; set; }
        }

        public async Task OnGetAsync()
        {
            await LoadAsync();

            // Preselecciona el tipo "TRANSFER" si existe
            var transfer = await _context.TEnums.FirstOrDefaultAsync(e =>
                e.Discriminator == TEnumTypes.Movement && e.Code == "TRANSFER");
            if (transfer != null) Input.MovementType = (int)transfer.Id;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadAsync();
            if (!ModelState.IsValid) return Page();

            if (Input.FromWarehouseId == Input.ToWarehouseId)
            {
                ModelState.AddModelError(string.Empty, "El origen y el destino deben ser distintos.");
                return Page();
            }

            if (!await _stock.MovementTypeExistsAsync(Input.MovementType!.Value))
            {
                ModelState.AddModelError(string.Empty, "Tipo de movimiento no válido.");
                return Page();
            }

            var qty = Input.Quantity!.Value;

            var error = await _stock.ApplyAsync(Input.ProductId!.Value, Input.FromWarehouseId!.Value, -qty, Input.MovementType.Value);
            if (error == null)
                error = await _stock.ApplyAsync(Input.ProductId.Value, Input.ToWarehouseId!.Value, qty, Input.MovementType.Value);

            if (error != null)
            {
                ModelState.AddModelError(string.Empty, error);
                return Page();
            }

            await _context.SaveChangesAsync();   // ambas filas en una sola transacción
            return RedirectToPage("Index");
        }

        private async Task LoadAsync()
        {
            Products = await _stock.ProductOptionsAsync();
            Warehouses = await _stock.WarehouseOptionsAsync();
            Types = await _stock.MovementTypeOptionsAsync();
        }
    }
}