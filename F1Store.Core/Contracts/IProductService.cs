using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using F1Store.Infrastructure.Data.Domain;

namespace F1Store.Core.Contracts
{
    public interface IProductService
    {
        public interface IProductService
        {
            bool Create(string name, int teamId, int categoryId, string picture, string discription, int quantity, decimal price, decimal discount);
            bool Update(int productId, string name, int teamId, int categoryId, string picture, string discription, int quantity, decimal price, decimal discount);
            List<Product> GetProducts();
            Product GetProductById(int productId);
            bool RemoveById(int dogproductId);
            List<Product> GetProducts(string searchStringCategoryName, string searchStringTeamName);
        }

    }
}
