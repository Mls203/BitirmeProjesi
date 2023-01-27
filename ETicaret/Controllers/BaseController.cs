using ETicaret.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ETicaret.Controllers
{
    public class BaseController : Controller
    {
        //EF İLE HEM TABLOLARIM AKARŞILIK GELEN SINIFIM OLUŞTU HEMDE
        //DB ERİŞİMİ İÇİN CONTEXT SINIFIM VE İÇERİNDE TABLOLARIMA
        //KARŞILIK GELEN DbSet<> LER GELMİŞ OLDU
        //ARTIK VERİTABANINA ERİŞM SAĞLAYABİLİRİM

        //protected  ETicaretEntities context; bu şekilde yazarsam miras alınan tüm sınflarda context değişkeni
        //set edilebilir. Ben sadece bu sınıf içeirnde set edilsin
        //istiyorum oyüzden  { get; private set; } böyle tanımlıyorum

        // protected ETicaretEntities context;bu şekilde yazarsam diğer sayfalrada NEW lenip set edilebilir
        // ben sadece bu sınıf içerinde set edilmsini istediğim için private set; olarak tanımladım ve
        // miras alan tüm classların erişmesini istediğimden { get; } ile tanımladım

        //YANİ MİRAS ALAN SINIFLAR OKUMA İŞLEMİNİ GERÇEKLEŞTİREBİLİR AMA SET İŞLEMİNİ GERÇEKLEŞTİRMEZ SADECE
        // ERİŞEBİLİR


        protected ETicaretEntities context { get; private set; }

        public BaseController()
        {
            context = new ETicaretEntities();
            //kategoriler layoutumda olduğu için hengi viewa gidersem gidiyim kategorileri db den alıyor olmam laızm o
            //yüzden viewbag içine atıyacam sonrasında her viewde kullanacağım için
            //kategorileri partial view içerinde koycam 
            ViewBag.MenuKategories = context.Categories.Where(x => x.Parent_Id == null).ToList();
            //1)db den ana categorileri çektiyorum sonrasında alt menülere doğru ilerlicem

        }

        protected Entities.Members CurrentUser()//oturumu olan kullanıcın member bilgisi
        {
            return (Entities.Members)Session["LogonUser"];
        }
        protected int CurrentUserId()//oturumu olan kullanıcın id si
        {
            return ((Entities.Members)Session["LogonUser"]).Id;
        }

        protected bool IsLogon()//oturumu olan kullanıcın id si
        {
            if (Session["LogonUser"] == null)
                return false;
            else
            {
                return true;
            }

        }

        protected List<Categories> GetChildCategories(Categories cat)
        {
            var result = new List<Categories>();
            result.AddRange(cat.Categories1);

            foreach (var item in cat.Categories1)
            {
              var list=  GetChildCategories(item);
                result.AddRange(list);
            }
            return result;
        }
    }
}