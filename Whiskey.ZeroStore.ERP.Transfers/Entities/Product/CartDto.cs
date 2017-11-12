using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class CartDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "会员")]
        public int MemberId { get; set; }

        [Display(Name = "商品")]
        public int ProductId { get; set; }

        [Display(Name = "数量")]
        public int Count { get; set; }

        [Display(Name = "标识")]
        public Int32 Id { get; set; }
    }
}
