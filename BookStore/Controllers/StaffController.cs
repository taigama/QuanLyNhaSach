using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BookStore.Models;
using Newtonsoft.Json;
using BookStore.Helpers;
using System.Data.Entity;
using BookStore.Data;
using System.IO;
using BookStore.ViewModels;
using System.Web.Security;

namespace BookStore.Controllers
{
    /// <summary>
    /// main class for Dashboard handling
    /// </summary>
    public class StaffController : BaseController
    {
        Data.BookStoreContext db = new Data.BookStoreContext();
        JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
        };

        // GET: Staff
        /// <summary>
        /// Dashboard [view]
        /// </summary>
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult Index()
        {
            ViewBag.CountProduct = db.Products.Count();
            ViewBag.CountOrder = db.Orders.Count();
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        #region orders management

        /// <summary>
        ///  [view]
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ListOrder()
        {
            Models.User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            ViewBag.User = user;
            List<Order> orders;
            if(User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
                //orders = db.Orders.ToList();

                orders = (from childs in db.Orders
                          where (childs.Status != OrderStatus.Completed
                          && childs.Status != OrderStatus.Canceled
                          && (childs.AnonymousUserId == null || childs.Status != OrderStatus.New))
                          select childs).ToList();

                ViewBag.DictUser = db.Users.ToDictionary(us => us.ID, us => (us.LastName + us.FirstName));
            }
            else
            {
                ViewBag.IsAdmin = false;
                

                orders = (from childs in db.Orders
                          where ((childs.AnonymousUserId != null && (childs.Status != OrderStatus.New && childs.Status != OrderStatus.Completed && childs.Status != OrderStatus.Canceled))
                          || (childs.AnonymousUserId == null && childs.Status == OrderStatus.New && childs.UserId == user.ID))
                          select childs).ToList();
            }
            return View(orders);
        }

        /// <summary>
        ///  [view]
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult Cashier(int? Id)
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }

            Order order;
            User user = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            ViewBag.User = user;

