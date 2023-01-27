using ETicaret.Entities;
using ETicaret.Filter;
using ETicaret.Models;
using ETicaret.Models.i;
using ETicaret.Views.i;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace ETicaret.Controllers
{
    public class iController : BaseController//base controllerdan miras alır 
    {

        public ActionResult Index(int? id)
        {
            //return [context.Products.ToList();] db ile ürünleri listelemek yerine
            //IndexModel adında class oluşturduk ve ürünlistimizi orada tutacağımızı belirledik
            // context.Products.ToList(); diyerekte IndexModel içierinde belirlediğimiz
            //ürünlistesi içeirini doldurduk
            List<Entities.Products> products = context.Products.OrderByDescending(x => x.AddedDate).Where(x => x.IsDeleted == false || x.IsDeleted == null).ToList();
            Entities.Categories category = null;
            if (id > 0)
            {
               
                //categori filtreeme işlemi için seçilen categoriyi alrıız
                category = context.Categories.FirstOrDefault(x => x.Id == id);
                var allCategoris = GetChildCategories(category);
                allCategoris.Add(category);
                var catIntList = allCategoris.Select(x => x.Id).ToList();
                //seçilen kategoriye göre urunler listelensin diye
                products = products.Where(x => catIntList.Contains(x.Category_Id)).ToList();

            }
            var ındexviewModel = new Models.i.IndexModel()
            {
                Products = products.ToList(),
                Category = category
            };

            return View(ındexviewModel);
        }


        [HttpGet]
        public ActionResult Product(int id = 0)
        {
            var product = context.Products.FirstOrDefault(x => x.Id == id);

            if (product == null)
            {
                return RedirectToAction("Index", "i");
            }

            ProductModel productmodel = new ProductModel
            {
                Product = product
            };

            return View(productmodel);
        }


        [HttpPost]
        //burda bize postla hiiden product_id ve text gelir veritabınındaki kolonla aynı yazdığımız için
        //otomatik olarak onu algılar ve kaydeder

         [MyAuthorizataion]
        public ActionResult Product(Entities.Comments comment)
        {
            try
            {
                comment.AddedDate = DateTime.Now;
                comment.Member_Id = base.CurrentUserId();
                context.Comments.Add(comment);
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                ViewBag.message = ex.Message;

            }

            return RedirectToAction("Product", "i");
        }



        [HttpGet]
        public ActionResult AddBasket(int id, Boolean remove = false)
        {
            List<BasketModels> basket = null;
            //  BasketModel basket = null;
            if (Session["Basket"] == null)
            {
                basket = new List<BasketModels>();
            }
            else
            {
                basket = (List<BasketModels>)Session["Basket"];
            }

            if (basket.Any(m => m.Product.Id == id))//sepette o ürüne ait ürünidsi varmı diye kontrol yapılır
            {
                var pro = basket.FirstOrDefault(m => m.Product.Id == id);

                if (remove && pro.Count > 0)
                {
                    pro.Count -= 1;

                }
                else
                {
                    if (pro.Product.UnitsInStock > pro.Count)//stoksayısından fazla ürün ekleyemez
                    {
                        pro.Count += 1;
                    }
                    else
                    {
                        TempData["Error"] = "yeterli stok yok";
                    }

                }

            }
            else//eğer öyle bir ürün yoksa o üründen 1 tane eklicem
            {
                var pro = context.Products.FirstOrDefault(m => m.Id == id);
                if (pro != null && pro.IsContinued && pro.UnitsInStock > 0)
                {
                    basket.Add(new BasketModels
                    {
                        Count = 1,
                        Product = pro
                    });
                }
                else if (pro != null && pro.IsContinued == false)
                {

                    TempData["Error"] = "bu ürürn satışı durduruldu";
                }

            }

            basket.RemoveAll(m => m.Count < 1);
            Session["Basket"] = basket;//sesionı güncellerim

            return RedirectToAction("Basket", "i");
        }

        [HttpGet]
        public ActionResult Basket()
        {
            List<BasketModels> model = (List<BasketModels>)Session["Basket"];
            if (model == null)
            {
                model = new List<BasketModels>();
            }

            if (IsLogon())//oturumu açık olan kullanıcının adreslerini sepet sayfasında getir
            {
                int currentId = base.CurrentUserId();
                ViewBag.CurrentAddress = context.Addresses
                                        .Where(m => m.Member_Id == currentId)
                                        .Select(x => new SelectListItem()//liste içerinden gelen veriyi farklı br tipe dnüştürüyorum
                                        {
                                            Text = x.Name + "    (" + x.AdresDescription + ")",
                                            Value = x.Id.ToString()
                                        }).ToList();
            }

            ViewBag.Total = model.Select(x => x.Product.Price * x.Count).Sum();//ürünlerin toplamı
            return View(model); //ürün ve kaç adet oldukları
        }


        [HttpGet]
        public ActionResult RemoveBasket(int id)
        {
            List<BasketModels> basket = (List<BasketModels>)Session["Basket"];
            if (basket != null)
            {
                if (id > 0)
                {
                    basket.RemoveAll(m => m.Product.Id == id);
                }
                else if (id == 0)
                {
                    basket.Clear();
                }

                Session["Basket"] = basket;

            }

            return RedirectToAction("Basket", "i");
        }

        [HttpPost]
        [MyAuthorizataion]
        public ActionResult Buy(string Address)//satın alma işleminde otuurm yoksa logine yönlendir
        {

            if (IsLogon())
            {
                try
                {
                    var guid = new Guid(Address); //gelen adres string olduğu için guide çeviriyorum
                    var basket = (List<BasketModels>)Session["Basket"];
                    var _adress = context.Addresses.FirstOrDefault(x => x.Id == guid);
                    //sipariş verildi = SV
                    //ödeme Bildirimi = OB
                    //odeme onaylandı = OO
                    var order = new Entities.Orders()//önce adresi order tablosuna ekleyecem sonra order detail kısmına 
                    {
                        AddedDate = DateTime.Now,
                        Address = _adress.AdresDescription,
                        Member_Id = CurrentUserId(),
                        Status = "SV",
                        Id = Guid.NewGuid(),

                    };
                    foreach (BasketModels item in basket)
                    {
                        var oDetail = new Entities.OrderDetails();

                        oDetail.AddedDate = DateTime.Now;
                        oDetail.Price = item.Product.Price * item.Count;
                        oDetail.Product_Id = item.Product.Id;
                        oDetail.Quantity = item.Count;
                        oDetail.Id = Guid.NewGuid();


                        order.OrderDetails.Add(oDetail);
                        var _product = context.Products.FirstOrDefault(m => m.Id == item.Product.Id);

                        if (_product != null && _product.UnitsInStock >= item.Count)
                        {
                            _product.UnitsInStock = _product.UnitsInStock - item.Count;
                        }
                        else
                        {
                            throw new Exception(string.Format("{0} ürün için yeterli stock yoktur. " +
                                "Veya olmayan bir ürünü almaya çalışıyorsunux", item.Product.Name));
                        }

                    }
                    context.Orders.Add(order);
                    context.SaveChanges();
                    Session["Basket"] = null;
                }
                catch (Exception ex)
                {

                    TempData["Myerror"] = ex.Message;
                }
                return RedirectToAction("Buy", "i");
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        [HttpGet]
        [MyAuthorizataion]
        public ActionResult Buy()//satın alma işleminde oturum yoksa logine yönlendir
        {

            if (IsLogon())
            {
                var currentId = CurrentUserId();
                IQueryable<Entities.Orders> orders;
                if (((int)CurrentUser().MemberType) >8)
                {
                    orders = context.Orders.Where(x => x.Status == "OB");

                }
                else
            {
                orders = context.Orders.Where(x => x.Member_Id == currentId);

            }
            List<BuyModels> model = new List<BuyModels>();
            foreach (var item in orders)
            {
                var byModel = new BuyModels();
                byModel.TotelPrice = item.OrderDetails.Sum(y => y.Price);
                byModel.OrderName = string.Join(", ", item.OrderDetails.Select(y => y.Products.Name + "( " + y.Quantity + ") adet"));
                byModel.OrderId = item.Id.ToString();
                byModel.OrderStatus = item.Status;
                byModel.Member = item.Members;
                model.Add(byModel);
            }
            return View(model);
        }
        else
        {
            return RedirectToAction("Login", "Account");
        }
     }


        [HttpPost]
        public JsonResult OrderNotification(OrderNotification model)
        {
            if (string.IsNullOrEmpty(model.OrderId) == false)
            {
                var guid = new Guid(model.OrderId);
                var order = context.Orders.FirstOrDefault(x => x.Id == guid);
                if (order != null)
                {
                    order.Description = model.OrderDes;
                    order.Status = "OB";
                    context.SaveChanges();
                }
            }
            return Json("");
        }

        [HttpGet]
        public JsonResult GetProductDescription(int id)
        {
            var pro = context.Products.FirstOrDefault(x => x.Id == id);
            return Json(pro.Description, JsonRequestBehavior.AllowGet);//string döner
        }

        [HttpGet]
        public JsonResult GetOrder(string id)
        {
            var boolea = System.Data.SqlTypes.SqlGuid.Parse(id);
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            return Json(
                new {  Description = order.Description,
                       Address=order.Address 
                    }
                , JsonRequestBehavior.AllowGet); 
        } 

        [HttpPost]
        [MyAuthorizataion]
        public JsonResult OrderCompilete(string id, string text)
        {
            var boolea= System.Data.SqlTypes.SqlGuid.Parse(id);
            var guid = new Guid(id);
            var order = context.Orders.FirstOrDefault(x => x.Id == guid);
            order.Description = text;
            order.Status = "OO";//ÖDEME TAMAMLANDI
            context.SaveChanges();
            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }

}