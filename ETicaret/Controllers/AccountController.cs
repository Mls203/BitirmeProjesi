using ETicaret.Filter;
using ETicaret.Models.Account;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class AccountController : BaseController //miras alıyor db ye erişp sayfamızdaki layout
                                                    //kısımları içn categeri vs olaylarında artık sorunsuz çalışır
    {
        // GET: Account
        public ActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Register(RegisterModel user)
        {

            try
            {

                    if (user.repasword != user.Member.Password)
                    {
                        throw new Exception("şifreler aynı değil");
                    }
                    else
                    {
                    if (context.Members.Any(x => x.Email == user.Member.Email))//varlık yokluk kontrolü
                    {
                        throw new Exception("such an e-mail already exists");

                    }

                    user.Member.MemberType = (int)Models.MemberTypeEnum.Customer;
                    user.Member.AddedDate = DateTime.Now;
                        context.Members.Add(user.Member);
                        context.SaveChanges();
                        return RedirectToAction("Login", "Account");//kayıt yapan logine gider
                    }
              
            }
            catch (Exception ex)
            {
                ViewBag.ReError = ex.Message.ToString();//yukarda yazdığım mesaj yazılır
                return View();
            }

        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(LoginModel loginUser)
        {
            try
            {
                var loginUserCheck = context.Members.Where(m => m.Email == loginUser.Member.Email && m.Password == loginUser.Member.Password).FirstOrDefault();
                if (loginUserCheck == null)//öyle bir kayıt yoktur
                {
                    ViewBag.message = "Your password or e-mail address is incorrect";
                    return View();
                }
                else
                {
                    Session["LogonUser"] = loginUserCheck;//giriş yapan kullanıcı için oturum açılır
                    return RedirectToAction("Index","i");
                }


            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;
                return View();

            }
            
           
        }
        public ActionResult Logout()
        {
            //sesion silinir
            Session["LogonUser"] = null;
            return RedirectToAction("Login", "Account");
       
        }

        [HttpGet]
        public ActionResult Profile(int id=0, string ad="" )
        {
            List<Entities.Addresses> address = null;
            Entities.Addresses currentAddress = new Entities.Addresses();
            if (id == 0)
            {
                id = base.CurrentUserId();
                address = context.Addresses.Where(m => m.Member_Id == id).ToList();

                if (!string.IsNullOrEmpty(ad))
                {
                    var guid = new Guid(ad);  
                    currentAddress = context.Addresses.FirstOrDefault(m => m.Id ==guid);
                }
            }

            var user =context.Members.FirstOrDefault(m => m.Id == id);
            if (user == null) return RedirectToAction("index","i");
            ProfileModel profil = new ProfileModel
            {
                Member = user,
                Addresses = address,
                CurrentAddress = currentAddress
            };
            return View(profil);
        }

        [HttpGet]
        [MyAuthorizataion]
        public ActionResult ProfileEdit()
        {

            var member = base.CurrentUser();
            ProfileModel profile = new ProfileModel
            {
                Member=member
            };

            return View(profile);
        }


        [HttpPost]
        [MyAuthorizataion]
        public ActionResult ProfileEdit(ProfileModel updateprofile)
        { 
            var id=base.CurrentUserId();
            try
            {
                var user = context.Members.FirstOrDefault(m => m.Id == id);//bu kullanici üzerinde update işlemi gerçekleştirecem
                if(user==null) return RedirectToAction("index","i");

                user.Surname = updateprofile.Member.Surname;
                user.Name = updateprofile.Member.Name;
                if (updateprofile.Member.Password != null)
                {
                    user.Password=updateprofile.Member.Password;

                }

                user.Bio = updateprofile.Member.Bio;
                user.AddedDate = DateTime.Now;
                var file = Request.Files[0];
                if (file.ContentLength > 0)
                {
                    var folder = Server.MapPath("~/Images/upload");//dosyanın kaydedilmesini istediğim path
                    var fileName = "100"+user.Name+ ".jpg";//dosya uzantısını yazıyorum
                    file.SaveAs(Path.Combine(folder,fileName));

                    var sqlfileName = "Images/upload/" + fileName;
                    user.ProfileImageName= sqlfileName;//sql e kaydolacak dosya yolu
                }
                
                context.SaveChanges();

                return RedirectToAction("Profile", "Account");

            }catch(Exception ex)
            {
                ViewBag.error = ex.Message;
                var model = context.Members.FirstOrDefault(x => x.Id == id);
                return View(model);//catch e düşsede model göndermem gerk yoksa öbür türlü hata alıyorum
                                  //çünkü sayfa benden model bekliyor
            }
        }


        [HttpPost]
        [MyAuthorizataion]
        public ActionResult Adress(Entities.Addresses adres)
        {
            Entities.Addresses _adres = null;
            if (adres.Id==Guid.Empty)  //yeni adres ekleniyor demektir
            {
                adres.Id=Guid.NewGuid();
                adres.AddedDate = DateTime.Now;
                adres.Member_Id = base.CurrentUserId();
                context.Addresses.Add(adres);

            }
            else  //adres güncellemsi oluyor demektir
            {
                _adres = context.Addresses.Where(m => m.Id == adres.Id).FirstOrDefault();
                _adres.ModifiedDate = DateTime.Now;
                _adres.Name = adres.Name;
                _adres.AdresDescription = adres.AdresDescription;   

            }

            context.SaveChanges();
            return RedirectToAction("Profile", "Account");
        }

        [MyAuthorizataion]
        public ActionResult RemoveAdress(string id)
        {
            var guid = new Guid(id);
            var silinecek = context.Addresses.FirstOrDefault(x => x.Id == guid);
            context.Addresses.Remove(silinecek);
            context.SaveChanges();

            return RedirectToAction("Profile","Account");

        }


        [HttpGet]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public ActionResult ForgotPassword(string email)
        {
            var member = context.Members.FirstOrDefault(x => x.Email == email);
            if (member==null)
            {
                ViewBag.MyError = "Öyle bir e posta adresi bulunamadı";
                return View();
            }
            else
            {
                var body = "Şifreniz: " + member.Password;
                MyEmail mail = new MyEmail(member.Email,"Şifremi Unuttum", body);
                mail.SendMail();
                TempData["info"] = email + " mail adresinize şifreniz gönderilmiştir.";
                return RedirectToAction("Login");
            }
          
        }
    }
}