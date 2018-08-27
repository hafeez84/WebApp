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
        private MyDBUserEntities db = new MyDBUserEntities();

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
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Signup
        public ActionResult Signup()
        {
            return View();
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
            var flag = db.Users.Any(x => x.Email == user.Email);

            if (!flag)
            {
                if (Avatar != null)
                {
                    var length = Avatar.InputStream.Length;
                    MemoryStream target = new MemoryStream();
                    Avatar.InputStream.CopyTo(target);
                    user.Avatar = target.ToArray();
                }
                if (ModelState.IsValid)
                {
                    db.Users.Add(u);
                    db.SaveChanges();
                    Session["u_id"] = user.Id;
                    Session["name"] = user.Fname + " " + user.Lname;
                    return RedirectToAction("Index", "Products");
                }
                return View(u);
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
            User u = db.Users.Find(id);
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

            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(HttpPostedFileBase Avatar, [Bind(Exclude = "Avatar")]User user)
        {   
            if (Avatar != null)
            { 
                var length = Avatar.InputStream.Length;
                MemoryStream target = new MemoryStream();
                Avatar.InputStream.CopyTo(target);
                user.Avatar = target.ToArray();
            }

            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Profile", "Users", new { id = user.Id });
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
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
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            if (Session["u_id"] == null)
            {
                TempData["Error"] = "You must be logged in to do this action !";
                return RedirectToAction("Index", "Account");
            }
            else
            {
                User user = db.Users.Find(id);
                db.Users.Remove(user);
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
