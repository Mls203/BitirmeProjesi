using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaret.Models.Account
{
    public class ProfileModel
    {
        public Entities.Members Member { get; set; }
        public List<Entities.Addresses> Addresses { get; set; }
        public Entities.Addresses CurrentAddress { get; set; }//güncelleneeck adres
    }
}