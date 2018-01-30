using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookStore.Data;
using BookStore.Helpers;

namespace BookStore.Models
{
    public class Pair
    {
        public int Key { get; set; }
        public string Value { get; set; }
    }

    public class ViewModel
    {
        public bool Success { get; set; }
    }

    public class ProductVM : ViewModel
    {
        public List<Pair> DropdownType { get; set; }

        public List<Pair> DropdownCover { get; set; }

        public List<Pair> DropdownPublisher { get; set; }

        public List<Pair> DropdownCategory { get; set; }

        public List<Pair> DropdownUnit { get; set; }

        public List<Pair> DropdownStatus { get; set; }

        public ProductDetail[] Details { get; set; }

        public Product Product { get; set; }

        public ProductType ProductType { get; set; }

        public static ProductVM GenerateProductVM(BookStoreContext db, bool success = false)
        {
            ProductVM model = new ProductVM
            {
                DropdownPublisher = HtmlExtensions.GenerateDropdownPublisher(db),
                DropdownType = HtmlExtensions.GenerateDropdownEnum<ProductType>(),
                DropdownUnit = HtmlExtensions.GenerateDropdownEnum<UnitType>(),
                DropdownCover = HtmlExtensions.GenerateDropdownEnum<CoverType>(),
                Success = success,
                Product = new Product(),
            };
            model.DropdownCategory = HtmlExtensions.GenerateDropdownProductCategory(db, model.ProductType);
            return model;
        }

        public static ProductVM GenerateProductVM(BookStoreContext db, int id, bool success = false)
        {
            ProductVM model = new ProductVM
            {
                DropdownPublisher = HtmlExtensions.GenerateDropdownPublisher(db),
                DropdownType = HtmlExtensions.GenerateDropdownEnum<ProductType>(),
                DropdownUnit = HtmlExtensions.GenerateDropdownEnum<UnitType>(),
                DropdownCover = HtmlExtensions.GenerateDropdownEnum<CoverType>(),
                Success = success,
                Product = db.Products.Find(id),
            };
            model.ProductType = model.Product.Category.Type;
            model.DropdownCategory = HtmlExtensions.GenerateDropdownProductCategory(db, model.ProductType);
            return model;
        }

        public static ProductVM GenerateProductVM(BookStoreContext db, int? id, bool success = false)
        {
            ProductVM model = new ProductVM
            {
                DropdownPublisher = HtmlExtensions.GenerateDropdownPublisher(db),
                DropdownType = HtmlExtensions.GenerateDropdownEnum<ProductType>(),
                DropdownUnit = HtmlExtensions.GenerateDropdownEnum<UnitType>(),
                DropdownCover = HtmlExtensions.GenerateDropdownEnum<CoverType>(),
                Success = success,
                Product = db.Products.Find(id),
            };
            model.ProductType = model.Product.Category.Type;
            model.DropdownCategory = HtmlExtensions.GenerateDropdownProductCategory(db, model.ProductType);
            return model;
        }
    }
}