using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebContract;

namespace WebApp.Controllers
{
    public class CompaniesController : Controller
    {
        private MyDBCompanyEntities db = new MyDBCompanyEntities();
        private MyDBProductEntities product_db = new MyDBProductEntities();

        // GET: Companies
        public ActionResult Index()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: Companies/Details/5
        public ActionResult Profile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            CompanyProductView c_p_view = new CompanyProductView
            {
                CompanyView = db.Companies.Find(id),
                ProductView = product_db.Products.Where(x => x.Cid == id).ToList()
            };

            if (c_p_view.CompanyView == null)
            {
                return HttpNotFound();
            }

            return View(c_p_view);
        }

        // GET: Companies/Create
        public ActionResult Signup()
        {
            if (Session["c_id"] != null)
            {
                return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
            }
            return View();
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup([Bind(Exclude = "Avatar")] Company company, HttpPostedFileBase Avatar)
        {
            CompanyContract c = new CompanyContract
            {
                Id = company.Id,
                Cname = company.Cname,
                Ctel = company.Ctel,
                Caddress = company.Caddress,
                Password = company.Password,
                Email = company.Email,
                Avatar = company.Avatar
            };
            var flag = db.Companies.Any(x => x.Email == company.Email);
            if (!flag)
            {
                if (Avatar != null)
                {
                    var length = Avatar.InputStream.Length;
                    MemoryStream target = new MemoryStream();
                    Avatar.InputStream.CopyTo(target);
                    company.Avatar = target.ToArray();
                }
                if (ModelState.IsValid)
                {
                    company.Status = 1;
                    db.Companies.Add(company);
                    db.SaveChanges();
                    Session["c_id"] = company.Id;
                    Session["name"] = company.Cname;
                    return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
                }

                return View(c);
            }
            else
            {
                TempData["Error"] = "The email address already exist, please login...";
                return View(c);
            }
            
        }

        // GET: Companies/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company c = db.Companies.Find(id);
            CompanyContract company = new CompanyContract
            {
                Caddress = c.Caddress,
                Cname = c.Cname,
                Ctel = c.Ctel,
                Id = c.Id,
                Email = c.Email,
                Password = c.Password
            };

            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Cname,Ctel,Caddress,Password,Email")] Company c, HttpPostedFileBase Avatar)
        {
            CompanyContract company = new CompanyContract
            {
                Caddress = c.Caddress,
                Cname = c.Cname,
                Ctel = c.Ctel,
                Id = c.Id,
                Email = c.Email,
                Password = c.Password
            };
          
            if (Avatar != null)
            {
                var length = Avatar.InputStream.Length;
                MemoryStream target = new MemoryStream();
                Avatar.InputStream.CopyTo(target);
                c.Avatar = target.ToArray();
            }
            if (ModelState.IsValid)
            {
                db.Entry(c).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile", "Companies", new { id = (int)c.Id });
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public ActionResult Delete(int? id)
        {
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Company company = db.Companies.Find(id);
            if (company == null)
            {
                return HttpNotFound();
            }
            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["c_id"] == null)
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Account");
            }
            else
            {
                Company company = db.Companies.Find(id);
                List<Product> prod = product_db.Products.Where(x => x.Cid == company.Id).ToList();
                foreach(var item in prod)
                {
                    item.Status = 0;
                }               
                product_db.SaveChanges();
                company.Status = 0;
                db.Entry(company).State = EntityState.Modified;
                db.SaveChanges();
                Session.Abandon();
                return RedirectToAction("Index", "Account");
            }
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
