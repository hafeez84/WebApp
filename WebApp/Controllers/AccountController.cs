using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class AccountController : Controller
    {
        private MyDBUserEntities db = new MyDBUserEntities();



        // GET: Account
        public ActionResult Index()
        {
            if (Session["id"] != null)
            {
                return RedirectToAction("Details", "Users", new { id = (int) Session["id"] });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(User user)
        {
            var ud = db.Users.Where(x => x.Email == user.Email && x.Password == user.Password).FirstOrDefault();

            if (ud != null)
            {
                Session["id"] = user.Id;
                TempData["Success"] = "You have succesfully logged in !";
                return RedirectToAction("Details", "Users", new { id = ud.Id });
            }
            else
            {
                TempData["Error"] = "Please check your email and password and try again !";
                return RedirectToAction("Index", "Account");
            }
        }

        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Account");
        }
    }
}