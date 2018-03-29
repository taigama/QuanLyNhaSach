using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    /// <summary>
    /// just a Page that render info
    /// </summary>
    public class ContactController : Controller
    {
        // GET: Contact
        public ActionResult Index()
        {
            // không có side menu => không cần dữ liệu cho side menu
            return View();
        }
    }
}