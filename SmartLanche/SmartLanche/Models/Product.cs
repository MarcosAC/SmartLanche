using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = "Geral";
        public decimal Price { get; set; }
        public bool IsCombo { get; set; }
        public string? Description { get; set; }
    }
}
