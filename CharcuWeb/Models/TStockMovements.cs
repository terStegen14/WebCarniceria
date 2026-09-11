using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradicioCarnica.Models
{
    public class TStockMovements
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public long ProductId { get; set; }

        [ForeignKey(nameof(ProductId))]
        public TProduct Product { get; set; }

        [Required]
        public long WarehouseId { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public TWarehouse Warehouse { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,5)")]
        public decimal Quantity { get; set; }

        [Required]
        public int MovementType { get; set; }

        [Required]
        public DateTime Date { get; set; }
    }
}