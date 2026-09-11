using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.Models;

namespace TradicioCarnica.Pages.Admin.PriceUnits
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<TPriceUnit> Units { get; set; } = new List<TPriceUnit>();

        public async Task OnGetAsync()
        {
            Units = await _context.TPriceUnits
                .ToListAsync();
        }
    }
}