using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using BookStore.Data;
using Newtonsoft.Json;
using BookStore.ViewModels;
using System.Data.Entity;

namespace BookStore.Controllers
{
    /// <summary>
    /// Focus in Handling carts for online orders. Orders that custommer buy directly in the shop, see OrderController
    /// </summary>
    public class CartController : Controller
    {
        BookStoreContext db = new BookStoreContext();

        /// <summary>
        /// Checkout order [view]
        /// </summary>
        [HttpGet]
        public ActionResult Index()
        {
            //CheckCustomerId();
            var viewModel = new UserViewModel();
            var cookie = Request.Cookies["customer_id"];
            var user = db.AnonymousUsers.Find(Int32.Parse(cookie.Value));
            if(user != null)
            {
                viewModel.FirstName = user.FirstName;
                viewModel.LastName = user.LastName;
                viewModel.PhoneNumber = user.PhoneNumber;
                viewModel.Email = user.Email;
                viewModel.Address = user.Address;
            }
            return View(viewModel);
        }
        [HttpPost]
        public ActionResult Index(UserViewModel user)
        {
            var userObj = new AnonymousUser
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Address = user.Address,
                PhoneNumber = user.PhoneNumber
            };
            
            if (ModelState.IsValid)
            {
                var cookie = Request.Cookies["customer_id"];
                userObj.ID = Int32.Parse(cookie.Value);
                db.Entry(userObj).State = EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "<script>alert('Cập nhật thông tin thành công');</script>";
            }
            return View(user);
        }
        public ActionResult CartPartial()
        {
            Order cart = CheckOrderId();
            var details = cart.OrderDetails;
            if(details == null)
            {
                details = new List<OrderDetail>();
            }

            ViewBag.Subtotal = cart.TotalAmount;
            return PartialView(details.ToList());
        }
        
        /// <summary>
        /// Get an order by id [json]
        /// </summary>
        [HttpGet]
        public ActionResult GetCart(int? id, int? depth)
        {
            Order yourOrder;
            if (id != null)
            {
                yourOrder = db.Orders.Find(id);
                if (yourOrder == null)
                {
                    return null;
                }
            }
            else
            {
                yourOrder = CheckOrderId();
            }

            var result = new JsonNetResult
            {
                Data = yourOrder,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = depth ?? 3// order, orderdetail, product
                }
            };

