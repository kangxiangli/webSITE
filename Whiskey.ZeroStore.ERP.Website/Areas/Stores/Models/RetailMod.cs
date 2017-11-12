using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models
{
    public class RetailMod
    {
        //pnums:[a,b,c]  a:[d]  d:{ProductNumb:"xxxxx",ExisBarcode:["xxxxx001","xxxxx002"],NeedCou:3}
        public string ProductNumb { get; set; }
        public string[] ExisBarcode { get; set; }
        public int NeedCou { get; set; }
    }
}