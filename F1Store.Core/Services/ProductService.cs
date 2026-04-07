using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

using F1Store.Core.Contracts;
using F1Store.Infrastructure.Data;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public bool Create(string name, int teamId, int categoryId, string picture,
            string? picture2, string? picture3, string? picture4, string? picture5,
            string description, int quantity, decimal price, decimal discount)
        {
            Product item = new Product
            {
                ProductName = name,
                Team = _context.Teams.Find(teamId),
                Category = _context.Categories.Find(categoryId),
                Picture = picture,
                Picture2 = picture2,
                Picture3 = picture3,
                Picture4 = picture4,
                Picture5 = picture5,
                Description = description,
                Quantity = quantity,
                Price = price,
                Discount = discount
            };

            _context.Products.Add(item);
            return _context.SaveChanges() != 0;
        }

        public Product GetProductById(int productId)
        {
            return _context.Products
                .Include(p => p.Team)
                .Include(p => p.Category)
                .FirstOrDefault(p => p.Id == productId);
        }

        public List<Product> GetProducts()
        {
            return _context.Products
                .Include(p => p.Team)
                .Include(p => p.Category)
                .ToList();
        }

        public List<Product> GetProducts(string searchStringCategoryName, string searchStringTeamName)
        {
            var query = _context.Products
                .Include(p => p.Team)
                .Include(p => p.Category)
                .AsQueryable();

            if (!String.IsNullOrEmpty(searchStringCategoryName))
            {
                query = query.Where(x => x.Category.CategoryName.ToLower().Contains(searchStringCategoryName.ToLower()));
            }

            if (!String.IsNullOrEmpty(searchStringTeamName))
            {
                query = query.Where(x => x.Team.TeamName.ToLower().Contains(searchStringTeamName.ToLower()));
            }

            return query.ToList();
        }

        public bool RemoveById(int productId)
        {
            var product = GetProductById(productId);
            if (product == null)
            {
                return false;
            }

            _context.Remove(product);
            return _context.SaveChanges() != 0;
        }

        public bool Update(int productId, string name, int teamId, int categoryId, string picture,
            string? picture2, string? picture3, string? picture4, string? picture5,
            string description, int quantity, decimal price, decimal discount)
        {
            var product = GetProductById(productId);
            if (product == null)
            {
                return false;
            }

            product.ProductName = name;
            product.Team = _context.Teams.Find(teamId);
            product.Category = _context.Categories.Find(categoryId);
            product.Picture = picture;
            product.Picture2 = picture2;
            product.Picture3 = picture3;
            product.Picture4 = picture4;
            product.Picture5 = picture5;
            product.Description = description;
            product.Quantity = quantity;
            product.Price = price;
            product.Discount = discount;

            _context.Update(product);
            return _context.SaveChanges() != 0;
        }
    }
}