using BookStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    public class Home1Controller : Controller
    {
        private BookStoreContext db = new BookStoreContext();

        /// <summary>
        /// Home page [view]
        /// </summary>
        public ActionResult Index()
        {
            //The side menu data
            ViewData["Categories"] = db.Categories.Take(5).ToList();
            ViewData["Authors"] = db.Authors.Take(5).ToList();

            // main page data
            ViewData["Products"] = db.Products.ToList();

            return View();
        }

        /// <summary>
        /// An author page [view]
        /// </summary>
        [HttpGet]
        public ActionResult Author(int? id)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var author = db.Authors.Find(id);
            if (author == null)
            {
                return RedirectToAction("Error", new
                {
                    errorCode = "Lỗi 404."
                            ,
                    errorDetail = "Không tìm thấy tác giả này"
                });
            }

            //The side menu data
            ViewData["Categories"] = db.Categories.ToList();
            ViewData["Authors"] = db.Authors.ToList();

            // main page data
            return View(author);
        }

        /// <summary>
        /// [json]
        /// </summary>
        [HttpGet]
        public ActionResult DropDownUser()
        {// tạo dữ liệu key/value để đổ vào dropdownlist


            var dictUser = db.Users.ToDictionary(us => us.ID, us => us.FirstName);

            List<KeyValuePair<int, string>> result = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(0, "Hệ thống")
            };
            foreach (var child in dictUser)
            {
                result.Add(child);
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Error page for re-direct [view]
        /// </summary>
        [HttpGet]
        public ActionResult Error(string errorCode, string errorDetail)
        {// trang khác redirect qua đây

            ViewBag.ErrorCode = errorCode;
            ViewBag.ErrorDetail = errorDetail;
            return View();
        }
    }
}