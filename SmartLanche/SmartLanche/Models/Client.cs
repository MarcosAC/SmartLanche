using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Models
{
    public class Client
    {
        [Key]
        public int Id { get; set; }

        [Required]        
        public string Name { get; set; } = string.Empty;       
        public string? Phone { get; set; }
        public string? Observations { get; set; }        
        public decimal OutstandingBalance { get; set; } = 0.00m;
    }
}
