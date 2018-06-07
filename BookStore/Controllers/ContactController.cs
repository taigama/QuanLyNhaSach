using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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

        public static bool SendEmail(string SentTo, string Text)
        {
            MailMessage msg = new MailMessage();

            msg.From = new MailAddress("TestEmail@gmail.com");
            msg.To.Add(SentTo);
            msg.Subject = "subject";
            msg.Body = Text;
            msg.Priority = MailPriority.High;
            msg.IsBodyHtml = true;

            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);



            client.UseDefaultCredentials = false;
            client.EnableSsl = false;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            //client.EnableSsl = true;

            try
            {
                client.Send(msg);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}