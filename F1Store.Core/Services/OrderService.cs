using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data.Domain;
using F1Store.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            if (quantity < 1)
            {
                return false;
            }

            var product = _context.Products.SingleOrDefault(x => x.Id == productId);

            if (product == null || product.Quantity < quantity)
            {
                return false;
            }

            Order item = new Order
            {
                OrderGroupId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                ProductId = productId,
                UserId = userId,
                Quantity = quantity,
                Price = product.Price,
                Discount = product.Discount
            };

            product.Quantity -= quantity;

            _context.Products.Update(product);
            _context.Orders.Add(item);

            return _context.SaveChanges() > 0;
        }

        public Order GetOrderById(int orderId)
        {
            return _context.Orders.FirstOrDefault(o => o.Id == orderId);
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

        public bool Delete(int orderId)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return false;
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return true;
        }

        public bool CreateFromCart(string userId)
        {
            var cartItems = _context.CartItems.Where(x => x.UserId == userId).ToList();
            if (cartItems.Count == 0) return false;

            // Validate stock first (no partial checkouts)
            foreach (var ci in cartItems)
            {
                var product = _context.Products.SingleOrDefault(p => p.Id == ci.ProductId);
                if (product == null) return false;
                if (product.Quantity < ci.Quantity) return false;
            }

            var groupId = Guid.NewGuid();
            var now = DateTime.Now;

            foreach (var ci in cartItems)
            {
                var product = _context.Products.Single(p => p.Id == ci.ProductId);

                var order = new Order
                {
                    OrderGroupId = groupId,
                    OrderDate = now,
                    ProductId = ci.ProductId,
                    UserId = userId,
                    Quantity = ci.Quantity,
                    Price = product.Price,
                    Discount = product.Discount
                };

                product.Quantity -= ci.Quantity;

                _context.Orders.Add(order);
                _context.Products.Update(product);
            }

            _context.CartItems.RemoveRange(cartItems);

            return _context.SaveChanges() != 0;
        }

        public Guid? GetLatestOrderGroupIdByUser(string userId)
        {
            var latestOrder = _context.Orders
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.OrderDate)
                .ThenByDescending(x => x.Id)
                .FirstOrDefault();

            return latestOrder?.OrderGroupId;
        }

        public List<Order> GetOrdersByGroupId(Guid orderGroupId, string userId)
        {
            return _context.Orders
                .Where(x => x.OrderGroupId == orderGroupId && x.UserId == userId)
                .OrderBy(x => x.Id)
                .ToList();
        }

        public bool UserHasOrders(string userId)
        {
            return _context.Orders.Any(o => o.UserId == userId);
        }

        public (bool Success, Guid? OrderGroupId, List<(int ProductId, string ProductName, int Requested, int Available, string Action)> Issues)
        TryCheckoutFromCart(string userId)
        {
            var issues = new List<(int ProductId, string ProductName, int Requested, int Available, string Action)>();

            var cartItems = _context.CartItems
                .Where(x => x.UserId == userId)
                .ToList();

            if (cartItems.Count == 0)
            {
                return (false, null, issues);
            }

            // Validate + auto-adjust cart
            foreach (var ci in cartItems.ToList())
            {
                var product = _context.Products.FirstOrDefault(p => p.Id == ci.ProductId);

                if (product == null)
                {
                    issues.Add((ci.ProductId, "Unknown product", ci.Quantity, 0, "Removed"));
                    _context.CartItems.Remove(ci);
                    continue;
                }

                if (product.Quantity <= 0)
                {
                    issues.Add((product.Id, product.ProductName, ci.Quantity, 0, "Removed"));
                    _context.CartItems.Remove(ci);
                    continue;
                }

                if (ci.Quantity > product.Quantity)
                {
                    issues.Add((product.Id, product.ProductName, ci.Quantity, product.Quantity, "Adjusted"));
                    ci.Quantity = product.Quantity;
                    _context.CartItems.Update(ci);
                }
            }

            // If any issues happened: save adjustments and stop (so user can review cart)
            if (issues.Count > 0)
            {
                _context.SaveChanges();
                return (false, null, issues);
            }

            // Proceed with checkout (no issues)
            var groupId = Guid.NewGuid();
            var now = DateTime.Now;

            foreach (var ci in cartItems)
            {
                var product = _context.Products.First(p => p.Id == ci.ProductId);

                var order = new Order
                {
                    OrderGroupId = groupId,
                    OrderDate = now,
                    ProductId = ci.ProductId,
                    UserId = userId,
                    Quantity = ci.Quantity,
                    Price = product.Price,
                    Discount = product.Discount
                };

                product.Quantity -= ci.Quantity;

                _context.Orders.Add(order);
                _context.Products.Update(product);
            }

            _context.CartItems.RemoveRange(cartItems);
            var saved = _context.SaveChanges() > 0;

            return (saved, saved ? groupId : null, issues);
        }
    }
}
