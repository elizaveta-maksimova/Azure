using System.Collections.Generic;
using System.IO;
using System.Linq;
using MVC_WebApplication.BL.Models;
using MVC_WebApplication.Data.DataAccess;
using MVC_WebApplication.Data.Model;

namespace MVC_WebApplication.BL.Account
{
    public class ProductService
    {
        private readonly ProductDataAccess _productDataAccess;
        private readonly StorageDataAccess _storageDataAccess;

        public ProductService()
        {
            _productDataAccess = new ProductDataAccess();
            _storageDataAccess = new StorageDataAccess();
        }

        public List<ProductEntity> GetAllProducts()
        {
            List<Product> products = _productDataAccess.GetAllProducts();
            return products.Select(p => new ProductEntity(p) { ImageName = _storageDataAccess.GetBlobSas(p.BlobPath) }).ToList();
        }

        public void InsertProduct(ProductEntity product, byte[] file, string fileName)
        {
            _storageDataAccess.Upload(file, fileName);

            if (product != null)
            {
                _productDataAccess.InsertProduct(
               new Product
               {
                   BlobPath = fileName,
                   Description = product.Description,
                   Price = product.Price,
                   Title = product.Title
               });
            }
           
        }
    }
}
