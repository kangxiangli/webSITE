using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class CheckupDto_t
    {
        public int Id { get; set; }
        public string ProNum { get; set; }
        public string ProName { get; set; }
        public string SizeName { get; set; }
        public string ColorName { get; set; }
        public int Count { get; set; }
    }
}