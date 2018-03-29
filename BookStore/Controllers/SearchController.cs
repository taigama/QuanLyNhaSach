using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using BookStore.Data;
using Newtonsoft.Json;

namespace BookStore.Controllers
{
    public class SearchController : Controller
    {
        BookStoreContext db = new BookStoreContext();

        // GET: Search
        /// <summary>
        /// A search page [view]
        /// </summary>
        public ActionResult Index(string key, int? costBegin, int? costEnd)
        {// từ trang khác, chuyển trang qua Search

            IQueryable<Product> products;
            if (!string.IsNullOrEmpty(key))
            {
                products = db.Products.Where(p => p.Name.Contains(key));
            }
            else
            {
                products = db.Products;
            }

            if (costBegin != null)
            {
                products = products.Where(p => p.CostPrice >= costBegin);
            }
            if (costEnd != null)
            {
                products = products.Where(p => p.CostPrice <= costEnd);
            }

            // keyword bỏ vào search box (cho người dùng biết họ đã nhập cái gì)
            ViewData["key"] = key;

            // dữ liệu đổ vào Side search
            ViewData["Categories"] = db.Categories.ToList();
            ViewData["Authors"] = db.Authors.ToList();

            // sản phẩm của kết quả search từ trang khác
            return View(products.ToList());
        }

        /// <summary>
        /// search data [json]
        /// </summary>
        [HttpGet]
        public ActionResult Search(
            string key
            , int? authorId, int? categoryId
            , int? costBegin, int? costEnd)
        {// search từ trang search, gọi lệnh search cùng các filter parameter


            if(costBegin!= null)
            {
                costBegin *= 1000;
            }
            if (costEnd != null)
            {
                costEnd *= 1000;
            }
            IQueryable<Product> products;
            if (!string.IsNullOrEmpty(key))
            {
                products = db.Products.Where(p => p.Name.Contains(key));
            }
            else
            {
                products = db.Products;
            }

            if (authorId != null)
            {
                List<Product> tmp = products.ToList();
                List<Product> tmp2 = new List<Product>(tmp);
                foreach(var singleProduct in tmp2)
                {
                    var details = singleProduct.ProductDetails;
                    if (details != null)
                    {
                        var found = details.Where(pd => pd.AuthorId == authorId);
                        if (found == null || found.Count() == 0)
                        {
                            tmp.Remove(singleProduct);
                        }
                    }
                }
                if (tmp.Count == 0)
                    return Json(tmp, JsonRequestBehavior.AllowGet);

                try
                {
                    products = tmp.AsQueryable();
                }
                catch (Exception)
                {
                    return Json("lỗi không xác định", JsonRequestBehavior.AllowGet);
                }
            }

            if(categoryId != null)
            {
                products = products.Where(p => p.CategoryId == categoryId);

                if (products.Count() == 0)
                    return Json(new List<Product>(), JsonRequestBehavior.AllowGet);
            }

            if (costBegin != null)
            {
                products = products.Where(p => p.Price >= costBegin);
                if (products.Count() == 0)
                    return Json(new List<Product>(), JsonRequestBehavior.AllowGet);
            }
            if (costEnd != null)
            {
                products = products.Where(p => p.Price <= costEnd);
                if (products.Count() == 0)
                    return Json(new List<Product>(), JsonRequestBehavior.AllowGet);
            }

            // trả về json dữ liệu sau khi filter
            var result = new JsonNetResult
            {
                Data = products.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 3// product, productdetail, author
                }
            };
            return result;
        }
    }


}