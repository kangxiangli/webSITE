using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Models
{
    public class DiscountValidSession
    {

        public string Uuid { get; set; }
        public int Id { get; set; }
        public string Number { get; set; }
        public int Count { get; set; }
        /// <summary>
        /// 0：商品货号 1：商品款号
        /// </summary>
        public int Type { get; set; } 

    }
}