using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class CategoryController : BaseController
    {
        // GET: Category
        public ActionResult i()
        {
            if (IsLogon() == false)
            {
                return RedirectToAction("index", "i");
            }
            else if (((int)(CurrentUser().MemberType)) < 4)
            {
                return RedirectToAction("index", "i");
            }
            var categories = context.Categories.Where(x => x.IsDeleted == false || x.IsDeleted == null).ToList();
            return View(categories.OrderByDescending(x => x.AddedDate).ToList());
        }

        public ActionResult Edit(int id = 0)
        {
            if (IsLogon() == false)
            {
                return RedirectToAction("index", "i");
            }
            var cat = context.Categories.FirstOrDefault(x => x.Id == id);
            var categories = context.Categories.Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString()
            }).ToList();

            categories.Add(new SelectListItem()
            {
                Value = "0",
                Text= "Ana Categori",
                Selected=true
            });
            ViewBag.Categories = categories;
            return View(cat);
        }

        [HttpPost]
        public ActionResult Edit(Entities.Categories category)
        {
            if (IsLogon() == false)
            {
                return RedirectToAction("index", "i");
            }
            if (category.Id > 0)
            {
                var cat = context.Categories.FirstOrDefault(x => x.Id == category.Id);
                cat.Description = category.Description;
                cat.Name = category.Name;
                cat.ModifedDate = DateTime.Now;
                cat.IsDeleted = false;

                if (category.Parent_Id > 0)
                    cat.Parent_Id = category.Parent_Id;
                else
                    cat.Parent_Id = null;

            }
            else
            {
                category.AddedDate = DateTime.Now;
                category.IsDeleted = false;
                if (category.Parent_Id == 0)
                    category.Parent_Id = null;
                context.Entry(category).State = System.Data.Entity.EntityState.Added;

            }
            context.SaveChanges();
            return RedirectToAction("i");
        }

        public ActionResult Delete(int id)
        {
            if (IsLogon() == false)
            {
                return RedirectToAction("index", "i");
            }
            var pro = context.Categories.FirstOrDefault(x => x.Id == id);
            pro.IsDeleted = true;//silinen true
            context.SaveChanges();
            return RedirectToAction("i");
           
        }
    }
}