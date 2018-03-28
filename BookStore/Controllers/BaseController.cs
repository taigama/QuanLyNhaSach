using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BookStore.Controllers
{
    public class BaseController : Controller
    {

        /// <summary>
        /// for short-writing
        /// </summary>
        public JsonResult JResponse(bool isSuccess = false, string message = "")
        {
            return Json(new { success = isSuccess, text = message }, JsonRequestBehavior.AllowGet);
        }
    }
}