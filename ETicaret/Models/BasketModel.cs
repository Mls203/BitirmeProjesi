using System.Collections.Generic;

namespace ETicaret.Models
{
    public class BasketModel//session koyacağım nesne
    {

        public BasketModel()
        {
            this.Products = new Dictionary<int, int>(); //null olmak yerine boş bir list oluturdum
        }

        //Dictionary<int, int> ürünidsi,üründen kaç adet alındığı
        public Dictionary<int, int> Products { get; set; }
    }
}