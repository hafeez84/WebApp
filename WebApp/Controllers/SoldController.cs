using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Controllers;
using System.Data.Entity;

namespace WebApp.Controllers
{
    public class SoldController : Controller
    {
        private MyDBSoldEntities sold_db = new MyDBSoldEntities();
        private MyDBProductEntities product_db = new MyDBProductEntities();
        private MyDBCompanyEntities company_db = new MyDBCompanyEntities();
        private MyDBCategoryEntities category_db = new MyDBCategoryEntities();
        private MyDBProductBrandEntities brand_db = new MyDBProductBrandEntities();
        private MyDBProductModelEntities product_model_db = new MyDBProductModelEntities();

        private void DeleteConfirmed(int id)
        {
            var product = product_db.Products.Find(id);
            product.Status = 0;
            product_db.Entry(product).State = EntityState.Modified;
            product_db.SaveChanges();
        }


        private void FromCart(int? id)
        {
            if (id != null)
            {
                var str = Request.Cookies["cart"].Value.ToString();
                var id_s = id.ToString();
                var start = id_s + ",";
                var replace = "";
                var between = "," + id_s + ",";
                var replace_between = ",";
                var end = "," + id_s;

                var res = System.Text.RegularExpressions.Regex.Replace(str, start, replace);
                if (res.Equals(str))
                {
                    var res1 = System.Text.RegularExpressions.Regex.Replace(str, between, replace_between);
                    if (res1.Equals(str))
                    {
                        var res2 = System.Text.RegularExpressions.Regex.Replace(str, end, replace);
                        if (res2.Equals(str))
                            Response.Cookies["cart"].Expires = DateTime.Now.AddDays(-1);
                        else
                            Response.Cookies["cart"].Value = res2;
                    }
                    else
                        Response.Cookies["cart"].Value = res1;
                }
                else
                    Response.Cookies["cart"].Value = res;
            }
            else
                TempData["Error"] = "Something went wrong ! Please try again... ";
        }
   
        // GET: Sold
        public ActionResult Buy(int? id)
        {
            if( Session["u_id"] != null)
            {
                if (id != null)
                {
                    var prod = product_db.Products.FirstOrDefault(x => x.Id == id);
                    int i_int = (int)Session["u_id"];

                    Sold_products product = new Sold_products
                    {
                        P_id = prod.Id,
                        U_id = i_int,
                        C_id = (int)prod.Cid,
                        Date = DateTime.UtcNow
                    };

                    sold_db.Sold_products.Add(product);
                    var flag = sold_db.SaveChanges();
                    prod.Amount = prod.Amount - 1;
                    //if(prod.Amount == 0)
                    //{
                    //DeleteConfirmed(prod.Id);
                    //  var delete_prod = new WebApp.Controllers.ProductsController();
                    //delete_prod.DeleteConfirmed(prod.Id);
                    //}
                    //else
                    //{
                    product_db.Entry(prod).State = System.Data.Entity.EntityState.Modified;
                    product_db.SaveChanges();
                    FromCart(id);
                    //}
                    return RedirectToAction("Profile", "Users", new { id = i_int });
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