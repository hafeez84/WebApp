using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class CommentsController : Controller
    {
        private MyDBCommentEntities db = new MyDBCommentEntities();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CompanyProductUpload c)
        {
            Comment comment = new Comment();
            int u_id = (int) Session["u_id"];
            comment.U_id = u_id;
            comment.State = 1;
            comment.P_id = c.ProductM.Id;
            comment.Description = c.P_Comment.Description;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
            }
            var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
            return RedirectToAction("Details", "Products", new { id = comment.P_id}) ;
        }
        
        public ActionResult Delete(int id)
        {
            var comment = db.Comments.Find(id);
            comment.State = 0;
            db.Entry(comment).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", "Products", new { id = comment.P_id });
        }
    }
}