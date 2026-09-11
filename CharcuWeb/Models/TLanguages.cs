using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class TLanguages
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(4)]
        public string Code { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        public ICollection<TProductTranslations>? ProductTranslations { get; set; }

        public ICollection<TCategoriesTranslations>? CategoriesTranslations { get; set; }
    }
}