using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public bool IsActive { get; set; } = true;
        public double StockQuantity { get; set; }
        public double MinStockLevel { get; set; } = 5;

        [NotMapped]
        public bool IsLowStock => StockQuantity <= MinStockLevel;
    }
}
