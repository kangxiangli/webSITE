using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Products
{
    //yxk 2016-3
    /// <summary>
    /// 买手说标签
    /// </summary>
    public class BuysaidAttribute : EntityBase<int>
    {
        public string AttriName { get; set; }
        public string Descri { get; set; }
        public virtual ICollection<ProductOriginNumber> ProductOrigNumbers { get; set; }
    }
}
