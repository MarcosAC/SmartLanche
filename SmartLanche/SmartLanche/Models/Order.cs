using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pendente;
        public int? ClientId { get; set; }
        public Client? Client { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();        
    }

    public enum OrderStatus
    {
        Pendente,
        EmPreparo,
        Pronto,
        Finalizado,
        Cancelado
    }

    public enum PaymentMethod
    {
        Dinheiro,
        Cartao,
        Pix,
        Fiado
    }
}
