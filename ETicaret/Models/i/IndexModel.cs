using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaret.Models.i
{
    public class IndexModel//i controller içerisindeki indexmodeli burada belirliyoruz
    {
        public List<Entities.Products> Products { get; set; }
        public Entities.Categories Category { get; set; }
    }
}