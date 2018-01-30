using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using Newtonsoft.Json;
using BookStore.Data;
using System.Data.Entity;
using System.Web.Security;

namespace BookStore.Controllers
{

    public class OrderController : Controller
    {
        BookStore.Data.BookStoreContext db = new Data.BookStoreContext();
        JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };

        [HttpGet]
        public ActionResult Index()
        {
            return RedirectToAction("ListOrder", "Staff");
        }

        [HttpGet]
        public ActionResult New()
        {
            if (Request.Cookies["order_id"] != null)
            {
                Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);
            }

            return RedirectToAction("Cashier", "Staff");
        }


        #region list of orders
        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult GetOrders()
        {
            IQueryable<Order> orders;
            if (!User.IsInRole("Admin"))
            {
                //orders = db.Orders.Where(o => o.Status == OrderStatus.Pending);

                int userId = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault().ID;

                orders = (from childs in db.Orders
                               where ((childs.Status == OrderStatus.Pending || childs.Status == OrderStatus.Packing) && (childs.AnonymousUserId != null))
                               || ((childs.UserId != null) && (childs.UserId == userId) && (childs.Status == OrderStatus.New))
                               select childs);
            }
            else
            {
                orders = db.Orders;
            }
            
            var result = new JsonNetResult
            {
                Data = orders.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 2
                }
            };

            return result;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult FilterOrders(int userId, int state)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền"
                        });
            }

            IQueryable<Order> orders;

            orders = db.Orders;
            if(orders == null)// no orders
            {
                return null;
            }

            if(userId != -1)
            {
                if(userId == 0)// filtered by "no user"
                {
                    orders = orders.Where(od => od.UserId == null);
                }
                else
                {
                    orders = orders.Where(od => od.UserId == userId);
                }
            }

            if(state == -2)
            {
                orders = (from childs in orders
                          where ((childs.Status != OrderStatus.Canceled)
                          && (childs.Status != OrderStatus.Completed)
                          && (childs.AnonymousUserId == null || childs.Status != OrderStatus.New))
                          select childs);
            }
            else if(state != -1)
            {
                orders = orders.Where(od => od.Status == (OrderStatus)state);
            }


            //var converted = JsonConvert.SerializeObject(orders, Formatting.Indented, serializerSettings);
            //return Content(converted, "json");

            //var result = new JsonNetResult
            //{
            //    Data = orders.ToList(),
            //    JsonRequestBehavior = JsonRequestBehavior.AllowGet,
            //    Settings = {
            //ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            //MaxDepth = 2
            //    }
            //};

            //return result;
            return PartialView("_ListOrder", orders);
        }
        #endregion

        #region single order
        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ViewOrder(int? Id)
        {
            if(Id == null)
            {
                return RedirectToAction("ListOrder", "Staff");
            }

            Order order = db.Orders.Find(Id);
            if(order == null)
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không tìm thấy đơn hàng với id: " + Id.ToString()
                        });
            }
            if(order.Status == OrderStatus.New && order.AnonymousUserId == null)
            {
                return RedirectToAction("Cashier", "Staff", new {Id });
            }

            Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            

            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                if ((order.AnonymousUserId != null && (order.Status != OrderStatus.New && order.Status != OrderStatus.Completed && order.Status != OrderStatus.Canceled))
                          || (order.AnonymousUserId == null && order.Status == OrderStatus.New && order.UserId == user.ID))
                {
                    ViewBag.IsAdmin = false;
                }
                else
                    return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền"
                        });
            }

            ViewBag.User = user;
            return View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ViewOrder(int? Id, OrderStatus? stateCurrent)
        {
            if(Id == null || stateCurrent == null)
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 400."
                            ,
                            errorDetail = "Yêu cầu của bạn không hợp lệ"
                        });
            }

            Models.Order order = db.Orders.Find(Id);
            if(order == null)
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không tìm thấy đơn hàng với id: " + Id.ToString()
                        });
            }
            
            if (stateCurrent == OrderStatus.New || stateCurrent == OrderStatus.Canceled)
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không được phép làm điều này"
                        });
            }

            

            switch (order.Status)
            {
                case OrderStatus.Packing:
                    {
                        order.Status = OrderStatus.Delivering;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;
                case OrderStatus.Delivering:
                    {
                        order.DeliveryDate = DateTime.Today;
                        order.Status = OrderStatus.Completed;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;
                case OrderStatus.Pending:
                    {
                        SubProduct(order);
                        order.Status = OrderStatus.Packing;
                        db.Entry(order).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    break;
            }

            order = db.Orders.Where(o => o.Status == stateCurrent).FirstOrDefault();
            if (order == null)
            {
                return RedirectToAction("ListOrder", "Staff");
            }

            Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            ViewBag.User = user;

            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }

            return View(order);
        }


        // /Order/DeleteOrder/6969 / deprecated
        // /Order/ArchiveOrder/6969
        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ArchiveOrder(int Id)
        {
            ResultWeb result = new ResultWeb();


            if (!User.IsInRole("Admin"))
            {
                result.Type = ResultWeb.ResultType.ACCESS_VIOLENCE;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            if (ModelState.IsValid)
            {
                //var detailsBelongTheOrder = db.OrderDetails.Where(od => od.OrderId == Id);
                //db.OrderDetails.RemoveRange(detailsBelongTheOrder);
                //db.SaveChanges();

                Order order = db.Orders.Find(Id);

                if(order == null)
                {
                    result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                int cookieId;
                if (Request.Cookies["order_id"] != null)
                {
                    cookieId = int.Parse(Request.Cookies["order_id"].Value);
                    if(cookieId == Id)
                    {
                        Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);
                    }
                }

                //db.Orders.Remove(order);
                order.Status = OrderStatus.Canceled;
                db.Entry(order).State = EntityState.Modified;
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
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult GetOrder(int id)
        {
            var order = db.Orders.Find(id);

            //var converted = JsonConvert.SerializeObject(order, Formatting.None, serializerSettings);
            //return Content(converted, "json");


            var result = new JsonNetResult
            {
                Data = order,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 3
                }
            };

            return result;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult DeleteOrderDetail(int id)
        {
            ResultWeb result = new ResultWeb();
            if (ModelState.IsValid)
            {
                var detail = db.OrderDetails.Find(id);

                if (detail == null)
                {
                    result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                int orderId = detail.OrderId;

                db.OrderDetails.Remove(detail);
                db.SaveChanges();

                RecalculateOrderCost(orderId);

                result.Type = ResultWeb.ResultType.OK_DELETE;
            }
            else
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddLine(int orderId, int proId, int count)
        {
            ResultWeb result = new ResultWeb();
            Product product = db.Products.Find(proId);
            if(product == null)
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            
            var details = db.OrderDetails.Where(od => od.OrderId == orderId);

            OrderDetail line = details.Where(od => od.ProductId == proId).FirstOrDefault();
            if(line == null)
            {
                line = new OrderDetail
                {
                    OrderId = orderId,
                    ProductId = proId
                };

                if(count > product.InStock)
                {
                    line.Quantity = product.InStock;
                    result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                }
                else
                {
                    line.Quantity = count;
                    result.Type = ResultWeb.ResultType.OK_ADD;
                }                

                if (line.Quantity > 0)
                {
                    line.TotalAmount = product.Price * line.Quantity;
                    db.OrderDetails.Add(line);
                    db.SaveChanges();
                }
            }
            else
            {
                line.Quantity += count;
                if (line.Quantity > product.InStock)
                {
                    line.Quantity = product.InStock;
                    result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                }
                else
                {
                    result.Type = ResultWeb.ResultType.OK_ADD;
                }

                if (line.Quantity > 0)
                {
                    line.TotalAmount = product.Price * line.Quantity;
                    db.Entry(line).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    db.OrderDetails.Remove(line);
                    db.SaveChanges();
                }
            }
            
            RecalculateOrderCost(orderId);
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult UpdateLine(int detailId, int proId, int count)
        {
            ResultWeb result = new ResultWeb();
            Product product = db.Products.Find(proId);
            if (product == null)
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            OrderDetail detail = db.OrderDetails.Find(detailId);
            if (detail == null)
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            detail.ProductId = proId;
            if (count > product.InStock)
            {
                detail.Quantity = product.InStock;
                result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
            }
            else
            {
                detail.Quantity = count;
                result.Type = ResultWeb.ResultType.OK_ADD;
            }

            if (detail.Quantity > 0)
            {
                detail.TotalAmount = product.Price * detail.Quantity;

                db.Entry(detail).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                db.OrderDetails.Remove(detail);
                db.SaveChanges();
            }
            PreventDetailDuplicate(detail.OrderId, proId);

            RecalculateOrderCost(detail.OrderId);

            result.Type = ResultWeb.ResultType.OK_UPDATE;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// prevent an order has duplicate product lines of an product
        /// <para> when you update a line, change its product, there may have duplicate </para>
        /// </summary>
        private void PreventDetailDuplicate(int orderId, int proId)
        {
            Product product = db.Products.Find(proId);
            if(product == null)
            {
                throw new NullReferenceException();
            }

            var details = (from childs in db.OrderDetails
                          where (childs.OrderId == orderId) && (childs.ProductId == proId)
                          select childs).ToList();

            if(details.Count > 1)// yes, there are duplicate product line in the order
            {
                OrderDetail lineFirst = details[0];
                details.RemoveAt(0);
                foreach (OrderDetail lineFollow in details)
                {
                    lineFirst.Quantity += lineFollow.Quantity;
                }

                if(lineFirst.Quantity > product.InStock)
                {
                    lineFirst.Quantity = product.InStock;
                }
                lineFirst.TotalAmount = lineFirst.Quantity * product.Price;
                db.Entry(lineFirst).State = EntityState.Modified;
                db.OrderDetails.RemoveRange(details);
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Re-calculate the cost of the order
        /// <para> call this after you modified the order, or the order-details
        /// </para>
        /// </summary>
        /// <param name="id">of the order</param>
        private void RecalculateOrderCost(int id)
        {
            Order order = db.Orders.Find(id);
            double total = 0;
            foreach (OrderDetail aLine in order.OrderDetails)
            {
                total += aLine.TotalAmount;
            }
            order.TotalAmount = total;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();
        }
        #endregion

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult DropDownOrderState()
        {
            //int[] itemValues = System.Enum.GetValues(typeof(OrderStatus)) as int[];
            //string[] itemNames = System.Enum.GetNames(typeof(OrderStatus));

            //var dict = new Dictionary<int, string>();
            //for (int i = 0; i <= itemNames.Length - 1; i++)
            //{
            //    dict[itemValues[i]] = itemNames[i];
            //}



            // nah, I hate to hard code this
            Dictionary<int, string> dict;

            if (User.IsInRole("Admin"))
            {
                dict = new Dictionary<int, string>
                {
                    [0] = "Mới tạo",
                    [1] = "Đã đóng gói",
                    [2] = "Đang giao",
                    [3] = "Hoàn tất",
                    [4] = "Chờ duyệt",
                    [5] = "Đã bị huỷ"
                };
            }
            else
            {
                dict = new Dictionary<int, string>();
            }

            var list = dict.ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        ///// <summary>
        ///// Checkout an order
        ///// <para> e.g. The customer paid for their order </para>
        ///// </summary>
        ///// <param name="id"> order id </param>
        ///// <returns></returns>
        //[HttpPost]
        //[Authorize(Roles = "Admin,ThuNgan")]
        //public ActionResult Checkout(int id)
        //{
        //    ResultWeb result = new ResultWeb();
        //    Order order = db.Orders.Find(id);
        //    if(order == null)
        //    {
        //        result.Type = ResultWeb.ResultType.FIELD_INVALID;
        //        return Json(result, JsonRequestBehavior.AllowGet);
        //    }
        //    switch (order.Status)
        //    {
        //        case OrderStatus.New:
        //            {
        //                SubProduct(order);
        //                order.Status = OrderStatus.Completed;
        //                db.Entry(order).State = EntityState.Modified;
        //                db.SaveChanges();
        //                result.Type = ResultWeb.ResultType.OK;
        //            }
        //            break;
        //        case OrderStatus.Packing:
        //            {
        //                order.Status = OrderStatus.Delivering;
        //                db.Entry(order).State = EntityState.Modified;
        //                db.SaveChanges();
        //                result.Type = ResultWeb.ResultType.OK;
        //            }
        //            break;
        //        case OrderStatus.Delivering:
        //            {
        //                order.DeliveryDate = DateTime.Today;
        //                order.Status = OrderStatus.Completed;
        //                db.Entry(order).State = EntityState.Modified;
        //                db.SaveChanges();
        //                result.Type = ResultWeb.ResultType.OK;
        //            }
        //            break;
        //        case OrderStatus.Pending:
        //            {
        //                SubProduct(order);
        //                order.Status = OrderStatus.Packing;
        //                db.Entry(order).State = EntityState.Modified;
        //                db.SaveChanges();
        //                result.Type = ResultWeb.ResultType.OK;
        //            }
        //            break;
        //    }
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult Complete(int Id)
        {
            ResultWeb result = new ResultWeb();
            Order order = db.Orders.Find(Id);
            if (order == null)
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            if (!(order.Status == OrderStatus.New && order.AnonymousUserId == null))
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            if (!User.IsInRole("Admin"))
            {
                if (order.UserId != user.ID)
                {
                    Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);
                    result.Type = ResultWeb.ResultType.ACCESS_VIOLENCE;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }

            order.DeliveryDate = DateTime.Today;
            order.Status = OrderStatus.Completed;
            SubProduct(order);
            //db.Entry(order).State = EntityState.Modified;
            //db.SaveChanges();
            result.Type = ResultWeb.ResultType.OK;


            int cookieId;
            if (Request.Cookies["order_id"] != null)
            {
                cookieId = int.Parse(Request.Cookies["order_id"].Value);
                if (cookieId == Id)
                {                    
                     Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);                    
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private bool SubProduct(Order order)
        {
            foreach (var detail in order.OrderDetails)
            {
                detail.Product.InStock -= detail.Quantity;
                db.Entry(detail.Product).State = System.Data.Entity.EntityState.Modified;
            }
            
            db.Entry(order).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return true;
        }

        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult UpdateState(int orderId, int state)
        {
            if(!User.IsInRole("Admin"))
            {
                return Json(
                    new ResultWeb {Type = ResultWeb.ResultType.ACCESS_VIOLENCE }
                    , JsonRequestBehavior.AllowGet);
            }

            ResultWeb result = new ResultWeb();

            Order order = db.Orders.Find(orderId);
            if(order == null)
            {
                result.Type = ResultWeb.ResultType.NOT_FOUND;
                return Json(result, JsonRequestBehavior.AllowGet);
            }

            // need: check authorize

            order.Status = (OrderStatus)state;
            db.Entry(order).State = EntityState.Modified;
            db.SaveChanges();

            result.Type = ResultWeb.ResultType.OK_UPDATE;
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult Export(int? id)
        {
            Order order;

            if (id != null)
            {
                order = db.Orders.Find(id);
                if (order == null)
                {
                    return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không tìm thấy đơn hàng với id: " + id.ToString()
                        });
                }

                Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                if (!User.IsInRole("Admin"))
                {
                    int userId = user.ID;

                    bool check1 = (order.AnonymousUserId != null) && (order.Status != OrderStatus.Canceled) && (order.Status != OrderStatus.Completed);
                    bool check2 = (order.AnonymousUserId == null) && (order.UserId == user.ID) && (order.Status == OrderStatus.New);
                    if (!(check1 || check2))
                    {
                        return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập đơn hàng này"
                        });
                    }
                }

                if (order.UserId == null)
                {
                    order.UserId = user.ID;
                    db.Entry(order).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    user = order.User;
                }

                // ------------------------ this is ok -------------------
                //return new ActionAsPdf("PrintPage", order);
                ViewBag.StaffName = user.LastName + " " + user.FirstName;
                ViewBag.Name = "Nhà sách AC";
                ViewBag.Title = "Hoá đơn bán hàng";
                ViewBag.Address = "Xa lộ Hà Nội, phường Linh Trung, quận Thủ Đức, thành phố Hồ Chí Minh";
                ViewBag.StrMoney = MoneyReader((decimal)order.TotalAmount);
                return View(order);
            }
            else
            {
                return RedirectToAction("Error", "Staff"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không xác định đơn hàng để in"
                        });
            }
        }

        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult PrintPage(int id)
        {
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return RedirectToAction("Error", "Staff"
                    , new
                    {
                        errorCode = "Lỗi 404."
                        ,
                        errorDetail = "Không tìm thấy đơn hàng với id: " + id.ToString()
                    });
            }

            Models.User user;
            if (order.UserId == null)
            {
                user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                order.UserId = user.ID;
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                user = order.User;
            }
             

            ViewBag.StaffName = user.LastName + " " + user.FirstName;
            ViewBag.Name = "Nhà sách AC";
            ViewBag.Title = "Hoá đơn bán hàng";
            ViewBag.Address = "Xa lộ Hà Nội, phường Linh Trung, quận Thủ Đức, thành phố Hồ Chí Minh";
            ViewBag.StrMoney = MoneyReader((decimal)order.TotalAmount);
            return View(order);
        }

        private String MoneyReader(decimal total)  //đọc đc 18 số vd: 999999999999999999
        {
            try
            {
                string rs = "";
                total = Math.Round(total, 0);
                string[] ch = { "không", "một", "hai", "ba", "bốn", "năm", "sáu", "bảy", "tám", "chín" };
                string[] rch = { "lẻ", "mốt", "", "", "", "lăm" };
                string[] u = { "", "mươi", "trăm", "ngàn", "", "", "triệu", "", "", "tỷ", "", "", "ngàn", "", "", "triệu" };
                string nstr = total.ToString();
                int[] n = new int[nstr.Length];
                int len = n.Length;
                for (int i = 0; i < len; i++)
                {
                    n[len - 1 - i] = Convert.ToInt32(nstr.Substring(i, 1));
                }
                for (int i = len - 1; i >= 0; i--)
                {
                    if (i % 3 == 2)// số 0 ở hàng trăm
                    {
                        if (n[i] == 0 && n[i - 1] == 0 && n[i - 2] == 0) continue;//nếu cả 3 số là 0 thì bỏ qua không đọc
                    }
                    else if (i % 3 == 1) // số ở hàng chục
                    {
                        if (n[i] == 0)
                        {
                            if (n[i - 1] == 0) { continue; }// nếu hàng chục và hàng đơn vị đều là 0 thì bỏ qua.
                            else
                            {
                                rs += " " + rch[n[i]]; continue;// hàng chục là 0 thì bỏ qua, đọc số hàng đơn vị
                            }
                        }
                        if (n[i] == 1)//nếu số hàng chục là 1 thì đọc là mười
                        {
                            rs += " mười"; continue;
                        }
                    }
                    else if (i != len - 1)// số ở hàng đơn vị (không phải là số đầu tiên)
                    {
                        if (n[i] == 0)// số hàng đơn vị là 0 thì chỉ đọc đơn vị
                        {
                            if (i + 2 <= len - 1 && n[i + 2] == 0 && n[i + 1] == 0) continue;
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 1)// nếu là 1 thì tùy vào số hàng chục mà đọc: 0,1: một / còn lại: mốt
                        {
                            rs += " " + ((n[i + 1] == 1 || n[i + 1] == 0) ? ch[n[i]] : rch[n[i]]);
                            rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);
                            continue;
                        }
                        if (n[i] == 5) // cách đọc số 5
                        {
                            if (n[i + 1] != 0) //nếu số hàng chục khác 0 thì đọc số 5 là lăm
                            {
                                rs += " " + rch[n[i]];// đọc số 
                                rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                                continue;
                            }
                        }
                    }
                    rs += (rs == "" ? " " : ", ") + ch[n[i]];// đọc số
                    rs += " " + (i % 3 == 0 ? u[i] : u[i % 3]);// đọc đơn vị
                }
                if (rs[rs.Length - 1] != ' ')
                    rs += " đồng.";
                else
                    rs += "đồng.";
                if (rs.Length > 2)
                {
                    string rs1 = rs.Substring(0, 2);
                    rs1 = rs1.ToUpper();
                    rs = rs.Substring(2);
                    rs = rs1 + rs;
                }
                return rs.Trim().Replace("lẻ,", "lẻ").Replace("mươi,", "mươi").Replace("trăm,", "trăm").Replace("mười,", "mười").Replace("Mười,", "Mười").Replace(",", " ");
            }
            catch
            {
                return "Số bạn nhập vào quá lớn";
            }
        }
    }
}