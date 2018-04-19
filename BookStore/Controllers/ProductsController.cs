using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Data;
using Newtonsoft.Json;
using BookStore.Models;

namespace BookStore.Controllers
{
    /// <summary>
    /// A class for showing products
    /// </summary>
    public class ProductsController : Controller
    {
        BookStoreContext db = new BookStoreContext();

        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// A list products by category [view]
        /// </summary>
        public ActionResult Category(int? id)
        {// trang xem danh sách các sản phẩm theo danh mục

            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return RedirectToAction("Error"
                    , new {
                    errorCode = "Lỗi 404."
                            ,
                    errorDetail = "Không tìm thấy danh mục này"
                });
            }

            // dữ liệu cho trang
            ViewData["CategoryName"] = category.Name;
            ViewData["Products"] = category.Products.ToList();

            //The side menu data
            ViewData["Categories"] = db.Categories.OrderByDescending(cate => cate.NumberOfProducts).Take(5).ToList();
            ViewData["Authors"] = db.Authors.OrderByDescending(au => au.NumberOfBooks).Take(5).ToList();
            return View();
        }

        /// <summary>
        /// A product [view]
        /// </summary>
        [HttpGet]
        public ActionResult Single(int? id)
        {// trang xem 1 sản phẩm

            if(id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var product = db.Products.Find(id);
            if(product == null)
            {
                return RedirectToAction("Error", new
                {
                    errorCode = "Lỗi 404."
                            ,
                    errorDetail = "Không tìm thấy sản phẩm này"
                });
            }

            //The side menu data
            ViewData["Categories"] = db.Categories.OrderByDescending(cate => cate.NumberOfProducts).Take(5).ToList();
            ViewData["Authors"] = db.Authors.OrderByDescending(au => au.NumberOfBooks).Take(5).ToList();

            // dữ liệu cho trang
            return View(product);
        }

        /// <summary>
        ///  [json]
        /// </summary>
        [HttpGet]
        public ActionResult DropDownProduct()
        {// tạo danh sách key/value tên sản phẩm (dùng trong dropdownlist)
            var dict = db.Products.ToDictionary(pro => pro.ID, pro => pro.Name);

            var list = dict.ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// get a product data [json]
        /// </summary>
        /// <param name="depth">detail level (data depth included to json)</param>
        [HttpGet]
        public ActionResult GetProduct(int id, int depth)
        {// trả về thông tin 1 sản phẩm bằng json

            var productYour = db.Products.Find(id);
            if (productYour == null)
            {
                return null;
            }

            var result = new JsonNetResult
            {
                Data = productYour,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = depth
                }
            };
            return result;
        }

        /// <summary>
        /// get list products  [json]
        /// </summary>
        [HttpGet]
        public ActionResult GetProducts()
        {
            var result = new JsonNetResult
            {
                Data = db.Products.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };

            return result;
        }


        [HttpPost]
        public ActionResult FilterProductByCategory(int categoryId)
        {
            IQueryable<Product> products;
            if (categoryId == -1)
            {
                products = db.Products;
            }
            else
            {
                products = from child in db.Products
                           where (child.CategoryId == categoryId)
                           select child;
            }

            var result = new JsonNetResult
            {
                Data = products.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };

            return result;
        }

        [HttpPost]
        public ActionResult FilterProductByPublisher(int publisherId)
        {
            IQueryable<Product> products;
            if (publisherId == -1)
            {
                products = db.Products;
            }
            else
            {
                products = from child in db.Products
                           where (child.PublisherId == publisherId)
                           select child;
            }

            var result = new JsonNetResult
            {
                Data = products.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };

            return result;
        }

        [HttpGet]
        public ActionResult DropdownCategory()
        {
            var dict = db.Categories.ToDictionary(cate => cate.ID, cate => cate.Name);

            var list = dict.ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteProduct(int id)
        {
            ResultWeb result = new ResultWeb();
            if (ModelState.IsValid)
            {
                Product product = db.Products.Find(id);

                if (product == null)
                {
                    result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                int cookieId;
                if (Request.Cookies["product_id"] != null)
                {
                    cookieId = int.Parse(Request.Cookies["product_id"].Value);
                    if (cookieId == id)
                    {
                        Response.Cookies["product_id"].Expires = DateTime.Now.AddDays(-1);
                    }
                }

                db.Products.Remove(product);
                db.SaveChanges();

                result.Type = ResultWeb.ResultType.OK_DELETE;
            }
            else
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropdownAuthor()
        {
            var dict = db.Authors.ToDictionary(a => a.ID, a => a.Name);
            var list = dict.ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Error(string errorCode, string errorDetail)
        {
            ViewBag.ErrorCode = errorCode;
            ViewBag.ErrorDetail = errorDetail;
            return View();
        }
    }
}