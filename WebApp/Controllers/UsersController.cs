using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

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
        public ActionResult Signup([Bind(Include = "Id,Fname,Lname,Tel,Email,Address,Password")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Users.Add(user);
                db.SaveChanges();
                Session["id"] = user.Id;
                return RedirectToAction("Profile", "Users", new { id = (int) Session["id"] });
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (Session["id"] == null)
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

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Fname,Lname,Tel,Email,Address,Password")] User user)
        {
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
            if (Session["id"] == null)
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
            if (Session["id"] == null)
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
