using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(Code), IsUnique = true)]
    public class TWarehouse
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(20)]
        public string Code { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ICollection<TStock>? Stocks { get; set; }
    }
}