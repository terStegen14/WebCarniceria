using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class TPriceUnit
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [MaxLength(100)]
        public string? Description { get; set; }

        public ICollection<TProduct>? Products { get; set; }
    }
}