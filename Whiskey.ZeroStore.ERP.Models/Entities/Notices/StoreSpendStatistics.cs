using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities.Notices
{
    /// <summary>
    /// 店铺开支统计
    /// </summary>
    public class StoreSpendStatistics : EntityBase<int>
    {
        [Display(Name = "统计店铺id")]
        public int StoreId { get; set; }
        /// <summary>
        /// 开支类型，具体见SpendType枚举
        /// </summary>
        public int SpendType { get; set; }
        /// <summary>
        /// 金额支出
        /// </summary>
        public float Amount { get; set; }
        /// <summary>
        /// 单据凭证图片
        /// </summary>
        public string OrderImg { get; set; }

        /// <summary>
        /// 起始年份
        /// </summary>
        public int StartYear { get; set; }
        /// <summary>
        /// 起始月份
        /// </summary>
        public byte StartMonth { get; set; }
        /// <summary>
        /// 起始日期
        /// </summary>
        public byte StartDay { get; set; }
        /// <summary>
        /// 结束年份
        /// </summary>
        public int EndYear { get; set; }
        /// <summary>
        /// 结束月份
        /// </summary>
        public byte EndMonth { get; set; }
        /// <summary>
        /// 结束日期
        /// </summary>
        public byte EndDay { get; set; }
        /// <summary>
        /// 详细说明
        /// </summary>
        [StringLength(500)]
        public string Notes { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 关键字，便于后期检索
        /// </summary>
        public string KeyWord { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }

    public enum SpendType
    {
        /// <summary>
        /// 水费
        /// </summary>
        [Description("水费")]
        WaterRate = 0,
        /// <summary>
        /// 电费
        /// </summary>
        [Description("电费")]
        ElectricCharge,
        /// <summary>
        /// 房租
        /// </summary>
        [Description("房租")]
        Chummage,
        /// <summary>
        /// 杂费
        /// </summary>
        [Description("杂费")]
        Incidentals,
        /// <summary>
        /// 税金
        /// </summary>
        [Description("税金")]
        Taxes,
        /// <summary>
        /// 员工的工资支出
        /// </summary>
        [Description("工资")]
        SalaryCoun,
        /// <summary>
        /// 店铺维护
        /// </summary>
        [Description("店铺维护")]
        Tending,
        /// <summary>
        /// 其他原因产生的支出费用
        /// </summary>
        [Description("其他")]
        Other,
    }

}
