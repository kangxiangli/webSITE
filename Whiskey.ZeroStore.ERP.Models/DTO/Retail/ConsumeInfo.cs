using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class ConsumeInfo
    {
        public decimal? LevelDiscount { get; set; }
        /// <summary>
        /// 出货店铺
        /// </summary>
        public int OutStoreId { get; set; }

        public decimal ConsumeCoun { get; set; }

        /// <summary>
        /// 现金
        /// </summary>
        public decimal Cash { get; set; }
        /// <summary>
        /// 积分
        /// </summary>
        public decimal Score { get; set; }
        /// <summary>
        /// 储值
        /// </summary>
        public decimal CardMoney { get; set; }
        /// <summary>
        /// 刷卡
        /// </summary>
        public decimal SwipCard { get; set; }
        public SwipeCardType? SwipeCardType { get; set; }
        /// <summary>
        /// 抹去
        /// </summary>
        public decimal Erase { get; set; }
        /// <summary>
        /// 找零
        /// </summary>
        public decimal ReturnMoney { get; set; }
        /// <summary>
        /// 优惠券编号
        /// </summary>
        public string CouponNum { get; set; }

        /// <summary>
        /// 优惠券优惠
        /// </summary>
        public decimal CouponMoney { get; set; }

        /// <summary>
        /// 店铺活动id
        /// </summary>
        public int? StoreActivityId { get; set; }

        /// <summary>
        /// 店铺活动折扣金额
        /// </summary>
        public decimal storeActivityDiscountMoney { get; set; }
        /// <summary>
        /// 出库时间
        /// </summary>
        public DateTime OutStoragTime { get; set; }
        /// <summary>
        /// 经办人
        /// </summary>
        public int Operat { get; set; }
        public string Note { get; set; }
    }
}
