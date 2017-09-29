using System.ComponentModel.DataAnnotations;

namespace MVC_WebApplication.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public double Price { get; set; }

        [Required]
        [RegularExpression("([1-9][0-9]*)")]
        public int Number { get; set; }

        public string ImageSource { get; set; }
    }
}