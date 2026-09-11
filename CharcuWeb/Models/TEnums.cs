using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace TradicioCarnica.Models
{
    [Index(nameof(Discriminator), nameof(Code), IsUnique = true)]
    public class TEnums
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string Discriminator { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Name { get; set; }
    }
}