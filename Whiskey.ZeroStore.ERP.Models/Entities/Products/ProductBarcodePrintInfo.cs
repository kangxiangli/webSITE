using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Products
{
    public class ProductBarcodePrintInfo : EntityBase<int>
    {

        public virtual string ProductNumber { get; set; }
        /// <summary>
        /// 3位数
        /// 当前已打印的最新标记，36进制
        /// </summary>
        public virtual string CurPrintFlag { get; set; }
        /// <summary>
        /// 该商品最后一次打印条码的时间
        /// </summary>
        public virtual DateTime LastUpdateTime { get; set; }
        public virtual ICollection<Product> Inventories { get; set; }
        

    }
}
