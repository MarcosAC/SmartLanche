using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(500)]
        public string? Observation { get; set; }

        [Required]
        public decimal OutstangingBalance { get; set; } = 0.00m;
    }
}
