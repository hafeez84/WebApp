using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Controllers;
using System.Data.Entity;
using System.Text.RegularExpressions;

namespace WebApp.Controllers
{
    public class SoldController : Controller
    {
        private WepAppMyDBEntities ent = new WepAppMyDBEntities();

        // GET: Sold
        public ActionResult Buy(string p_id, string amount)
        {
            if ( Session["u_id"] != null)
            {
                int id = Convert.ToInt32(p_id);
                int a_int = Convert.ToInt32(amount);
                if (id != 0)
                {
                    var prod = ent.Products.FirstOrDefault(x => x.Id == id);
                    int i_int = (int)Session["u_id"];

                    Sold_products product = new Sold_products
                    {
                        P_id = prod.Id,
                        U_id = i_int,
                        C_id = (int)prod.Cid,
                        Date = DateTime.UtcNow,
                        P_name = prod.Pname
                    };
                    ent.Sold_products.Add(product);
                    var flag = ent.SaveChanges();
                    prod.Amount = prod.Amount - a_int;
                    ent.Entry(prod).State = System.Data.Entity.EntityState.Modified;
                    ent.SaveChanges();

                    //Fromcart(prod.Id, prod.Pname, prod.Amount + a_int);
                    var str = Request.Cookies["cart"].Value.ToString();
                    var id_s = prod.Id.ToString() + "," + prod.Pname + "," + (prod.Amount + a_int).ToString();
                    var replace = "";

                    var res = Regex.Replace(str, id_s, replace);
                    Response.Cookies["cart"].Value = res;

                    //var c_ps = Request.Cookies["cart"].Value.ToString().Split('|');
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
                    if(cart_pros.ProductsV.Count > 0)
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
                    TempData["Error"] = "There was an error, please try again...";
                    return RedirectToAction("Profile", "Users", new { id = (int)Session["u_id"] });
                }
            }
            else
            {
                return RedirectToAction("Index", "Account");
            }
            
        }
    }
}