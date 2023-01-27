using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaret.Models.Message
{
    public class iModels
    {
        public List<System.Web.Mvc.SelectListItem> Users { get; set; }
        public List<Entities.Messages> Messages { get; set; }

    }
}