using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TradicioCarnica.Data;
using TradicioCarnica.ViewModels;

public class CatalogController : Controller
{
    private readonly ApplicationDbContext _context;

    public CatalogController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(long? categoryId)
    {
        int lang = 1; // catalán por ejemplo

        var categories = await _context.TCategories
            .Include(c => c.CategoriesTranslations
            .Where(t => t.LanguageId == lang))
            .ToListAsync();

        var productsQuery = _context.TProducts
            .Where(p => p.Active)
            .Include(p => p.ProductTranslations
                .Where(t => t.LanguageId == lang))
            .Include(p => p.Category)
            .Include(p => p.Prices)
            .Include(p => p.PriceUnit)
            .AsQueryable();

        if (categoryId.HasValue)
            productsQuery = productsQuery.Where(p => p.CategoryId == categoryId);

        var vm = new CatalogViewModel
        {
            Categories = categories,
            Products = await productsQuery.ToListAsync(),
            SelectedCategoryId = categoryId
        };

        return View(vm);
    }
}