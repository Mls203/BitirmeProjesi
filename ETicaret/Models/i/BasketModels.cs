﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaret.Models.i
{
    public class BasketModels
    {
        public Entities.Products Product { get; set; }
        public int Count { get; set; }//ürün adedi
    }
}