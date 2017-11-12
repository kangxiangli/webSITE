using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class StoreCartItemDto : IAddDto, IEditDto<int>
    {
        public StoreCartItemDto()
        {
            //初始化数量，初始值为1
            Quantity = 1;
        }

        [DisplayName("购物车")]
        public virtual int? StoreCartId { get; set; }

        [Display(Name = "款号")]
        [StringLength(10)]
        [Description("同款式商品下款号是相同的")]
        public virtual string BigProdNum { get; set; }

        [DisplayName("商品")]
        public virtual int? ProductId { get; set; }

        [DisplayName("颜色")]
        public virtual int? ColorId { get; set; }

        [DisplayName("尺码")]
        public virtual int? SizeId { get; set; }

        [DisplayName("数量")]
        public virtual int Quantity { get; set; }

        //[DisplayName("选购仓库")]
        //public virtual int? PurchaseStorageId { get; set; }

        [DisplayName("标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
