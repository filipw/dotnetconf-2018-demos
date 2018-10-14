using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCore.Authentication.Embedded.Models
{
    public class OrderService
    {
        private readonly List<Order> _orders = new List<Order>
        {
            new Order { OrderId = 1, Description = "Teddy Bears", Quantity = 5, PaymentMethod = PaymenthMethod.CreditCard, OrderedBy = "filip@w.com" },
            new Order { OrderId = 2, Description = "Unicorns", Quantity = 3, PaymentMethod = PaymenthMethod.CreditCard, OrderedBy = "filip@w.com" },
            new Order { OrderId = 3, Description = "Lollipops", Quantity = 123, PaymentMethod = PaymenthMethod.CreditCard, OrderedBy = "filip@w.com" },
            new Order { OrderId = 4, Description = "Haggis", Quantity = 1, PaymentMethod = PaymenthMethod.Invoice, OrderedBy = "jose@bluejays.com" },
            new Order { OrderId = 5, Description = "Baseballs", Quantity = 10, PaymentMethod = PaymenthMethod.Invoice, OrderedBy = "jose@bluejays.com" },
        };

        public Task<IEnumerable<Order>> GetAll()
        {
            return Task.FromResult(_orders.AsEnumerable());
        }

        public Task<Order> Get(int id)
        {
            return Task.FromResult(_orders.FirstOrDefault(x => x.OrderId == id));
        }

        public Task<int> Add(Order contact)
        {
            var newId = (_orders.LastOrDefault()?.OrderId ?? 0) + 1;
            contact.OrderId = newId;
            _orders.Add(contact);
            return Task.FromResult(newId);
        }

        public async Task Delete(int id)
        {
            var contact = await Get(id).ConfigureAwait(false);
            if (contact == null)
            {
                throw new InvalidOperationException(string.Format("Contact with id '{0}' does not exists", id));
            }

            _orders.Remove(contact);
        }
    }
}
