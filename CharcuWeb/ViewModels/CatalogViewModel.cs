using TradicioCarnica.Models;

namespace TradicioCarnica.ViewModels
{
    public class CatalogViewModel
    {
        public List<TCategories> Categories { get; set; }

        public List<TProduct> Products { get; set; }

        public long? SelectedCategoryId { get; set; }
    }
}