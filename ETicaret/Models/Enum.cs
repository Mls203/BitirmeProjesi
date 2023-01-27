using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ETicaret.Models
{
    public enum MemberTypeEnum:int//db de membertype int tanımlanmış

    {

        Customer = 0,
        Admin = 10,
        Editor = 8
    }
}