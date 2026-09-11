using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(DateFrom))]
    [Index(nameof(DateTo))]
    [Index(nameof(Active))]
    public class TOffers
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public int OfferType { get; set; }

        [Column(TypeName = "decimal(10,5)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,5)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(10,5)")]
        public decimal BuyQty { get; set; }

        [Column(TypeName = "decimal(10,5)")]
        public decimal GetQty { get; set; }

        [Required]
        public DateTime DateFrom { get; set; }

        [Required]
        public DateTime DateTo { get; set; }

        [Required]
        public bool Active { get; set; }

        public ICollection<TOffersProducts>? OfferProducts { get; set; }
    }
}