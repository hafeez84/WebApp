using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using WebContract;

namespace WebApp.Controllers
{
    public class UsersController : Controller
    {
        private WepAppMyDBEntities ent = new WepAppMyDBEntities();

        // GET: Users
        public ActionResult Index()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        }

        // GET: Users/Profile/5
        public ActionResult Profile(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            List<Product> temp = new List<Product>();
            if (Request.Cookies["cart"] != null)
            {
                var ps = Request.Cookies["cart"].Value.ToString().Split('|');
                foreach (var i in ps)
                {
                    if(i != "")
                    {
                        var p = i.Split(',');
                        int i_int = Convert.ToInt32(p[0]);
                        int amount = Convert.ToInt32(p[2]);
                        var new_p = new Product
                        {
                            Id = i_int,
                            Pname = p[1],
                            Amount = amount,
                            Status = 1
                        };
                        temp.Add(new_p);
                    }
                }
            }
            else
            {
                temp = null;
            }

            UserProducts user = new UserProducts
            {
                UserV = ent.Users.Find(id),
                ProductsV = temp,
                Bought_Prod = ent.Sold_products.Where(x => x.U_id == id).ToList()
        };

            if (user.UserV == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Signup
        public ActionResult Signup()
        {
            return PartialView("_USignup");
        }

        // POST: Users/Signup
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(HttpPostedFileBase Avatar ,[Bind(Exclude = "Avatar")]User u)
        {
            UserContract user = new UserContract
            {
                Id = u.Id,
                Address = u.Address,
                Avatar = u.Avatar,
                Email = u.Email,
                Fname = u.Fname,
                Lname = u.Lname,
                Password = u.Password,
                Tel = u.Tel
            };
            var flag = ent.Users.Any(x => x.Email == user.Email && x.Status == 1);

            if (!flag)
            {
                if (Avatar != null)
                {
                    var length = Avatar.InputStream.Length;
                    MemoryStream target = new MemoryStream();
                    Avatar.InputStream.CopyTo(target);
                    u.Avatar = target.ToArray();
                }
                if (ModelState.IsValid)
                {
                    u.Status = 1;
                    ent.Users.Add(u);
                    ent.SaveChanges();
                    Session["u_id"] = u.Id;
                    Session["name"] = u.Fname + " " + u.Lname;
                    return RedirectToAction("Index", "Products");
                }
                return View(user);
            }
            else
            {
                TempData["Error"] = "The email address already exist, please login...";
                return View(user);
            }
           
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["u_id"] == null)
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Account");
            }
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = ent.Users.Find(id);

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Exclude = "Avatar")]User u, HttpPostedFileBase Avatar)
        {
            if (Avatar != null)
            {
                var length = Avatar.InputStream.Length;
                MemoryStream target = new MemoryStream();
                Avatar.InputStream.CopyTo(target);
                u.Avatar = target.ToArray();
            }

            if (ModelState.IsValid)
            {
                u.Status = 1;
                ent.Entry(u).State = EntityState.Modified;
                ent.SaveChanges();
                return RedirectToAction("Profile", "Users", new { id = u.Id });
            }

            return View(u);
            
        }

        // POST: Users/Delete/5
        public ActionResult Delete(int id)
        {
            if (Session["u_id"] == null )
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Products");
            }
            else
            {
                int s_id = (int)Session["u_id"];
                if(s_id == id)
                {
                    User user = ent.Users.Find(id);
                    user.Status = 0;
                    ent.Entry(user).State = EntityState.Modified;
                    ent.SaveChanges();
                    Session.Abandon();
                    return RedirectToAction("Index", "Products");
                }
                else
                {
                    TempData["Error"] = "You can only delete your own account !";
                    return RedirectToAction("Index", "Products");
                }
                
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
