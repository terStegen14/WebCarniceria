using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(OfferId), nameof(ProductId), IsUnique = true)]
    public class TOffersProducts
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long OfferId { get; set; }

        [ForeignKey(nameof(OfferId))]
        public TOffers Offer { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public TProduct Product { get; set; }
    }
}