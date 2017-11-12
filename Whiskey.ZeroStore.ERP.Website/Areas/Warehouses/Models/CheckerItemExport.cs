using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Warehouses.Models
{
    public class CheckerItemExport
    {
        public string ProductBarcode { get; set; }

        /// <summary>
        /// 状态描述
        /// </summary>
        public string StateDes { get; set; }
    }
}