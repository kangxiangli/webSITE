



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Order : EntityBase<int>
    {
        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        #region 外键

        [Display(Name = "所属店铺")]
        public virtual int? StoreId { get; set; }

        [Display(Name = "购买会员")]
        public virtual int? MemberId { get; set; }

        [Display(Name = "快递公司")]
        public virtual int? ExpressId { get; set; }

        [Display(Name = "支付公司")]
        public virtual int? PaymentId { get; set; } 

        #endregion



        #region 订单

        [Display(Name = "订单标识")]
        public virtual Guid OrderGuid { get; set; }

        [Display(Name = "订单号码")]
        public virtual string OrderNumber { get; set; }

        [Display(Name = "订单状态")]
        public virtual int OrderStatus { get; set; }//0：提交订单，1：商品出库，2：等待收货，3：完成

        [Display(Name = "备注信息")]
        public virtual string Notes { get; set; }

        #endregion



        #region 运输

        [Display(Name = "运输方式")]
        public virtual int DeliverType { get; set; }//0：柜台拿货，1：快递运输，2：货到付款

        [Display(Name = "快递运费")]
        public virtual decimal ExpressFee { get; set; } 

        [Display(Name = "快递公司")]
        public virtual string ExpressCompany { get; set; }

        [Display(Name = "快递单号")]
        public virtual string ExpressNumber { get; set; }

        #endregion



        #region 收货

        [Display(Name = "收件人")]
        public virtual string Receiver { get; set; }

        [Display(Name = "所在城市")]
        public virtual string CityName { get; set; }

        [Display(Name = "所在省份")]
        public virtual string ProvinceName { get; set; }

        [Display(Name = "家庭地址")]
        public virtual string HomeAddress { get; set; }

        [Display(Name = "固定电话")]
        public virtual string Telephone { get; set; }

        [Display(Name = "手机号码")]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "邮政编码")]
        public virtual string ZipCode { get; set; }

        #endregion



        #region 支付

        [Display(Name = "付款方式")]
        public virtual int PaymentType { get; set; }//0：线下付款，1：在线支付

        [Display(Name = "支付公司")]
        public virtual string PaymentCompany { get; set; }

        [Display(Name = "付款帐号")]
        public virtual string PaymentAccount { get; set; }//支付平台的用户帐号

        [Display(Name = "付款流水")]
        public virtual string PaymentTradeNumber { get; set; }//交易记录流水号 

        #endregion




        #region 零售消费

        [Display(Name = "现金消费")]
        public virtual decimal Cash { get; set; }

        [Display(Name = "刷卡消费")]
        public virtual decimal Card { get; set; }

        [Display(Name = "积分消费")]
        public virtual decimal Score { get; set; }

        [Display(Name = "储值消费")]
        public virtual decimal Balance { get; set; }

        [Display(Name = "抹去价格")]
        public virtual decimal Coupon { get; set; }
        [Display(Name="消费总额")]
        public virtual decimal ConsumeCount { get; set; }

        [Display(Name = "找回零钱")]
        public virtual decimal Change { get; set; }

        #endregion


        [NotMapped]
        public virtual int Quantity {
            get {
                return OrderItems.Count;
            }
        }

        [NotMapped]
        public virtual decimal Price {
            get {
                return OrderItems.Sum(m => m.TagPrice);
                
            }
        }



        #region 导航

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ExpressId")]
        public virtual Express Express { get; set; }

        [ForeignKey("PaymentId")]
        public virtual Payment Payment { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; } 

        #endregion

    }
}


