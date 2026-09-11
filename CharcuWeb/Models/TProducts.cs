using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(Code), IsUnique = true)]
    [Index(nameof(CategoryId))]
    [Index(nameof(PriceUnitId))]
    [Index(nameof(Active))]
    public class TProduct
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Code { get; set; }

        [MaxLength(500)]
        public string? ImgUrl { get; set; }

        [Required]
        public long CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public TCategories Category { get; set; }

        [Required]
        public long PriceUnitId { get; set; }

        [ForeignKey(nameof(PriceUnitId))]
        public TPriceUnit PriceUnit { get; set; }

        [Required]
        public bool Active { get; set; }

        [Required]
        public DateTime DateCre { get; set; }

        [Required]
        public DateTime DateAme { get; set; }

        [Required]
        public string UserCreId { get; set; }

        [Required]
        public string UserAmeId { get; set; }

        [NotMapped]
        public decimal? CurrentPrice =>
            Prices?.OrderByDescending(p => p.DateFrom).FirstOrDefault()?.Price;
        public ICollection<TProductTranslations>? ProductTranslations { get; set; }
        public ICollection<TStock>? Stocks { get; set; }
        public ICollection<TProductPrice>? Prices { get; set; }
        public ICollection<TOffersProducts>? OfferProducts { get; set; }
    }
}