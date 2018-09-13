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
        private WepAppMyDBEntities ent = new WepAppMyDBEntities();

        // Cached these foo 2 mins to not keep connecting ent
        [OutputCache(CacheProfile = "Cache2min")]
        public List<Brand> GetBrandlist()
        {
            return ent.Brands.ToList();
        }

        [OutputCache(CacheProfile = "Cache2min")]
        public List<Category> GetCategorylist()
        {
            return ent.Categories.ToList();
        }

        [OutputCache(CacheProfile = "Cache2min")]
        public List<Model> GetModellist()
        {
            return ent.Models.ToList();
        }


        public ActionResult Index()
        {
            ProductsView products = new ProductsView
            {
                Products = ent.Products.ToList(),
                Product_b = GetBrandlist(),
                Categories = GetCategorylist(),
                Product_m = GetModellist(),
                P_Photos = ent.P_photo.ToList()
            };
            var c_ps = products.Products.Where(x => x.Amount <= 5 && x.Status != 0).ToList();
            products.Carousel_ps = c_ps;

            return View(products);
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            List<Category> temp = new List<Category>();
            temp = GetCategorylist();
            List<Brand> temp_br = new List<Brand>();
            temp_br = GetBrandlist();
            ProductsView pv = new ProductsView
            {
                Categories = temp,
                Product_b = temp_br
            };

            CompanyProductUpload product = new CompanyProductUpload
            {
                ProductM = ent.Products.Find(id),
                Categories = pv
            };
            
            
            if (product.ProductM == null)
            {
                return HttpNotFound();
            }
            product.P_Comments = ent.Comments.Where(x => x.P_id == product.ProductM.Id).ToList();
            product.BrandM = ent.Brands.SingleOrDefault(x=>x.Id == product.ProductM.B_id);
            product.CategoryM = ent.Categories.SingleOrDefault(x => x.Id == product.BrandM.Cate_id);
            product.ProductModelM = ent.Models.SingleOrDefault(x=>x.Id == product.ProductM.M_id);
            product.P_photos = ent.P_photo.Where(x=>x.P_id == product.ProductM.Id).ToList();

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
                if(ent.Models.Any(x=>x.Name == model.Name)) // if model with same name exist do not create new one
                    model = ent.Models.FirstOrDefault(x => x.Name == model.Name);
                else
                {
                    ent.Models.Add(model);
                    ent.SaveChanges();
                }

                category.Name = upload.CategoryM.Name.Trim().ToLower(); // same as model
                if(ent.Categories.Any(x=>x.Name == category.Name))
                {
                    category = ent.Categories.FirstOrDefault(x => x.Name == category.Name);
                }
                else
                {
                    ent.Categories.Add(category);
                    ent.SaveChanges();
                }

                brand.Name = upload.BrandM.Name.Trim().ToLower();
                if(ent.Brands.Any(x=>x.Name == brand.Name)) //same
                {
                    brand = ent.Brands.FirstOrDefault(x => x.Name == brand.Name);
                }
                else
                {
                    brand.Cate_id = category.Id;
                    ent.Brands.Add(brand);
                    ent.SaveChanges();
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
                ent.Products.Add(product);
                ent.SaveChanges();

                P_photo _Photo = new P_photo();
                if(Photo != null)
                {

                    var length = Photo.InputStream.Length;
                    MemoryStream target = new MemoryStream();
                    Photo.InputStream.CopyTo(target);
                    _Photo.Photo = target.ToArray();

                    _Photo.P_id = product.Id;
                    ent.P_photo.Add(_Photo);
                    ent.SaveChanges();
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
                ProductM = ent.Products.Find(id),
            };
            
            if (product.ProductM == null)
            {
                return HttpNotFound();
            }
            product.BrandM = ent.Brands.SingleOrDefault(x => x.Id == product.ProductM.B_id);
            product.ProductModelM = ent.Models.SingleOrDefault(x => x.Id == product.ProductM.M_id);
            product.CategoryM = ent.Categories.SingleOrDefault(x => x.Id == product.BrandM.Cate_id);
            product.P_photos = ent.P_photo.Where(x => x.P_id == product.ProductM.Id).ToList();
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
                if (!ent.Categories.Any(x => x.Name == cat_n))
                {
                    ent.Categories.Add(product.CategoryM);
                    ent.SaveChanges();
                    product.BrandM.Cate_id = product.CategoryM.Id;
                }

                var brand_n = product.BrandM.Name.Trim().ToLower(); // same as up
                if(!ent.Brands.Any(x=>x.Name == brand_n))
                {
                    ent.Brands.Add(product.BrandM);
                    ent.SaveChanges();
                    product.ProductM.B_id = product.BrandM.Id;
                }

                var model_n = product.ProductModelM.Name.Trim().ToLower(); // same as up
                if(!ent.Models.Any(x=>x.Name == model_n))
                {
                    ent.Models.Add(product.ProductModelM);
                    ent.SaveChanges();
                    product.ProductM.M_id = product.ProductModelM.Id;
                }

                if ( _Photo.Photo != null)
                {
                    if(ent.P_photo.Any(x=>x.P_id == product.ProductM.Id))
                    {
                        var p = ent.P_photo.SingleOrDefault(x => x.P_id == product.ProductM.Id);
                        p.Photo = _Photo.Photo;
                        ent.Entry(p).State = EntityState.Modified;
                        ent.SaveChanges();
                    }
                    else
                    {
                        P_photo ph = new P_photo
                        {
                            P_id = product.ProductM.Id,
                            Photo = _Photo.Photo
                        };
                        ent.P_photo.Add(ph);
                        ent.SaveChanges();
                    }
                    
                }

                ent.Entry(product.ProductM).State = EntityState.Modified;
                ent.SaveChanges();
                return RedirectToAction("Profile", "Companies", new { id = (int) Session["c_id"] });
            }
            return View(product);
        }

        public ActionResult Delete(int id)
        {
            if(Session["c_id"] != null)
            {
                int s_id = (int)Session["c_id"];
                if(ent.Products.Any(x=>x.Id == id))
                {
                    var product = ent.Products.Find(id);
                    if(product.Cid == s_id)
                    {
                        product.Status = 0;
                        ent.Entry(product).State = EntityState.Modified;
                        ent.SaveChanges();
                        TempData["Success"] = "Product has been deleted successfully...";
                        return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
                    }
                    else
                    {
                        TempData["Error"] = "You can only delete your own products !";
                        return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
                    }
                    
                }
                else
                {
                    TempData["Error"] = "There isn't such product !";
                    return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
                }

            }
            else
            {
                TempData["Error"] = "You can't perform this action !";
                return RedirectToAction("Index", "Account");
            }
            
        }

        public ActionResult Categories(string name)
        {
            Category cat = ent.Categories.FirstOrDefault(x=>x.Name == name);
            List<Category> cats = new List<Category>();
            cats.Add(cat);
            var brands = ent.Brands.Where(x => x.Cate_id == cat.Id).ToList();
            List<Product> products_temp = new List<Product>();
            List<Model> model_temp = new List<Model>();
            List<P_photo> photos_temp = new List<P_photo>();

            foreach (var i in brands)
            {
                var temp = ent.Products.Where(x => x.B_id == i.Id).ToList();
                products_temp.AddRange(temp);
            }

            foreach (var p in products_temp)
            {
                var temp = ent.Models.Find(p.M_id);
                if(temp != null)
                {
                    model_temp.Add(temp);
                }
                var temp1 = ent.P_photo.FirstOrDefault(x => x.P_id == p.Id);
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
            var c_ps = ent.Products.Where(x => x.Amount <= 5 && x.Status != 0).ToList();
            products.Carousel_ps = c_ps;
            return View("Index", products);
        }

        public ActionResult Brands (string name)
        {
            var brand_temp = ent.Brands.FirstOrDefault(x=>x.Name == name);
            List<Brand> brand_t = new List<Brand>();
            brand_t.Add(brand_temp);
            var products_temp = ent.Products.Where(x => x.B_id == brand_temp.Id).ToList();
            List<Model> model_temp = new List<Model>();
            List<P_photo> photos_temp = new List<P_photo>();

            foreach (var p in products_temp)
            {
                var temp = ent.Models.Find(p.M_id);
                if (temp != null)
                {
                    model_temp.Add(temp);
                }
                var temp1 = ent.P_photo.FirstOrDefault(x => x.P_id == p.Id);
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
            var c_ps = ent.Products.Where(x => x.Amount <= 5 && x.Status != 0).ToList();
            products.Carousel_ps = c_ps;
            return View("Index", products);
        }

        public ActionResult Search(string s_str)
        {
            if (s_str != "")
            {
                s_str = s_str.Trim().ToLower();
                var ps = ent.Products.ToList();
                List<Product> res = new List<Product>();
                foreach (var i in ps)
                {
                    if (i.Pname.Contains(s_str) || i.Pname == s_str || i.Pdescription.Contains(s_str) || i.Pdescription == s_str)
                    {
                        res.Add(i);
                    }
                }

                if(res.Count >= 1)
                {
                    List<Model> model_temp = new List<Model>();
                    List<P_photo> photos_temp = new List<P_photo>();
                    List<Brand> brand_t = new List<Brand>();

                    foreach (var p in res)
                    {
                        var temp = ent.Models.FirstOrDefault(x => x.Id == p.M_id);
                        if (temp != null)
                        {
                            model_temp.Add(temp);
                        }
                        var temp1 = ent.P_photo.FirstOrDefault(x => x.P_id == p.Id);
                        if (temp1 != null)
                        {
                            photos_temp.Add(temp1);
                        }
                        var temp2 = ent.Brands.FirstOrDefault(x => x.Id == p.Id);
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
                else
                {
                    return Content("Such product doesn't exist !");
                }
            }
            else
            {
                return Content("You entered an empty string, please try again with a string...");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ent.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
