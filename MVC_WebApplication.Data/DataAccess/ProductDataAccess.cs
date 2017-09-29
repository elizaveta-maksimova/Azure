using System.Collections.Generic;
using System.Linq;
using MVC_WebApplication.Data.Model;

namespace MVC_WebApplication.Data.DataAccess
{
    public class ProductDataAccess
    {
        public List<Product> GetAllProducts()
        {
            List<Product> products;

            using (var dbContext = new ProductModel())
            {
                products = dbContext.Products.ToList();
            }

            return products;
        }

        public void InsertProduct(Product product)
        {
            using (var dbContext = new ProductModel())
            {
                dbContext.Products.Add(product);
                dbContext.SaveChanges();
            }
        }
    }
}
