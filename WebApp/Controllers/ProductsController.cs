using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebContract;

namespace WebApp.Controllers
{
    public class ProductsController : Controller
    {
        private MyDBProductEntities db = new MyDBProductEntities();
        private MyDBCategoryEntities category_db = new MyDBCategoryEntities();
        private MyDBProductBrandEntities brand_db = new MyDBProductBrandEntities();
        private MyDBProductModelEntities product_model_db = new MyDBProductModelEntities();

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
                List<Product> temp = new List<Product>() ;
                foreach (var i in p)
                {
                    int i_int = Convert.ToInt32(i);
                    temp.Add(db.Products.Where(x => x.Id == i_int).FirstOrDefault());
                }

                ProductsView cart_pros = new ProductsView
                {
                    Products = temp
                };
                
                return PartialView("~/Views/Products/_Cart.cshtml", cart_pros);
            }
            else
            {
                TempData["Error"] = "Your basket is Empty !";
                return RedirectToAction("Index", "Products");
            }
        }

        // GET: Products
        public ActionResult Index()
        {
            ProductsView products = new ProductsView
            {
                Products = db.Products.ToList(),
                Product_b = GetBrandlist(),
                Product_c = GetCategorylist(),
                Product_m = GetModellist()
            };

            return View(products);
        }

        // Cached these foo 2 mins to not keep connecting db
        [OutputCache(CacheProfile = "Cache2min")]
        public List<Brand> GetBrandlist()
        {
            return brand_db.Brands.ToList();
        }

        [OutputCache(CacheProfile = "Cache2min")]
        public List<Category> GetCategorylist()
        {
            return category_db.Categories.ToList();
        }

        [OutputCache(CacheProfile = "Cache2min")]
        public List<Model> GetModellist()
        {
            return product_model_db.Models.ToList();
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You have to be login as a company to add a product !";
                return RedirectToAction("Index", "Account");
            }
            else
            {
                return View();
            }
        }

        // POST: Products/Create
     
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyProductUpload upload)
        {
            if (ModelState.IsValid)
            {
                Product product = new Product();
                Brand brand = new Brand();
                Category category = new Category();
                Model model = new Model();

                product.Cid = (int)Session["c_id"];
                product.Created_at = DateTime.UtcNow;
                product.Amount = upload.ProductM.Amount;
                product.Pname = upload.ProductM.Pname;
                product.Pdescription = upload.ProductM.Pdescription;
                db.Products.Add(product);
                db.SaveChanges();

                brand.P_id = product.Id;
                brand.Name = upload.BrandM.Name;
                brand_db.Brands.Add(brand);
                brand_db.SaveChanges();

                category.Brand_id = brand.Id;
                category.Name = upload.CategoryM.Name;
                category_db.Categories.Add(category);
                category_db.SaveChanges();

                model.P_id = product.Id;
                model.Name = upload.ProductModelM.Name;
                product_model_db.Models.Add(model);
                product_model_db.SaveChanges();

                return RedirectToAction("Index");
            }
            return View(upload);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must log in to perfum this action";
                return RedirectToAction("Index", "Account");
            }
            var s_id = (int)Session["c_id"];
            if ( s_id != product.Cid )
            {
                TempData["Error"] = "You can only edit your own products !";
                return RedirectToAction("Index", "Products");
            }
            
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( Product product)
        {
            product.Cid = (int)Session["c_id"];
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must log in to perfum this action";
                return RedirectToAction("Index", "Account");
            }
            var s_id = (int)Session["c_id"];
            if (s_id != product.Cid)
            {
                TempData["Error"] = "You can only delete your own products !";
                return RedirectToAction("Index", "Products");
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
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

            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
