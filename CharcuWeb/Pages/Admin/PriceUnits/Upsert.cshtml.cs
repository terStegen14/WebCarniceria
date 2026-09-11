using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TradicioCarnica.Data;
using TradicioCarnica.Models;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Pages.Admin.PriceUnits
{
    public class UpsertModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UpsertModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public TPriceUnit Unit { get; set; } = new();

        public bool IsEdit => Unit.Id != 0;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                // Crear nueva unidad
                Unit = new TPriceUnit();
                return Page();
            }

            // Editar existente
            Unit = await _context.TPriceUnits
                .FirstOrDefaultAsync(x => x.Id == id);

            if (Unit == null)
                return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            if (Unit.Id == 0)
            {
                _context.TPriceUnits.Add(Unit);
            }
            else
            {
                _context.TPriceUnits.Update(Unit);
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("Index");
        }
    }
}