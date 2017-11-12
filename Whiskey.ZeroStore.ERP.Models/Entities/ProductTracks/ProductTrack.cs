using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class ProductTrack : EntityBase<int>
    {

        public ProductTrack()
        {

        }
        /// <summary>
        /// 商品货号
        /// </summary>
        [Display(Name = "商品货号")]
        public virtual string ProductNumber { get; set; }

        [Display(Name = "商品流水号")]
        public virtual string ProductBarcode { get; set; }

        [Display(Name = "描述")]
        public virtual string Describe { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }

    public class ProductOptDescTemplate
    {
        public const string ON_PRODUCT_ADD = "商品建档";
        public const string ON_PRODUCT_PRINT = "商品条码打印";
        public const string ON_PRODUCT_INVENTORY = "商品入库到{0}";
        public const string ON_ORDERBLANK_ADD = "配货锁定中,商品由{0}配货到{1},单号:{2}";
        public const string ON_ORDERBLANK_REMOVE = "配货状态已解锁,商品从配货单中移除";
        public const string ON_ORDERBLANK_DROP = "配货状态已解锁,配货单已撤销";
        public const string ON_ORDERBLANK_DELIVERY = "发货待接收,商品由{0}发货到{1}";
        public const string ON_ORDERBLANK_REFUSE = "{0}拒绝收货,商品返回{1}";
        public const string ON_ORDERBLANK_ACCEPT = "{0}已收货";
        public const string ON_PRODUCT_CHECKER_START = "商品由{0}盘点中";
        public const string ON_PRODUCT_CHECKER_END = "商品由{0}结束盘点";
        public const string ON_PRODUCT_CHECKER_DELETE = "商品盘点记录删除，商品退出盘点";
        public const string ON_PRODUCT_RETAIL = "商品由{0}零售";
        public const string ON_PRODUCT_CHECKER_INVENTORY_DELETE = "{0}商品校对时从缺货列表中删除库存数据";
        public const string ON_PRODUCT_CHECKER_INVENTORY_ADD = "商品校对时从余货列表中，对库存入库到{0}";
        public const string ON_PRODUCT_RETURN = "商品零售退回到{0}";
        public const string ON_PRODUCT_EXCHANGE_ORIGIN = "商品从订单中被换货退回到{0}";
        public const string ON_PRODUCT_EXCHANGE_NEW = "商品成为换货的新商品由{0}售出";
        public const string ON_PRODUCT_PURCHASE_ADD = "采购锁定中,商品由店铺-{0},仓库-{1}锁定";
        public const string ON_PRODUCT_PURCHASE_DROP = "取消商品在店铺-{0},仓库-{1}中的锁定";
        public const string ON_APPOINTMENT_PACKING_ADD = "商品被预约装箱采购单锁定,店铺{0},仓库{1}";
        public const string ON_APPOINTMENT_PACKING_DROP = "商品从预约装箱采购单移除而解锁,店铺{0},仓库{1}";

    }
}
