using SmartLanche.Helpers;
using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Models
{
    public class StockMovement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
        public MovementType Type { get; set; }
        public double Quantity { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string? Reason { get; set; }
    }
}
