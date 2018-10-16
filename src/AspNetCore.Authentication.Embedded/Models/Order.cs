using System.ComponentModel.DataAnnotations;

namespace AspNetCore.Authentication.Embedded.Models
{
    public class OrderRequest
    {
        [Required]
        public string Description { get; set; }

        public int Quantity { get; set; }

        public PaymenthMethod PaymentMethod { get; set; }
    }

    public class Order : OrderRequest
    {
        public Order()
        {
        }

        public Order(string email, OrderRequest orderRequest)
        {
            Description = orderRequest.Description;
            Quantity = orderRequest.Quantity;
            PaymentMethod = orderRequest.PaymentMethod;
            OrderedBy = email ?? "default@no-reply.com";
        }

        public int OrderId { get; set; }

        public string OrderedBy { get; set; }
    }

    public enum PaymenthMethod
    {
        CreditCard,
        Invoice
    }
}
