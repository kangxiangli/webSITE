using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Models
{
    public class SelectListDat
    {
        public string label { get; set; }
        public string value { get; set; }
        public string title { get; set; }
        public bool selected { get; set; }
        public bool disabled { get; set; }
        public SelectListDat[] children { get; set; }
    }
}