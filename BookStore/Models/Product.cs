using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models
{
    

    public class Product : BaseEntity
    {
        public Product()
        {
        }

        [Display(Name = "Tên")]
        public string Name { get; set; }

        public string Description { get; set; }

        public UnitType Unit { get; set; }

        // Loại bìa
        public CoverType Cover { get; set; }

        // Cân nặng
        public double Weight { get; set; }

        // Chiều rộng bìa
        public double Width { get; set; }

        // Chiều dài bìa
        public double Height { get; set; }

        public int PageCount { get; set; }

        [Display(Name = "Tồn kho")]
        public int InStock { get; set; }

        public double CostPrice { get; set; }

        public double Price { get; set; }

        public DateTime PublishedTime { get; set; }

        public int PublisherId { get; set; }

        public int CategoryId { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; }

        public string AvatarImage { get; set; }

        public virtual Publisher Publisher { get; set; }

        public virtual Category Category { get; set; }

        public virtual ICollection<ProductDetail> ProductDetails { get; set; }

        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}