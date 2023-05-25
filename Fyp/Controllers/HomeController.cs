using Fyp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime;
using Microsoft.Ajax.Utilities;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace Fyp.Controllers
{
    public class HomeController : Controller
    {
        
       
        Model3 db = new Model3();
        public ActionResult IndexCustomer()
        {
            return View();
        }
        public ActionResult IndexAdmin()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(Admin admin)
        {
            Admin a = db.Admins.Where(X => X.ADMIN_EMAIL == admin.ADMIN_EMAIL && X.ADMIN_PASSWORD == admin.ADMIN_PASSWORD).FirstOrDefault();
            if (a != null) {
                Session["Admin"] = a;
                return RedirectToAction("IndexAdmin", "Home");
           }
            else
           {
                ViewBag.Message = "Invalid Email or Password";
                return View();
            }

        }
        public ActionResult Logout()
        {
            Session["Admin"] = null;
            return RedirectToAction("Login");
        }
        public ActionResult Feedback()
        {
            return View();
        }
        public ActionResult Cart()
        {
            return View();
        }
        public ActionResult Shop(int? id)
        {
            ShopModel s = new ShopModel();
            s.Cat = db.Categories.ToList();
            if (id == null)
            {
                s.Pro = db.Products.ToList();
            } else
                s.Pro = db.Products.Where(p => p.CATEGORY_FID == id).ToList();


            return View(s);
        }
        public ActionResult AddToCart(int id)
        {
            List<Product> list;
            if (Session["myCart"] == null)
            {
                list = new List<Product>();
            }
            else
            {
                list = (List<Product>)Session["myCart"];
            }
            Boolean isProductExist = false;
            foreach(var item in list)
            {
                if(id==item.PRODUCT_ID)

                {
                    isProductExist = true;
                    item.PRO_QUANTITY++;
                }
            }
            if(isProductExist==false)
            {
                list.Add(db.Products.Where(p => p.PRODUCT_ID == id).FirstOrDefault());
                list[list.Count - 1].PRO_QUANTITY = 1;
            }
           
           
            Session["myCart"] = list;                                     // grand global variable(state) that can be access anywhere
            return RedirectToAction("Cart");                             // in application// har qism ka data store kr skty hain 
                                                                         // we will access this variable in Cart,Payment,Bill,order booking page... 

        }
        public ActionResult MinusFromCart(int RowNo)
        {
            List<Product> list = (List<Product>)Session["myCart"];
            if (list[RowNo].PRO_QUANTITY <=1) {
                list[RowNo].PRO_QUANTITY=1; }
            else
            {
                list[RowNo].PRO_QUANTITY--;
                Session["myCart"] = list;
            }

            return RedirectToAction("Cart");
        }


        public ActionResult PlusToCart(int RowNo)
        {
            List<Product> list = (List<Product>)Session["myCart"];
            int P_ID = list[RowNo].PRODUCT_ID;
            int? available = db.OrderDetails.Where(x => x.PRODUCT_FID == P_ID).Sum(x => x.QUANTITY);
            if (available > list[RowNo].PRO_QUANTITY)
            { list[RowNo].PRO_QUANTITY++; }
            else 
            {
                TempData["Message"] = "You can only add followimg number of items: ";
            }


            Session["myCart"] = list;// grand global variable(state) that can be access anywhere
            // in application// har qism ka data store kr skty hain 
            // we will access this variable in Cart,Payment,Bill,order booking page... 
            return RedirectToAction("Cart");
        }
        public ActionResult RemoveFromCart(int RowNo)
        {
            List<Product> list = (List<Product>)Session["myCart"];
            list.RemoveAt(RowNo);
            Session["myCart"] = list;// grand global variable(state) that can be access anywhere
            // in application// har qism ka data store kr skty hain 
            // we will access this variable in Cart,Payment,Bill,order booking page... 
            return RedirectToAction("Cart");
        }
        public ActionResult Checkout()
        {
            return View();
        }
        public ActionResult PToBilling(Order o)
        {
            o.ORDER_DATE = System.DateTime.Now;
         
            o.ORDER_TYPE = "Sale";
            o.STATUS = "Active";
            if (Session["Customer"]!=null)
            {
                Customer c = (Customer)Session["Customer"];
                o.CUSTOMER_FID = c.CUSTOMER_ID;
            }

            Session["Order"] = o;
            if(o.ORDER_STATUS== "Cash On Delivery")
            {
                return RedirectToAction("OrderBooked");
            }
            else
            {
                return Redirect("https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_xclick&business=eternglam@gmail.com&item_name=Save EternGlamProducts!&return=http://localhost:63830/Home/OrderBooked&amount=" + double.Parse(Session["totalAmount"].ToString()) / 160);
            }

           
        }
        public ActionResult CheckoutF()
        {
            return RedirectToAction("Checkout", "Home");
        }
        public ActionResult OrderBooked()
        {
            Order o = (Order)Session["Order"];
              MailMessage mail = new MailMessage();
              mail.From = new MailAddress("eternglam@gmail.com");
              mail.To.Add(o.ORDER_EMAIL);
              mail.Subject = "Order Confirmation";
              mail.Body = "ThankYou!,<b>EternGlam</b>";
              mail.IsBodyHtml = true;

              SmtpClient smtpServer = new SmtpClient("smtp.gmail.com");
              smtpServer.Port = 587;
              smtpServer.Credentials = new NetworkCredential("eternglam@gmail.com", "asfdlbsmsmvzfggg");
              smtpServer.EnableSsl = true;
              smtpServer.Send(mail);



              /*code for sending sms
              String api = "https://lifetimesms.com/json?api_token=d5ac7403dd1e8ddb5c4038e4d1e07a4bf967578285&api_secret=eternGlam&to=" + o.ORDER_CONTACT + "&from=EternGlam&message=Thanks For Shopping here...";
              HttpWebRequest req = (HttpWebRequest)WebRequest.Create(api);
              var httpResponse = (HttpWebResponse)req.GetResponse();*/


            // code for savingorder in order and details table
            db.Orders.Add(o);
            db.SaveChanges();
            List<Product> p = (List<Product>)Session["myCart"];
                for (int i = 0; p.Count > i; i++)
            {
                OrderDetail od = new OrderDetail();
                int orderID = db.Orders.Max(x => x.ORDER_ID);
                od.ORDER_FID = orderID; 
                od.PRODUCT_FID = p[i].PRODUCT_ID;
                od.QUANTITY = p[i].PRO_QUANTITY * -1;
                od.PURCHASE_PRICE = p[i].PRODUCT_PURCHASEPRICE;
                od.SALE_PRICE = p[i].PRODUCT_SALEPRICE;
                db.OrderDetails.Add(od);
                db.SaveChanges();
            }
           


            return View();


        }
        public ActionResult CloseOrder()
        {
            Session["Order"] = null;
            Session["myCart"] = null;
            return RedirectToAction("IndexCustomer");
        }
       




    }
}