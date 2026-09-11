using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(ProductId))]
    [Index(nameof(DateFrom))]
    public class TProductPrice
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public TProduct Product { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,5)")]
        public decimal Price { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }
    }
}