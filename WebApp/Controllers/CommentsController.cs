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
        public ActionResult Create(string pid, string cmnt)
        {
            int p_id = Convert.ToInt32(pid);
            Comment comment = new Comment();
            int u_id = (int) Session["u_id"];
            comment.U_id = u_id;
            comment.State = 1;
            comment.P_id = p_id;
            comment.Description = cmnt;

            if (ModelState.IsValid)
            {
                db.Comments.Add(comment);
                db.SaveChanges();
            }
            var errors = ModelState
            .Where(x => x.Value.Errors.Count > 0)
            .Select(x => new { x.Key, x.Value.Errors })
            .ToArray();
            CompanyProductUpload comments = new CompanyProductUpload
            {
                P_Comments = db.Comments.Where(x=>x.P_id == p_id).ToList()
            };
            return PartialView("~/Views/Products/_Comments.cshtml", comments);
        }
        
        [HttpPost]
        public ActionResult Delete(string id, string p_id)
        {
            int i_id = Convert.ToInt32(id); 
            var comment = db.Comments.Find(i_id);
            comment.State = 0;
            db.Entry(comment).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

            int i_p_id = Convert.ToInt32(p_id);
            CompanyProductUpload comments = new CompanyProductUpload
            {
                P_Comments = db.Comments.Where(x => x.P_id == i_p_id).ToList()
            };
            return PartialView("~/Views/Products/_Comments.cshtml", comments);
        }
    }
}