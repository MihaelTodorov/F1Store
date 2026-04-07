using System;
using System.Collections.Generic;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contracts
{
    public interface IProductService
    {
        bool Create(string name, int teamId, int categoryId, string picture,
                    string? picture2, string? picture3, string? picture4, string? picture5,
                    string description, int quantity, decimal price, decimal discount);

        bool Update(int productId, string name, int teamId, int categoryId, string picture,
                    string? picture2, string? picture3, string? picture4, string? picture5,
                    string description, int quantity, decimal price, decimal discount);

        List<Product> GetProducts();
        Product GetProductById(int productId);
        bool RemoveById(int productId);
        List<Product> GetProducts(string searchStringCategoryName, string searchStringTeamName);
    }
}