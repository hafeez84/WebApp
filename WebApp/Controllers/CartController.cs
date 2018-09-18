using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class CartController : Controller
    {
        private WepAppMyDBEntities ent = new WepAppMyDBEntities();

        public ActionResult ToCart(int? id)
        {
            if (id != null && Session["c_id"] == null)
            {
                var prod = ent.Products.SingleOrDefault(x => x.Id == id);
                string prod_id = prod.Id.ToString();
                string prod_str = prod.Id.ToString();
                prod_str = prod_str + "," + prod.Pname.ToString();
                prod_str = prod_str + "," + prod.Amount.ToString();

                if (Request.Cookies["cart"] == null)
                {
                    Response.Cookies["cart"].Value = prod_str;
                }
                else
                {
                    string[] p = Request.Cookies["cart"].Value.ToString().Split('|');
                    foreach (var i in p)
                    {
                        if (i != "")
                        {
                            var p_a = i.Split(',');
                            int i_int = Convert.ToInt32(p_a[0]);
                            if (id == i_int)
                            {
                                TempData["Error"] = "The product already exist in your Basket... ";
                                return RedirectToAction("Index", "Products");
                            }
                        }
                    }
                    Response.Cookies["cart"].Value = Request.Cookies["cart"].Value + "|" + prod_str;
                }
                Response.Cookies["cart"].Expires = DateTime.Now.AddMonths(3);
                if (Session["u_id"] == null && TempData["Error"] == null)
                {
                    TempData["Warning"] = "Please login or sign up to proced buying the items...";
                }
                TempData["Success"] = "Successfully added to your cart...";
                //return RedirectToAction("Details", "Products", new { id = prod.Id });
                return RedirectToAction("Index", "Products");
            }
            else
            {
                TempData["Error"] = "Spmething went wrong ! Please try again... ";
                return RedirectToAction("Index", "Products");
            }

        }

        [HttpPost]
        public ActionResult FromCart(string id, string name, string amount)
        {
            if (id != null)
            {
                var str = Request.Cookies["cart"].Value.ToString();
                var id_s = id+ "," + name + "," + amount;
                var replace = "";

                var res = Regex.Replace(str, id_s, replace);
             
                Response.Cookies["cart"].Value = res;

                var c_ps = res.Split('|');
                List<Product> temp = new List<Product>();
                foreach (var i in c_ps)
                {
                    var p = i.Split(',');

                    if (p[0] != "")
                    {
                        int ii_int = Convert.ToInt32(p[0]);
                        int amountt = Convert.ToInt32(p[2]);
                        var new_p = new Product
                        {
                            Id = ii_int,
                            Pname = p[1],
                            Amount = amountt,
                            Status = 1
                        };
                        temp.Add(new_p);
                    }
                }

                UserProducts cart_pros = new UserProducts
                {
                    ProductsV = temp
                };
                if (cart_pros.ProductsV.Count > 0)
                {
                    return PartialView("~/Views/Users/_Cart.cshtml", cart_pros);
                }
                else
                {
                    return Content("You don't have any product in your cart at the time...");
                }
            }
            else
            {
                TempData["Error"] = "Spmething went wrong ! Please try again... ";
                return RedirectToAction("Index", "Products");
            }
        }
        public ActionResult Cart()
        {
            if (Session["u_id"] == null)
            {
                TempData["Error"] = "Please login to see your Cart...";
                return View("_Login");
            }
               else
                return null;
        }
    }
}