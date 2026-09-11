using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradicioCarnica.Models
{
    public class TInventoryItems
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long InventoryId { get; set; }

        [ForeignKey(nameof(InventoryId))]
        public TInventory Inventory { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public TProduct Product { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,5)")]
        public decimal Quantity { get; set; }
    }
}