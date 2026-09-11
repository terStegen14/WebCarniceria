//using System;
//using TradicioCarnica.Data;    
//using TradicioCarnica.Models;
//using Microsoft.AspNetCore.Mvc.RazorPages;
//using Microsoft.EntityFrameworkCore;

//namespace TradicioCarnica.Pages.Products
//{
//    public class IndexModel : PageModel
//    {
//        private readonly ApplicationDbContext _context;

//        public IndexModel(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        public IList<TProduct> products { get; set; } = new List<TProduct>();

//        public async Task OnGetAsync()
//        {
//            products = await _context.TProducts
//                .Include(p => p.PriceUnit)
//                .Include(p => p.Subsubcategory)
//                    .ThenInclude(ss => ss.Subcategory)
//                        .ThenInclude(sc => sc.Category)
//                .Where(p => p.active)
//                .OrderBy(p => p.name)
//                .ToListAsync();
//        }
//    }
//}
