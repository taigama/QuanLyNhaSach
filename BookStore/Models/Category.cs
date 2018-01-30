using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    public class Category : BaseEntity
    {
        [Display(Name = "Danh mục")]
        public string Name { get; set; }

        public string Description { get; set; }

        public int NumberOfProducts { get; set; }

        [Display(Name = "Loại")]
        public ProductType Type { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}