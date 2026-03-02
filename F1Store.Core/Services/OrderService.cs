using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Infrastructure.Data;

namespace F1Store.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly IProductService _productService;

        public OrderService(ApplicationDbContext context, IProductService productService)
        {
            _context = context;
            _productService = productService;
        }

        public bool Create(int productId, string userId, int quantity)
        {
            // намираме продукта по неговото id
            var product = this._context.Products.SingleOrDefault(x => x.Id == productId);

            // проверяваме дали има такъв продукт
            if (product == null)
            {
                return false;
            }

            // създаване на поръчка
            Order item = new Order
            {
                OrderDate = DateTime.Now,
                ProductId = productId,
                UserId = userId,
                Quantity = quantity,
                Price = product.Price,
                Discount = product.Discount
            };

            // намаляване на количеството на продукта
            product.Quantity -= quantity;

            // отразяване на промените в колекциите
            this._context.Products.Update(product);
            this._context.Orders.Add(item);

            // запис на промените в БД
            return _context.SaveChanges() != 0;
        }

        public Order GetOrderById(int orderId)
        {
            throw new NotImplementedException();
        }

        public List<Order> GetOrders()
        {
            return _context.Orders.OrderByDescending(x => x.OrderDate).ToList();
        }

        public List<Order> GetOrdersByUser(string userId)
        {
            return _context.Orders.Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OrderDate)
                .ToList();
        }

        public bool RemoveById(int orderId)
        {
            throw new NotImplementedException();
        }

        public bool Update(int orderId, int productId, string userId, int quantity)
        {
            throw new NotImplementedException();
        }
    }

}