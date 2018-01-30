using BookStore.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Controllers
{

    public class BlogController : Controller
    {
        private BookStoreContext db = new BookStoreContext();

        // GET: Blog
        public ActionResult Index()
        {
            ViewData["Categories"] = db.Categories.ToList();
            ViewData["Authors"] = db.Authors.ToList();
            return View();
        }
    }
}