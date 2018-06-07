using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    public class ExampleModel
    {
        public int id;
        public string content; 
    }

    public class ExampleController : Controller
    {
        // GET: Example
        public ActionResult Index()
        {
            ExampleModel data = new ExampleModel
            {
                id = 123,
                content = "pass data thành công rồi!"
            };
            return View(data);
        }
    }
}