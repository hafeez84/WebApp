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
        private MyDBProductEntities db = new MyDBProductEntities();

        public ActionResult ToCart(int? id)
        {
            if (id != null)
            {
                var prod = db.Products.SingleOrDefault(x => x.Id == id);
                string prod_id = prod.Id.ToString();
                if (Request.Cookies["cart"] == null)
                {
                    Response.Cookies["cart"].Value = prod_id;
                }
                else
                {
                    string[] p = Request.Cookies["cart"].Value.ToString().Split(',');
                    foreach (var i in p)
                    {
                        int i_int = Convert.ToInt32(i);
                        if (id == i_int)
                        {
                            TempData["Error"] = "The product already exist in your Basket... ";
                            return RedirectToAction("Index", "Products");
                        }
                    }
                    Response.Cookies["cart"].Value = Request.Cookies["cart"].Value + "," + prod_id;
                }
                Response.Cookies["cart"].Expires = DateTime.Now.AddMonths(3);
                TempData["Success"] = "Product has been added to your Cart successfully !";
                return RedirectToAction("Details", "Products", new { id = prod.Id });
            }
            else
            {
                TempData["Error"] = "Spmething went wrong ! Please try again... ";
                return RedirectToAction("Index", "Products");
            }

        }

        public ActionResult FromCart(int? id)
        {
            if (id != null)
            {
                string str = Request.Cookies["cart"].Value.ToString();
                var id_s = id.ToString();
                var start = id_s + ",";
                var replace = "";
                var between = "," + id_s + ",";
                var replace_between = ",";
                var end = "," + id_s;

                var res = Regex.Replace(str, start, replace);
                if (res.Equals(str))
                {
                    var res1 = Regex.Replace(str, between, replace_between);
                    if (res1.Equals(str))
                    {
                        var res2 = Regex.Replace(str, end, replace);
                        if (res2.Equals(str))
                        {
                            Response.Cookies["cart"].Expires = DateTime.Now.AddDays(-1);
                            return RedirectToAction("Index", "Products");
                        }
                        else
                        {
                            Response.Cookies["cart"].Value = res2;
                            return RedirectToAction("Index", "Products");
                        }
                    }
                    else
                    {
                        Response.Cookies["cart"].Value = res1;
                        return RedirectToAction("Index", "Products");
                    }
                }
                else
                {
                    Response.Cookies["cart"].Value = res;
                    return RedirectToAction("Index", "Products");
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
            if (Request.Cookies["cart"] != null)
            {
                string[] p = Request.Cookies["cart"].Value.ToString().Split(',');
                List<Product> temp = new List<Product>();
                foreach (var i in p)
                {
                    int i_int = Convert.ToInt32(i);
                    temp.Add(db.Products.Where(x => x.Id == i_int).FirstOrDefault());
                }

                UserProducts cart_pros = new UserProducts
                {
                    ProductsV = temp
                };

                return PartialView("~/Views/Products/_Cart.cshtml", cart_pros);
            }
            else
            {
                TempData["Error"] = "Your basket is Empty !";
                return RedirectToAction("Index", "Products");
            }
        }

    }
}