            int userId = user.ID;
            if (Id != null)
            {
                order = db.Orders.Find(Id);
                if (order == null)
                {
                    //return new HttpNotFoundResult("khong tim thay don hang nay");

                    return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            , errorDetail = "Không tìm thấy đơn hàng với id: " + Id.ToString()
                        });
                }

                if (!(order.Status == OrderStatus.New && order.AnonymousUserId == null))
                {
                    return RedirectToAction("ViewOrder", "Order", new { Id });
                }

                if (!User.IsInRole("Admin"))
                {
                    if(order.UserId != userId)
                    {
                        Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);
                        return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập đơn hàng này"
                        });
                    }
                }

                Response.Cookies["order_id"].Value = order.ID.ToString();
                Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(3);
                ViewBag.id = Id;
                return View();
            }



            if (Request.Cookies["order_id"] != null)
            {
                Id = int.Parse(Request.Cookies["order_id"].Value);
                order = db.Orders.Find(Id);
                if (order != null)
                {
                    if (!(order.Status == OrderStatus.New && order.AnonymousUserId == null))
                    {
                        return RedirectToAction("ViewOrder", "Order", new { Id });
                    }

                    if (!User.IsInRole("Admin"))
                    {
                        if (order.UserId != userId)
                        {
                            Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(-1);
                            return RedirectToAction("Error"
                            , new
                            {
                                errorCode = "Lỗi 403."
                                ,
                                errorDetail = "Bạn không có quyền truy cập đơn hàng này"
                            });
                        }
                    }

                    ViewBag.id = Id;
                    return View();
                }
                Id = null;
            }            

            order = new Order
            {
                UserId = userId,
                Status = OrderStatus.New,
                CreatedAt = DateTime.Today,
                DeliveryDate = DateTime.Today,
                TotalAmount = 0
            };
            db.Orders.Add(order);
            db.SaveChanges();

            Response.Cookies["order_id"].Value = order.ID.ToString();
            Response.Cookies["order_id"].Expires = DateTime.Now.AddDays(7);
            ViewBag.id = order.ID;

            //details = new List<OrderDetail>();

            return View();
        }

        
        #endregion

        

        public ActionResult Error(string errorCode, string errorDetail)
        {
            ViewBag.ErrorCode = errorCode;
            ViewBag.ErrorDetail = errorDetail;
            return View();
        }

        // -------------------------------------- PRODUCTS --------------------------------------- //

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ListProduct()
        {
            if(!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult FixCountCategory()
        {
            if(!User.IsInRole("Admin"))
            {
                return JResponse(true, "Bạn không có quyền sử dụng chức năng này!");
            }

            foreach (var cate in db.Categories)
            {
                if (cate.Products != null)
                {
                    cate.NumberOfProducts = cate.Products.Count;
                    db.Entry(cate).State = EntityState.Modified;
                }
            }
            db.SaveChanges();

            return JResponse(true, "Cập nhật số lượng loại sản phẩm thành công!");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddProduct(bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(ProductVM.GenerateProductVM(db, success));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddProduct(ProductVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            if (ModelState.IsValid)
            {
                myModel.Product.ProductDetails = myModel.Details;
                db.Products.Add(myModel.Product);
                db.SaveChanges();

                if (myModel.Details != null)
                {
                    foreach (ProductDetail detail in myModel.Details)
                    {
                        UpdateCountAuthor(detail.AuthorId);
                    }
                }
                UpdateCountCategory(myModel.Product.CategoryId);

                return RedirectToAction("AddProduct", "Staff", new { success = true });
            }
            else
            {
                ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                return View();
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditProduct(int? productId, bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            if (productId == null)
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không xác định sản phẩm"
                        });

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(ProductVM.GenerateProductVM(db, productId, success));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditProduct(ProductVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            if (ModelState.IsValid)
            {
                db.Entry(myModel.Product).State = EntityState.Modified;
                db.SaveChanges();

                db.Entry(myModel.Product).State = EntityState.Detached;

                var details = db.Products.Find(myModel.Product.ID).ProductDetails;
                db.ProductDetails.RemoveRange(details); // Xóa detail
                db.SaveChanges(); // Lưu

                if (myModel.Details != null)
                {
                    foreach (var item in myModel.Details)
                    {
                        item.ProductId = myModel.Product.ID;
                        db.ProductDetails.Add(item);
                    }
                }
                db.SaveChanges();

                if (myModel.Details != null)
                {
                    foreach (ProductDetail detail in myModel.Details)
                    {
                        UpdateCountAuthor(detail.AuthorId);
                    }
                }
                UpdateCountCategory(myModel.Product.CategoryId);

                return RedirectToAction("EditProduct", "Staff", new {
                    productId = myModel.Product.ID,
                    success = true
                });
            }
            else
            {
                ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                return View();
            }
        }

        [HttpGet]
        public ActionResult GetCategoryByType(int typeId)
        {
            Dictionary<int, string> dict = db.Categories
                .Where(m => (int)m.Type == typeId)
                .ToDictionary(m => m.ID, m => m.Name);

            return Json(dict.ToList(), JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropdownPublisher()
        {
            var list = db.Publishers
               .ToDictionary(item => item.ID, item => item.Name)
               .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropdownCategory()
        {
            var list = db.Categories
               .ToDictionary(item => item.ID, item => item.Name)
               .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropdownProduct()
        {
            var list = db.Products
               .ToDictionary(item => item.ID, item => item.Name)
               .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DropdownAuthor()
        {
            var list = db.Authors
               .ToDictionary(item => item.ID, item => item.Name)
               .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetListProductByName(int id)
        {
            JsonNetResult result = new JsonNetResult
            {
                Data = (id == 0) ? db.Products.ToList() : new List<Product> { db.Products.Find(id) },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 3
                }
            };
            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetListProductByCategory(int id)
        {
            JsonNetResult result = new JsonNetResult
            {
                Data = (id == 0) ? db.Products.ToList() : db.Categories.Find(id).Products,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 3
                }
            };
            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetListProductByPublisher(int id)
        {
            JsonNetResult result = new JsonNetResult
            {
                Data = (id == 0) ? db.Products.ToList() : db.Publishers.Find(id).Books,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 3
                }
            };
            return result;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetStoreAuthor(int id)
        {
            var list = db.Products.Find(id).ProductDetails
                .Where(m => m != null)
                .ToDictionary(m => m.AuthorId, m => m.Author.Name)
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult UploadFiles()
        {
            ResultWeb result = new ResultWeb();
            // Checking no of files injected in Request object  
            if (Request.Files.Count > 0)
            {
                try
                {
                    //  Get all files from Request object  
                    HttpFileCollectionBase files = Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        //string path = AppDomain.CurrentDomain.BaseDirectory + "Uploads/";  
                        //string filename = Path.GetFileName(Request.Files[i].FileName);  

                        HttpPostedFileBase file = files[i];
                        string fname = String.Empty;
                        string fpath = String.Empty;
                        string fstorePath = String.Empty;

                        // Checking for Internet Explorer  
                        if (Request.Browser.Browser.ToUpper() == "IE" || Request.Browser.Browser.ToUpper() == "INTERNETEXPLORER")
                        {
                            string[] testfiles = file.FileName.Split(new char[] { '\\' });
                            fname = testfiles[testfiles.Length - 1];
                        }
                        else
                        {
                            fname = file.FileName;
                        }

                        // Get the complete folder path and store the file inside it.  

                        do
                        {
                            fpath = HtmlExtensions.RandomString(10) + fname;
                            fstorePath = Path.Combine(Server.MapPath("~/UploadFiles/"), fpath);
                            fpath = @"~/UploadFiles/" + fpath;
                        } while (System.IO.File.Exists(fstorePath));
                        
                        file.SaveAs(fstorePath);
                        result.StringValue = fpath;
                    }
                    
                    result.Type = ResultWeb.ResultType.OK;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                catch 
                {
                    result.Type = ResultWeb.ResultType.SOMETHING_NOT_RIGHT;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                result.Type = ResultWeb.ResultType.OK;
                result.StringValue = "";
                return Json(result, JsonRequestBehavior.AllowGet);
            }
        }

        // --------------------------------------- AUTHORS ----------------------------------------- //

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ListAuthor()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpPost]
        [Authorize(Roles="Admin")]
        public ActionResult FixCountAuthor()
        {
            foreach(var author in db.Authors.ToList())
            {
                if(author.Books != null)
                {
                    author.NumberOfBooks = author.Books.Count;
                    db.Entry(author).State = EntityState.Modified;
                }
            }
            db.SaveChanges();

            return Json(
                new { success = true
                , text = "Cập nhật số lượng đầu sách thành công!" }
            , JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [Authorize(Roles = "Admin, ThuNgan")]
        public ActionResult AddAuthor(bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(AuthorVM.GenerateAuthorVM(db, success));
        } 

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddAuthor(AuthorVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            if (ModelState.IsValid)
            {
                db.Authors.Add(myModel.Author);
                db.SaveChanges();
                return RedirectToAction("AddAuthor", "Staff", new { success = true });
            }
            else
            {
                ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
                return View();
            }            
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditAuthor(int? authorId, bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            if (authorId == null)
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không xác định tác giả"
                        });
            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(AuthorVM.GenerateAuthorVM(db, authorId, success));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditAuthor(AuthorVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            if (ModelState.IsValid)
            {
                db.Entry(myModel.Author).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EditAuthor", "Staff", new {
                    authorId = myModel.Author.ID,
                    success = true
                });
            }
            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetAuthors()
        {
            var result = new JsonNetResult
            {
                Data = db.Authors.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };

            return result;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteAuthor(int? authorId)
        {
            ResultWeb result = new ResultWeb();
            if (ModelState.IsValid)
            {
                Author author = db.Authors.Find(authorId);

                if (author == null)
                {
                    result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                db.Authors.Remove(author);
                db.SaveChanges();

                result.Type = ResultWeb.ResultType.OK_DELETE;
            }
            else
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        // --------------------------------------- PUBLISHERS ---------------------------------------- //

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult ListPublisher()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddPublisher(bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(PublisherVM.GeneratePublisherVM(db, success));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult AddPublisher(PublisherVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            if (ModelState.IsValid)
            {
                db.Publishers.Add(myModel.Publisher);
                db.SaveChanges();
                return RedirectToAction("AddPublisher", "Staff", new { success = true });
            }
            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditPublisher(int? publisherId, bool success = false)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            if (publisherId == null)
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 404."
                            ,
                            errorDetail = "Không xác định nhà sản xuất / nhà xuất bản"
                        });
            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View(PublisherVM.GeneratePublisherVM(db, publisherId, success));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult EditPublisher(PublisherVM myModel, FormCollection collection)
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }

            if (ModelState.IsValid)
            {
                db.Entry(myModel.Publisher).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EditPublisher", "Staff", new
                {
                    publisherId = myModel.Publisher.ID,
                    success = true,
                });
            }
            ViewBag.IsAdmin = true;
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GetPublishers()
        {
            var result = new JsonNetResult
            {
                Data = db.Publishers.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };

            return result;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult DeletePublisher(int? publisherId)
        {
            ResultWeb result = new ResultWeb();
            if (ModelState.IsValid)
            {
                Publisher publisher = db.Publishers.Find(publisherId);

                if (publisher == null)
                {
                    result.Type = ResultWeb.ResultType.FIELD_INVALID;
                    return Json(result, JsonRequestBehavior.AllowGet);
                }

                db.Publishers.Remove(publisher);
                db.SaveChanges();

                result.Type = ResultWeb.ResultType.OK_DELETE;
            }
            else
            {
                result.Type = ResultWeb.ResultType.FIELD_INVALID;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private void UpdateCountCategory(int? id)
        {
            if (id == null)
                return;

            Category cate = db.Categories.Find(id);
            cate.NumberOfProducts = cate.Products.Count;
            db.Entry(cate).State = EntityState.Modified;
            db.SaveChanges();
        }

        private void UpdateCountAuthor(int? id)
        {
            if (id == null)
                return;

            Author author = db.Authors.Find(id);
            author.NumberOfBooks = author.Books.Count;
            db.Entry(author).State = EntityState.Modified;
            db.SaveChanges();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.Request.IsAuthenticated)
            {
                if (string.IsNullOrEmpty(returnUrl))
                {
                    return RedirectToAction("Index");
                }
                else
                {
                    return Redirect(returnUrl);
                }
            }

            var model = new LoginViewModel();

            model.ReturnUrl = returnUrl;

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var isAuthenticated = HttpContext.Request.IsAuthenticated;

                //var users = db.Users.ToList();
                //var user = users.First(x => x.Username == model.Email);

                var user = db.Users.Where(u => u.Username == model.Email).FirstOrDefault();

                if (user != null)
                {
                    if (user.Password == model.Password)
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, false);

                        var authTicket = new FormsAuthenticationTicket(1, user.Username, DateTime.Now, DateTime.Now.AddMinutes(20), false, user.Role.Name);
                        var encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                        var authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                        HttpContext.Response.Cookies.Add(authCookie);

                        if (string.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            return Redirect(returnUrl);
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("Password", "Sai mật khẩu.");
                    }
                }
                else
                {
                    ModelState.AddModelError("Email", "Không tìm thấy người dùng.");
                }
            }

            return View(model);
        }

        [Authorize(Roles = "Admin,ThuNgan")]
        public ActionResult UserProfile()
        {
            if (User.IsInRole("Admin"))
            {
                ViewBag.IsAdmin = true;
            }
            else
            {
                ViewBag.IsAdmin = false;
            }
            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }


        // --- CATEGORY ------------------------------------------------------------------------- //
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ListCategory()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        public ActionResult GetListCategory()
        {
            return new JsonNetResult
            {
                Data = db.Categories.ToList(),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };
        }

        public ActionResult GetListType()
        {
            return Json(HtmlExtensions.GenerateDropdownEnum<ProductType>(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult DropdownListCategory()
        {
            var list = db.Categories.ToDictionary(m => m.ID, n => n.Name).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult EditCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                db.Entry(model).State = EntityState.Modified;
                db.SaveChanges();
                return Json("Sửa thành công");
            }
            return Json("Có lỗi xảy ra khi sửa");
        }

        [HttpPost] 
        public ActionResult AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(model);
                db.SaveChanges();
                return Json("Thêm thành công");
            }
            return Json("Có lỗi xảy ra khi thêm");
        }

        [HttpPost]
        public ActionResult DeleteCategory(int oldCategoryID, int newCategoryID)
        {
            if (ModelState.IsValid)
            {
                if (oldCategoryID == newCategoryID)
                {
                    return Json("Phải chọn danh mục thay thế");
                }

                Category oldCategory = db.Categories.Find(oldCategoryID);
                Category newCategory = db.Categories.Find(newCategoryID);

                var list = db.Products.Where(m => m.CategoryId == oldCategoryID);
                foreach (var item in list)
                {
                    item.CategoryId = newCategory.ID;
                    db.Entry(item).State = EntityState.Modified;
                }
                db.SaveChanges();

                db.Categories.Remove(oldCategory);
                db.SaveChanges();
                return Json("Xóa thành công");
            }
            return Json("Có lỗi xảy ra khi xóa");
        }



        // --- IMPORT ---------------------------------------------------------------------------- //
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult ListImport()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult AddImport()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();
            return View();
        }

        public ActionResult AddImport(ProductImport model)
        {
            if (ModelState.IsValid)
            {
                db.ProductImports.Add(model);
                db.SaveChanges();

                foreach (ImportDetail detail in model.ImportDetails)
                {
                    db.Products.Find(detail.ProductId).InStock += detail.Quantity;
                }
                db.SaveChanges();

                return Json("Lập phiếu nhập thành công");
            }
            return Json("Lập phiếu nhập thất bại");
        }

        public ActionResult GetListStatus()
        {
            List<Pair> JsonList = HtmlExtensions.GenerateDropdownEnumWithAny<ImportStatus>();
            return Json(JsonList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListStatusName()
        {
            List<Pair> JsonList = HtmlExtensions.GenerateDropdownEnum<ImportStatus>();
            return Json(JsonList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetListImport(int importStatus)
        {
            List<ProductImport> result = (importStatus == -1) ?
                db.ProductImports.ToList() :
                db.ProductImports.Where(m => (int)m.Status == importStatus).ToList();

            return new JsonNetResult
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 2
                }
            };
        }

        public ActionResult GetImportDetail(int id)
        {
            return new JsonNetResult
            {
                Data = db.ProductImports.Find(id),
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                Settings = {
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    MaxDepth = 3
                }
            };
        }

        [HttpPost]
        public ActionResult CancelImport(int model)
        {
            if (ModelState.IsValid)
            {
                ProductImport import = db.ProductImports.Find(model);
                import.Status = ImportStatus.Cancelled;

                foreach (ImportDetail detail in import.ImportDetails)
                {
                    db.Products.Find(detail.ProductId).InStock -= detail.Quantity;
                }
                db.SaveChanges();
                return Json("Hủy phiếu nhập hàng thành công");
            }
            return Json("Có lỗi khi hủy phiếu nhập hàng");
        }


        // not currently used
        #region statistic

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Finance()
        {
            if (!User.IsInRole("Admin"))
            {
                return RedirectToAction("Error"
                        , new
                        {
                            errorCode = "Lỗi 403."
                            ,
                            errorDetail = "Bạn không có quyền truy cập"
                        });
            }
            ViewBag.IsAdmin = true;

            ViewBag.User = db.Users.Where(u => u.Username == User.Identity.Name).FirstOrDefault();

            return View();
        }

        #endregion
    }
}