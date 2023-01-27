using ETicaret.Models.Message;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class MessageController : BaseController
    {
        // GET: Message
        public ActionResult i()
        {
            if (IsLogon() == false) return RedirectToAction("index", "i");
            var currentId = CurrentUserId();
            Models.Message.iModels model = new Models.Message.iModels();
            model.Users = new List<SelectListItem>();
            var users = context.Members.Where(x => ((int)x.MemberType) > 0 && (x.Id != currentId)).ToList();
            model.Users = users.Select(x => new SelectListItem()
            {
                Value = x.Id.ToString(),
                Text = string.Format("{0} {1} ({2})", x.Name, x.Surname, x.MemberType.ToString())
            }).ToList();


            var mesajList = context.Messages.Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId)).ToList();
            model.Messages = mesajList;
            return View(model);
        }

        [HttpPost]
        public ActionResult SendMessage(Models.Message.SendMessageModel message)
        {
            if (IsLogon() == false) return RedirectToAction("index", "i");

            Entities.Messages mesaj = new Entities.Messages()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                IsRead = false,
                Subject = message.subject,
                ToMemberId = message.ToUserId
                
            };
            var mRep = new Entities.MessageReplies()
            {
                Id = Guid.NewGuid(),
                AddedDate = DateTime.Now,
                Member_Id = CurrentUserId(),
                Text = message.MessageBody,
                
            };
            mesaj.MessageReplies.Add(mRep);
            context.Messages.Add(mesaj);
            context.SaveChanges();
            return RedirectToAction("i", "Message");
        }

        [HttpGet]
       public ActionResult MessageReplies(string id)
        {
            if (IsLogon() == false) return RedirectToAction("index", "i");
            var guid = new Guid(id);
            var currentUserId = CurrentUserId();
            Entities.Messages message = context.Messages.FirstOrDefault(x=>x.Id==guid);
            if (message.ToMemberId == currentUserId)
            {
                message.IsRead = true;
                context.SaveChanges();
            }
            
            MessageReplies model = new MessageReplies();
            
            model.MReplice = context.MessageReplies.Where(x => x.MessageId == guid).OrderBy(x=>x.AddedDate).ToList();

            return View(model);
        }


        [HttpPost]
        public ActionResult MessageReplies(Entities.MessageReplies message)
        {
            if (IsLogon() == false) return RedirectToAction("index", "i");

            message.AddedDate = DateTime.Now;
            message.Id = Guid.NewGuid();
            message.Member_Id = CurrentUserId();
            context.MessageReplies.Add(message);
            context.SaveChanges();
            return RedirectToAction("MessageReplies","Message",new {id=message.MessageId});
        }


        [HttpGet]
        public ActionResult RenderMessage()
        {
            RenderMessageModel model = new RenderMessageModel();
            var currentId = CurrentUserId();
            var mesajList = context.Messages.
                  Where(x => x.ToMemberId == currentId || x.MessageReplies.Any(y => y.Member_Id == currentId))
                    .OrderByDescending(x => x.AddedDate);
                    
                  
            model.Messages = mesajList.Take(4).ToList();


            model.Count = mesajList.Count();
            return PartialView("_Message",model);
        }

        public ActionResult RemoveMessageReplies(string id)
        {
            var guid = new Guid(id);
            //mesaj cevapları silindi
            var mReplice = context.MessageReplies.Where(x => x.MessageId == guid);
            context.MessageReplies.RemoveRange(mReplice);

            //mesajın kendisi silindi
            var message = context.Messages.FirstOrDefault(x => x.Id == guid);
            context.Messages.Remove(message);
            context.SaveChanges();
            return RedirectToAction("i", "Message");
        }
    }




}