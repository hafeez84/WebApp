using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebApp.Controllers;

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
                TempData["Error"] = "Spmething went wrong ! Please try again... ";
        }
        private void DeleteConfirmed(int id)
        {
            Brand brand = brand_db.Brands.Where(x => x.P_id == id).FirstOrDefault();

            Category category = category_db.Categories.Where(x => x.Brand_id == brand.Id).FirstOrDefault();
            category_db.Categories.Remove(category);
            category_db.SaveChanges();


            brand_db.Brands.Remove(brand);
            brand_db.SaveChanges();

            Model model = product_model_db.Models.Where(x => x.P_id == id).FirstOrDefault();
            product_model_db.Models.Remove(model);
            product_model_db.SaveChanges();

            Product product = product_db.Products.Find(id);
            product_db.Products.Remove(product);
            product_db.SaveChanges();
        }




        // GET: Sold
        public ActionResult Buy(int? id)
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
    }
}