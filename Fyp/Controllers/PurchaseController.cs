using Fyp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Antlr.Runtime;

namespace Fyp.Controllers
{
    public class PurchaseController : Controller
    {
        Model3 db = new Model3();
        public ActionResult Cart()
        {
            return View();
        }
        public ActionResult Purchase(int? id)
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

            list.Add(db.Products.Where(p => p.PRODUCT_ID == id).FirstOrDefault());
            list[list.Count - 1].PRO_QUANTITY = 1;
            Session["myCart"] = list;                                     // grand global variable(state) that can be access anywhere
            return RedirectToAction("Cart");                             // in application// har qism ka data store kr skty hain 
                                                                         // we will access this variable in Cart,Payment,Bill,order booking page... 

        }
        public ActionResult MinusFromCart(int RowNo)
        {
            List<Product> list = (List<Product>)Session["myCart"];
            if (list[RowNo].PRO_QUANTITY <= 1)
            {
                list[RowNo].PRO_QUANTITY = 1;
            }
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
            list[RowNo].PRO_QUANTITY+=3;
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
        public ActionResult PurchaseNow(Order o)
        {
            o.ORDER_DATE = System.DateTime.Now;
            o.ORDER_STATUS = "Paid";
            o.ORDER_TYPE = "Purchase";

            Session["Order"] = o;
            return RedirectToAction ("OrderBooked");
        }
        public ActionResult CheckoutA()
        {
            return RedirectToAction("Checkout", "Purchase");
        }
        public ActionResult OrderBooked()
        {
            Order o = (Order)Session["Order"];
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
                od.QUANTITY = p[i].PRO_QUANTITY;
                od.PURCHASE_PRICE = p[i].PRODUCT_PURCHASEPRICE;
                od.SALE_PRICE = p[i].PRODUCT_SALEPRICE;
                db.OrderDetails.Add(od);
                db.SaveChanges();
                }
          

            return View();


        }
}
}