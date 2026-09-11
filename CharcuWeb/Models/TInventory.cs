using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TradicioCarnica.Models
{
    public class TInventory
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        public long WarehouseId { get; set; }

        [ForeignKey(nameof(WarehouseId))]
        public TWarehouse Warehouse { get; set; }

        [Required]
        public DateTime Date { get; set; }

        public ICollection<TInventoryItems>? Items { get; set; }
    }
}