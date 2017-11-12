using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Models
{
    public class SingleInfo
    {
        public int MemberId { get; set; }

        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int? CategoryId { get; set; }
        public int? ColorId { get; set; }
        public int? SizeId { get; set; }
        public string SeasonId { get; set; }
        public string ProductAttrId { get; set; }
        public decimal? Price { get; set; }
        public string Brand { get; set; }
        public string Notes { get; set; }
    }
}