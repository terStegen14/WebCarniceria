using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(CategoryId), nameof(LanguageId), IsUnique = true)]
    public class TCategoriesTranslations
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public TCategories Category { get; set; }

        [Required]
        public int LanguageId { get; set; }

        [ForeignKey(nameof(LanguageId))]
        public TLanguages Language { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
    }
}