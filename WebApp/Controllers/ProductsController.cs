using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
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
        private MyDBP_photoEntities P_photo_db = new MyDBP_photoEntities();
        private MyDBCommentEntities comment_db = new MyDBCommentEntities();

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


        public ActionResult Index()
        {
            ProductsView products = new ProductsView
            {
                Products = db.Products.ToList(),
                Product_b = GetBrandlist(),
                Categories = GetCategorylist(),
                Product_m = GetModellist(),
                P_Photos = P_photo_db.P_photo.ToList()
            };
            var c_ps = products.Products.Where(x => x.Amount <= 5).ToList();
            products.Carousel_ps = c_ps;

            return View(products);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CompanyProductUpload product = new CompanyProductUpload
            {
                ProductM = db.Products.Find(id)
            };
            if (product.ProductM == null)
            {
                return HttpNotFound();
            }
            product.P_Comments = comment_db.Comments.Where(x => x.P_id == product.ProductM.Id).ToList();
            product.BrandM = brand_db.Brands.SingleOrDefault(x=>x.Id == product.ProductM.B_id);
            product.CategoryM = category_db.Categories.SingleOrDefault(x => x.Id == product.BrandM.Cate_id);
            product.ProductModelM = product_model_db.Models.SingleOrDefault(x=>x.Id == product.ProductM.M_id);
            product.P_photos = P_photo_db.P_photo.Where(x=>x.P_id == product.ProductM.Id).ToList();

            return View(product);
        }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyProductUpload upload, HttpPostedFileBase Photo)
        {
            
            if (ModelState.IsValid)
            {
                Product product = new Product();
                Brand brand = new Brand();
                Category category = new Category();
                Model model = new Model();

                model.Name = upload.ProductModelM.Name.Trim().ToLower();
                if(product_model_db.Models.Any(x=>x.Name == model.Name)) // if model with same name exist do not create new one
                    model = product_model_db.Models.FirstOrDefault(x => x.Name == model.Name);
                else
                {
                    product_model_db.Models.Add(model);
                    product_model_db.SaveChanges();
                }

                category.Name = upload.CategoryM.Name.Trim().ToLower(); // same as model
                if(category_db.Categories.Any(x=>x.Name == category.Name))
                {
                    category = category_db.Categories.FirstOrDefault(x => x.Name == category.Name);
                }
                else
                {
                    category_db.Categories.Add(category);
                    category_db.SaveChanges();
                }

                brand.Name = upload.BrandM.Name.Trim().ToLower();
                if(brand_db.Brands.Any(x=>x.Name == brand.Name)) //same
                {
                    brand = brand_db.Brands.FirstOrDefault(x => x.Name == brand.Name);
                }
                else
                {
                    brand.Cate_id = category.Id;
                    brand_db.Brands.Add(brand);
                    brand_db.SaveChanges();
                }

                product.Cid = (int)Session["c_id"];
                product.Created_at = DateTime.UtcNow;
                product.Amount = upload.ProductM.Amount;
                product.Pname = upload.ProductM.Pname;
                product.Pdescription = upload.ProductM.Pdescription;
                product.B_id = brand.Id;
                product.M_id = model.Id;
                product.Price = upload.ProductM.Price;
                product.Status = 1;
                db.Products.Add(product);
                db.SaveChanges();

                P_photo _Photo = new P_photo();
                if(Photo != null)
                {

                    var length = Photo.InputStream.Length;
                    MemoryStream target = new MemoryStream();
                    Photo.InputStream.CopyTo(target);
                    _Photo.Photo = target.ToArray();

                    _Photo.P_id = product.Id;
                    P_photo_db.P_photo.Add(_Photo);
                    P_photo_db.SaveChanges();
                }

                return RedirectToAction("Index");
            }
            return View(upload);
        }

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            CompanyProductUpload product = new CompanyProductUpload
            {
                ProductM = db.Products.Find(id),
            };
            
            if (product.ProductM == null)
            {
                return HttpNotFound();
            }
            product.BrandM = brand_db.Brands.SingleOrDefault(x => x.Id == product.ProductM.B_id);
            product.ProductModelM = product_model_db.Models.SingleOrDefault(x => x.Id == product.ProductM.M_id);
            product.CategoryM = category_db.Categories.SingleOrDefault(x => x.Id == product.BrandM.Cate_id);
            product.P_photos = P_photo_db.P_photo.Where(x => x.P_id == product.ProductM.Id).ToList();
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must log in to perfum this action";
                return RedirectToAction("Index", "Account");
            }
            var s_id = (int)Session["c_id"];
            if ( s_id != product.ProductM.Cid )
            {
                TempData["Error"] = "You can only edit your own products !";
                return RedirectToAction("Index", "Products");
            }
            
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit( CompanyProductUpload product, HttpPostedFileBase Photo)
        {
            P_photo _Photo = new P_photo();
            if (Photo != null)
            {

                var length = Photo.InputStream.Length;
                MemoryStream target = new MemoryStream();
                Photo.InputStream.CopyTo(target);
                _Photo.Photo = target.ToArray();
            }

            if (ModelState.IsValid)
            {

                var cat_n = product.CategoryM.Name.Trim().ToLower(); // if there is category with that name
                if (!category_db.Categories.Any(x => x.Name == cat_n))
                {
                    category_db.Categories.Add(product.CategoryM);
                    category_db.SaveChanges();
                    product.BrandM.Cate_id = product.CategoryM.Id;
                }

                var brand_n = product.BrandM.Name.Trim().ToLower(); // same as up
                if(!brand_db.Brands.Any(x=>x.Name == brand_n))
                {
                    brand_db.Brands.Add(product.BrandM);
                    brand_db.SaveChanges();
                    product.ProductM.B_id = product.BrandM.Id;
                }

                var model_n = product.ProductModelM.Name.Trim().ToLower(); // same as up
                if(!product_model_db.Models.Any(x=>x.Name == model_n))
                {
                    product_model_db.Models.Add(product.ProductModelM);
                    product_model_db.SaveChanges();
                    product.ProductM.M_id = product.ProductModelM.Id;
                }

                if ( _Photo.Photo != null)
                {
                    if(P_photo_db.P_photo.Any(x=>x.P_id == product.ProductM.Id))
                    {
                        var p = P_photo_db.P_photo.SingleOrDefault(x => x.P_id == product.ProductM.Id);
                        p.Photo = _Photo.Photo;
                        P_photo_db.Entry(p).State = EntityState.Modified;
                        P_photo_db.SaveChanges();
                    }
                    else
                    {
                        P_photo ph = new P_photo
                        {
                            P_id = product.ProductM.Id,
                            Photo = _Photo.Photo
                        };
                        P_photo_db.P_photo.Add(ph);
                        P_photo_db.SaveChanges();
                    }
                    
                }

                db.Entry(product.ProductM).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile", "Companies", new { id = (int) Session["c_id"] });
            }
            return View(product);
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var product = db.Products.Find(id);
            product.Status = 0;
            db.Entry(product).State = EntityState.Modified;
            db.SaveChanges();
            TempData["Success"] = "Product has been deleted successfully...";
            return RedirectToAction("Profile", "Companies", new { id = (int) Session["c_id"] });
        }

        public ActionResult Categories(string name)
        {
            Category cat = category_db.Categories.FirstOrDefault(x=>x.Name == name);
            List<Category> cats = new List<Category>();
            cats.Add(cat);
            var brands = brand_db.Brands.Where(x => x.Cate_id == cat.Id).ToList();
            List<Product> products_temp = new List<Product>();
            List<Model> model_temp = new List<Model>();
            List<P_photo> photos_temp = new List<P_photo>();

            foreach (var i in brands)
            {
                var temp = db.Products.Where(x => x.B_id == i.Id).ToList();
                products_temp.AddRange(temp);
            }

            foreach (var p in products_temp)
            {
                var temp = product_model_db.Models.Find(p.M_id);
                if(temp != null)
                {
                    model_temp.Add(temp);
                }
                var temp1 = P_photo_db.P_photo.FirstOrDefault(x => x.P_id == p.Id);
                if (temp1 != null)
                {
                    photos_temp.Add(temp1);
                }
            }

            ProductsView products = new ProductsView
            {
                Product_c = cats,
                Product_b= brands,
                Product_m = model_temp,
                Products = products_temp,
                P_Photos = photos_temp,
                Categories = GetCategorylist()
            };
            var c_ps = db.Products.Where(x => x.Amount <= 5);
            products.Carousel_ps = c_ps;
            return View("Index", products);
        }

        public ActionResult Brands (string name)
        {
            var brand_temp = brand_db.Brands.FirstOrDefault(x=>x.Name == name);
            List<Brand> brand_t = new List<Brand>();
            brand_t.Add(brand_temp);
            var products_temp = db.Products.Where(x => x.B_id == brand_temp.Id).ToList();
            List<Model> model_temp = new List<Model>();
            List<P_photo> photos_temp = new List<P_photo>();

            foreach (var p in products_temp)
            {
                var temp = product_model_db.Models.Find(p.M_id);
                if (temp != null)
                {
                    model_temp.Add(temp);
                }
                var temp1 = P_photo_db.P_photo.FirstOrDefault(x => x.P_id == p.Id);
                if (temp1 != null)
                {
                    photos_temp.Add(temp1);
                }
            }

            ProductsView products = new ProductsView
            {
                Products = products_temp,
                Product_b = brand_t,
                Product_m = model_temp,
                P_Photos = photos_temp,
                Categories = GetCategorylist()
            };
            var c_ps = db.Products.Where(x => x.Amount <= 5);
            products.Carousel_ps = c_ps;
            return View("Index", products);
        }

        public ActionResult Search(string s_str)
        {
            s_str = s_str.Trim().ToLower();
            var ps = db.Products.ToList();
            List<Product> res = new List<Product>();
            foreach(var i in ps)
            {
                if (i.Pname.Contains(s_str) || i.Pname == s_str /*|| i.Pdescription.Contains(s_str)*/)
                {
                    res.Add(i);
                }
            }

            List<Model> model_temp = new List<Model>();
            List<P_photo> photos_temp = new List<P_photo>();
            List<Brand> brand_t = new List<Brand>();

            foreach (var p in res)
            {
                var temp = product_model_db.Models.FirstOrDefault(x=>x.Id == p.M_id);
                if (temp != null)
                {
                    model_temp.Add(temp);
                }
                var temp1 = P_photo_db.P_photo.FirstOrDefault(x => x.P_id == p.Id);
                if (temp1 != null)
                {
                    photos_temp.Add(temp1);
                }
                var temp2 = brand_db.Brands.FirstOrDefault(x => x.Id == p.Id);
                if (temp2 != null)
                {
                    brand_t.Add(temp2);
                }
            }

            ProductsView products = new ProductsView
            {
                Products = res,
                Product_b = brand_t,
                Product_m = model_temp,
                P_Photos = photos_temp,
            };
            return PartialView("_Products", products);
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
