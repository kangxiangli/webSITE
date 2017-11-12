using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class ExchangeOrderPayDto
    {

        public ExchangeOrderPayDto()
        {

            SwipeCardType = SwipeCardType.银行卡;
            BarcodesToExchange = new List<string>();
            NewProductBarcodes = new List<string>();
        }

        public List<ProductInfo> NewProductInfo { get; set; }
        /// <summary>
        /// 要换商品的流水号
        /// </summary>
        public List<string> BarcodesToExchange { get; set; }
        /// <summary>
        /// 新商品的流水号
        /// </summary>
        public List<string> NewProductBarcodes { get; set; }

        /// <summary>
        /// 零售单号
        /// </summary>
        public string RetailNumber { get; set; }



        /// <summary>
        /// 原始换货商品总金额
        /// </summary>
        [Display(Name = "原始换货商品总金额")]
        public decimal OriginProductTotalAmount { get; set; }

        /// <summary>
        /// 换得商品总金额
        /// </summary>
        [Display(Name = "换得商品总金额")]
        public decimal NewProductTotalAmount { get; set; }

        /// <summary>
        /// 差额，换得商品与原始商品价格差
        /// </summary>
        [Display(Name = "差额")]
        public decimal DiffAmount { get; set; }


        /// <summary>
        /// 现金消费
        /// </summary>
        [Display(Name = "现金消费")]
        public virtual decimal CashConsume { get; set; }

        /// <summary>
        /// 刷卡消费
        /// </summary>
        [Display(Name = "刷卡消费")]
        public virtual decimal SwipeConsume { get; set; }

        /// <summary>
        /// 储值消费
        /// </summary>
        [Display(Name = "储值消费")]
        public virtual decimal StoredValueConsume { get; set; }


        /// <summary>
        /// 积分消费
        /// </summary>
        [Display(Name = "积分消费")]
        public virtual decimal ScoreConsume { get; set; }


        /// <summary>
        /// 抹去
        /// </summary>
        [Display(Name = "抹去")]
        public virtual decimal EraseConsume { get; set; }

        /// <summary>
        /// 找零
        /// </summary>
        [Display(Name = "找零")]
        public virtual decimal ReturnMoney { get; set; }

        /// <summary>
        /// 换货会员
        /// </summary>
        [Display(Name = "换货会员")]
        public virtual int? ConsumerId { get; set; }



        /// <summary>
        /// 换货店铺
        /// </summary>
        [Display(Name = "换货店铺")]
        public virtual int StoreId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [StringLength(20, ErrorMessage = "备注长度在20个字符以内")]
        public virtual string Note { get; set; }

        /// <summary>
        /// 经办人
        /// </summary>
        public int OperatorId { get; set; }



        /// <summary>
        /// 刷卡消费类型
        /// </summary>
        public SwipeCardType SwipeCardType { get; set; }

        /// <summary>
        /// 储值返还
        /// </summary>
        public decimal ReturnStoredValue { get; set; }

        /// <summary>
        /// 是否为补差价
        /// </summary>
        public bool IsPay { get; set; }

        /// <summary>
        /// 会员折扣
        /// </summary>
        public decimal? LevelDiscount { get; set; }

    }
}
