using System.ComponentModel.DataAnnotations;

namespace TradicioCarnica.Models
{
    public class TCategories
    {
        [Key]
        public long Id { get; set; }

        public ICollection<TProduct>? Products { get; set; }

        public ICollection<TCategoriesTranslations>? CategoriesTranslations { get; set; }
    }
}