using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MVC_WebApplication.BL.Models;

namespace MVC_WebApplication.Models
{
    public class ProductViewModel
    {
        public List<ProductEntity> Products { get; set; }

        public Product NewProduct { get; set; }
    }
}