            return result;
        }

        #region Customer

    // Lấy thông tin 1 khách hàng bằng id, trả về JSON
    [HttpGet]
    public ActionResult GetCustomer(int? id)
    {
        AnonymousUser customer;
        if (id != null)
        {   // nếu có id, tìm trong cơ sở dữ liệu về khách hàng này
            customer = db.AnonymousUsers.Find(id);
            if (customer == null)
            {   // không tìm thấy? trả về rỗng
                return null;
            }
        }
        else
        {   // nếu không truyền vào id, sẽ lấy id khách hàng hiện tại
            customer = CheckCustomerId();
        }
        // trả về thông tin khách hàng
        var result = new JsonNetResult
        {
            Data = customer,// dữ liệu để tự động parse ra JSON
            JsonRequestBehavior = JsonRequestBehavior.AllowGet,// cho phép truyền json này
            Settings = {
        // do LazyLoading, khi customer chứa giỏ hàng thì tự động query lấy giỏ hàng
        //   rồi trong giỏ hàng lại chứa customer, thì lại tự động query lấy customer
        //     như vậy không nên. Setting này sẽ không query nữa nếu thấy query dữ liệu lặp lại
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        MaxDepth = 1// số tầng của json
            }
        };
        return result;// trả về JSON thông tin khách hàng
    }

        /// <summary>
        ///  [json]
        /// </summary>
        [HttpPost]
        public ActionResult UpdateCurrentCustomer(
            string firstName
            , string lastName
            , string telephone
            , string email
            , string address)
        {
            AnonymousUser customer = CheckCustomerId();

            customer.FirstName = firstName;
            customer.LastName = lastName;
            customer.PhoneNumber = telephone;
            customer.Email = email;
            customer.Address = address;


            db.Entry(customer).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            return Json(
                new { success = true
                , text = "cập nhật thông tin khách hàng thành công" }
            , JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region cart update

        /// <summary>
        /// add product into the cart [json]
        /// </summary>
        /// <param name="id">product Id</param>
        [HttpPost]
        public ActionResult AddProduct(int id)
        {
            ResultWeb result = new ResultWeb();
            Order cart = CheckOrderId();

            Product yourProduct = db.Products.Find(id);
            if(yourProduct == null)
            {
                //result.Type = ResultWeb.ResultType.NOT_FOUND;
                return Json(new
                {
                    success = false
                , text = "Không tìm thấy sản phẩm này"
                }, JsonRequestBehavior.AllowGet);
            }

            var details = cart.OrderDetails;

            if(details == null)
            {
                details = new List<OrderDetail>();
            }
            
            OrderDetail yourDetail = details.Where(od => od.ProductId == id).FirstOrDefault();
            // trường hợp đã thêm sản phẩm này rồi, thêm nó vào lần nữa
            if(yourDetail != null)
            {
                if(yourProduct.InStock <= yourDetail.Quantity)
                {
                    //result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                    return Json(new
                    {
                        success = false ,
                        text = "Sản phẩm không đủ hàng"
                    }, JsonRequestBehavior.AllowGet);
                }
                
                yourDetail.Quantity += 1;
                yourDetail.TotalAmount = yourDetail.Quantity * yourProduct.Price;
                db.Entry(yourDetail).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
            }
            else// trường hợp chưa có sản phẩm này, thêm vào giỏ
            {
                if (yourProduct.InStock <= 0)
                {
                    //result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                    return Json(new
                    {
                        success = false
                ,
                        text = "Sản phẩm không đủ hàng"
                    }, JsonRequestBehavior.AllowGet);
                }

                yourDetail = new OrderDetail
                {
                    OrderId = cart.ID,
                    ProductId = yourProduct.ID,
                    Quantity = 1,
                    TotalAmount = yourProduct.Price
                };
                
                db.OrderDetails.Add(yourDetail);
                db.SaveChanges();
            }
            RecalculateOrderCost(cart);
            //return PartialView("CartPartial", details.ToList());
            result.Type = ResultWeb.ResultType.OK;
            return Json(new
            {
                success = true
                ,
                text = "thêm sản phẩm vào giỏ thành công"
            }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// minus a product in the cart [json]
        /// </summary>
        /// <param name="id">product id</param>
        [HttpPost]
        public ActionResult MinusProduct(int id)
        {
            ResultWeb result = new ResultWeb();
            Order cart = CheckOrderId();

            Product yourProduct = db.Products.Find(id);
            if (yourProduct == null)
            {
                //result.Type = ResultWeb.ResultType.NOT_FOUND;
                return Json(new
                {
                    success = false
                ,
                    text = "Không tìm thấy sản phẩm"
                }, JsonRequestBehavior.AllowGet);
            }

            var details = cart.OrderDetails;

            if(details == null)
            {
                return Json(new
                {
                    success = false
                ,
                    text = "Không tìm thấy chi tiết giỏ hàng"
                }, JsonRequestBehavior.AllowGet);
            }

            OrderDetail yourDetail = details.Where(od => od.ProductId == id).FirstOrDefault();
            if (yourDetail != null)
            {
                if(yourDetail.Quantity < 0)
                {
                    yourDetail.Quantity = 0;
                    yourDetail.TotalAmount = 0;
                    db.Entry(yourDetail).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    RecalculateOrderCost(cart);

                    //result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(new
                    {
                        success = false
                ,
                        text = "Lỗi chi tiết giỏ hàng không xác định"
                    }, JsonRequestBehavior.AllowGet);
                }
                if(yourDetail.Quantity == 0)
                {
                    //result.Type = ResultWeb.ResultType.OK;
                    return Json(new
                    {
                        success = false
                ,
                        text = "Chi tiết giỏ hàng đã ở số nhỏ nhất"
                    }, JsonRequestBehavior.AllowGet);
                }

                string resultText = "";
                bool theSuccess = false;
                if (yourProduct.InStock < yourDetail.Quantity - 1)
                {
                    //result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                    theSuccess = false;
                    resultText = "Lỗi số lượng chi tiết giỏ hàng";
                    yourDetail.Quantity = yourProduct.InStock;
                }
                else
                {
                    //result.Type = ResultWeb.ResultType.OK;
                    theSuccess = true;
                    resultText = "Bớt sản phẩm thành công";
                    yourDetail.Quantity -= 1;
                }
                
                yourDetail.TotalAmount = yourDetail.Quantity * yourProduct.Price;
                db.Entry(yourDetail).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                RecalculateOrderCost(cart);

                return Json(new
                {
                    success = theSuccess
                ,
                    text = resultText
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(new
                {
                    success = false
                ,
                    text = "Lỗi đơn hàng không hợp lệ"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        /// <summary>
        /// remove a product line in your cart [json]
        /// </summary>
        /// <param name="id">the product id</param>
        [HttpPost]
        public ActionResult DeleteProduct(int id)
        {
            ResultWeb result = new ResultWeb();
            Order cart = CheckOrderId();

            Product yourProduct = db.Products.Find(id);
            if (yourProduct == null)
            {
                //result.Type = ResultWeb.ResultType.NOT_FOUND;
                return Json(new
                {
                    success = false
                ,
                    text = "Không tìm thấy sản phẩm"
                }, JsonRequestBehavior.AllowGet);
            }

            var details = cart.OrderDetails;

            if (details == null)
            {
                return Json(new
                {
                    success = false
                ,
                    text = "Không tìm thấy chi tiết giỏ hàng"
                }, JsonRequestBehavior.AllowGet);
            }

            OrderDetail yourDetail = details.Where(od => od.ProductId == id).FirstOrDefault();
            if (yourDetail != null)
            {
                db.OrderDetails.Remove(yourDetail);
                db.SaveChanges();
                RecalculateOrderCost(cart);

                //result.Type = ResultWeb.ResultType.OK_DELETE;
                return Json(new
                {
                    success = true
                ,
                    text = "Xoá thành công"
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                //result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(new
                {
                    success = false
                ,
                    text = "Không tìm thấy chi tiết giỏ hàng"
                }, JsonRequestBehavior.AllowGet);
            }
        }

        #endregion


        /// <summary>
        /// checkout a cart [json]
        /// </summary>
        [HttpPost]
        public ActionResult Checkout()
        {
            if (Request.Cookies["cart_id"] == null)
            {
                //result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(new { success = false, text = "Dữ liệu không hợp lệ | Không có giỏ hàng để thanh toán" }, JsonRequestBehavior.AllowGet);
            }



            Order cart = CheckOrderId();
            // có khuyến mãi 0đ thì vui
            if(cart.TotalAmount == 0)
            {
                //result.Type = ResultWeb.ResultType.FIELD_INVALID;
                return Json(new { success = false, text = "Bạn không thể không mua gì cả" }, JsonRequestBehavior.AllowGet);
            }


            // sub products in list
            if(!CheckStock(cart))
            {
                RecalculateOrderCost(cart);
                //result.Type = ResultWeb.ResultType.OUT_OF_STOCK;
                return Json(new { success = false, text = "Chúng tôi rất tiếc, tình trạng kho không đủ đáp ứng, chúng tôi đã cập nhật lại thông tin giỏ hàng" }, JsonRequestBehavior.AllowGet);
            }
            
            //SureCheckOut(cart);
            cart.Status = OrderStatus.Pending;
            db.Entry(cart).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            // delete the cookie
            Response.Cookies["cart_id"].Expires = DateTime.Today.AddDays(-1);

            //result.Type = ResultWeb.ResultType.OK;
            return Json(new { success = true, text = "Đặt hàng thành công" }, JsonRequestBehavior.AllowGet);
        }


        #region Helper

        /// <summary>
        /// If "no cart id" access the web => gen a new cart
        /// </summary>
        private Order CheckOrderId()
        {
            string orderId = "";
            Order order = null;
            var requestCookie = Request.Cookies["cart_id"];

            if (requestCookie != null)
            {
                orderId = requestCookie.Value;
                int cartId = int.Parse(orderId);
                order = db.Orders.Find(cartId);
                if (order == null)
                {
                    requestCookie = null;
                }

                if(order!= null && order.Status != OrderStatus.New)
                {
                    requestCookie = null;
                }
            }

            if (requestCookie == null)
            {
                order = new Order
                {
                    AnonymousUserId = CheckCustomerId().ID,
                    CreatedAt = DateTime.Today,
                    DeliveryDate = DateTime.Today.AddDays(-1),
                    Address = "",
                    Description = "",
                    Status = OrderStatus.New,
                    TotalAmount = 0
                };                

                db.Orders.Add(order);
                db.SaveChanges();

                orderId = order.ID.ToString();
            }

            Response.Cookies["cart_id"].Value = orderId;
            Response.Cookies["cart_id"].Expires = DateTime.Now.AddDays(30);

            return order;
        }
        

        /// <summary>
        /// If "no customer id" access the web => gen a new customer
        /// </summary>
        private AnonymousUser CheckCustomerId()
        {
            string customerId = "";
            AnonymousUser customer = null;

            var cookie = Request.Cookies["customer_id"];

            if (cookie != null)
            {
                customerId = cookie.Value;
                int cusId = int.Parse(customerId);
                customer = db.AnonymousUsers.Find(cusId);
                if (customer == null)
                {
                    cookie = null;
                }
            }

            if (cookie == null)
            {
                customer = new AnonymousUser();
                db.AnonymousUsers.Add(customer);
                db.SaveChanges();

                customerId = customer.ID.ToString();
            }

            Response.Cookies["customer_id"].Value = customerId;
            Response.Cookies["customer_id"].Expires = DateTime.Now.AddDays(30);

            return customer;
        }

        /// <summary>
        /// Re-calculate the cost of the order
        /// <para> call this after you modified the order, or the order-details
        /// </para>
        /// </summary>
        /// <param name="id">of the order</param>
        private void RecalculateOrderCost(Order cart)
        {
            List<OrderDetail> willRemoveDetails = new List<OrderDetail>();
            foreach (OrderDetail aLine in cart.OrderDetails)
            {
                if(aLine.Quantity <= 0)
                {
                    willRemoveDetails.Add(aLine);
                }
            }

            if (willRemoveDetails.Count > 0)
            {
                db.OrderDetails.RemoveRange(willRemoveDetails);
                db.SaveChanges();
                cart = db.Orders.Find(cart.ID);
            }

            double total = 0;
            foreach (OrderDetail aLine in cart.OrderDetails)
            {
                total += aLine.TotalAmount;
            }
            cart.TotalAmount = total;
            db.Entry(cart).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
        }

        /// <summary>
        /// check the products available in stock
        /// </summary>
        private bool CheckStock(Order cart)
        {
            bool theReturn = true;
            Product tmpProduct;
            foreach(var detail in cart.OrderDetails)
            {
                tmpProduct = detail.Product;
                if (detail.Quantity > tmpProduct.InStock)
                {
                    theReturn = false;

                    detail.Quantity = tmpProduct.InStock;
                    detail.TotalAmount = tmpProduct.Price * detail.Quantity;
                    db.Entry(detail).State = System.Data.Entity.EntityState.Modified;
                }
            }

            if(theReturn == false)
            {
                db.SaveChanges();
            }

            return theReturn;
        }
        
        //private bool SureCheckOut(Order cart)
        //{
        //    foreach (var detail in cart.OrderDetails)
        //    {
        //        detail.Product.InStock -= detail.Quantity;
        //        db.Entry(detail.Product).State = System.Data.Entity.EntityState.Modified;
        //    }

        //    cart.Status = OrderStatus.Pending;
        //    db.Entry(cart).State = System.Data.Entity.EntityState.Modified;
        //    db.SaveChanges();

        //    return true;
        //}

        #endregion
    }
}