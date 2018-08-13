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
        private MyDBUserEntities dbUser = new MyDBUserEntities();
        private MyDBCompanyEntities dbCompany = new MyDBCompanyEntities();



        // GET: Account
        public ActionResult Index()
        {
            if (Session["u_id"] != null)
            {
                return RedirectToAction("Profile", "Users", new { id = (int) Session["u_id"] });
            }
            else if (Session["c_id"] != null)
            {
                return RedirectToAction("Profile", "Companies", new { id = (int)Session["c_id"] });
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Login(User user)
        {   

            var user_a = dbUser.Users.Where(x => x.Email == user.Email && x.Password == user.Password).FirstOrDefault();
            var company_a = dbCompany.Companies.Where(x => x.Email == user.Email && x.Password == user.Password).FirstOrDefault();

            if (user_a != null)
            {
                Session["u_id"] = user_a.Id;
                return RedirectToAction("Index", "Products", new { id = user_a.Id });
            }
            else if(company_a != null)
            {
                Session["c_id"] = company_a.Id;
                return RedirectToAction("Profile", "Companies", new { id = company_a.Id });
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