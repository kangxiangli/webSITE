using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 商品零售
    /// </summary>
    public class RetaiInfoDTO
    {
        public List<ProductInfo> Products { get; set; }
        public bool IsMember { get; set; }
        public MemberInfo MemberInfo { get; set; }
        public ConsumeInfo ConsumeInfo { get; set; }
        public string TradeCredential { get; set; }
        public string TradeReferNumber { get; set; }
    }
}
