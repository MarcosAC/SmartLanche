using System.ComponentModel.DataAnnotations;

namespace SmartLanche.Helpers
{
    public enum OrderStatus
    {
        [Display(Name = "Pendente")]
        Pending,
        [Display(Name = "Em Preparo")]
        InPreparation,
        [Display(Name = "Pronto")]
        Ready,
        [Display(Name = "Finalizado")]
        Completed,
        [Display(Name = "Cancelado")]
        Cancelled
    }

    public enum PaymentMethod
    {
        [Display(Name = "Dinheiro")]
        Cash,
        [Display(Name = "Cartão")]
        Card,
        [Display(Name = "Pix")]
        Pix,
        [Display(Name = "Fiado")]
        Credit
    }
}
