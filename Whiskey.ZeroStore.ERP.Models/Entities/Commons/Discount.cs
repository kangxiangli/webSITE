
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Discount : EntityBase<int>
    {
        public Discount() {

			DiscountName="";

            DiscountMethod = 0; //0：直接打折 1：按时间打折

            Description = "";

            StartTime = DateTime.Now;

            EndTime = DateTime.Now;

            DiscountByPrice = false;

            DiscountPrice = 0;

            DiscountByAmount = false;

            DiscountAmount = 0;

            DiscountType = 0; //0:打折 1:减现金

            DiscountRate = 0; //现金或折扣

            IsEnabled = false;
        }
			
		[Display(Name = "名称")]
        public virtual string DiscountName { get; set; }

        [Display(Name = "打折类型")]
        public virtual int DiscountMethod { get; set; }//0：直接打折 1：按时间打折

        [Display(Name = "折扣简介")]
        public virtual string Description { get; set; }



        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime EndTime { get; set; }



        [Display(Name = "按订单金额打折")]
        public virtual bool DiscountByPrice { get; set; }

        [Display(Name = "订单金额满")]
        public virtual decimal DiscountPrice { get; set; }



        [Display(Name = "按货品数量打折")]
        public virtual bool DiscountByAmount { get; set; }

        [Display(Name = "货品数量满")]
        public virtual decimal DiscountAmount { get; set; }



        [Display(Name = "折扣类型")]
        public virtual int DiscountType { get; set; } //0:打折 1:减现金

        [Display(Name = "折扣比例")]
        public virtual Decimal DiscountRate { get; set; } //现金或折扣


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }
}


