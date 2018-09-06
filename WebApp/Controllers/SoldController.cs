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

        // GET: Sold
        public ActionResult Buy(int? id, int amount)
        {
            var a = amount;
            if ( Session["u_id"] != null)
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
                        Date = DateTime.UtcNow,
                        P_name = prod.Pname
                    };
                    sold_db.Sold_products.Add(product);
                    var flag = sold_db.SaveChanges();
                    prod.Amount = prod.Amount - 1;
                    product_db.Entry(prod).State = System.Data.Entity.EntityState.Modified;
                    product_db.SaveChanges();
                    //var cntrlr = Controllers.CartController();
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