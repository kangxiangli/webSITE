

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class CheckerItem : EntityBase<int>
    {
        public CheckerItem()
        {

        }

        [DisplayName("盘点数据")]
        public virtual int? CheckerId { get; set; }

        [Display(Name="盘点标识符")]
        [StringLength(20)]
        public virtual string CheckGuid { get; set; }

        [Display(Name = "盘点商品")]
        public virtual int? ProductId { get; set; }

        [Display(Name = "商品一维码")]

        public virtual string ProductBarcode { get; set; }

        [Display(Name = "盘点结果")]
        [Description("参照CheckerItemFlag枚举值")]
        public virtual int CheckerItemType { get; set; }

        [Display(Name = "是否校对")]
        [Description("对盘点后的数据校对如果校对过就不再计算范围内")]
        public virtual bool IsCheckup { get; set; }

        //[Display(Name = "盘点商品状态")]
        //[Description("参照CheckerProductFlag枚举值")]
        //public virtual int CheckerProductType { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("CheckerId")]
        public virtual Checker Cherker { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }

        #region 注销代码
                
        //[Display(Name = "待盘编号")]
        //public virtual string CheckCount { get; set; }//多个编号之间用逗号隔开

        //[Display(Name = "已盘编号")]
        //public virtual string CheckedCount { get; set; }

        //[Display(Name = "有效数量")]
        //public virtual string ValidCount { get; set; }

        //[Display(Name = "无效数量")]
        //public virtual string InvalidCount { get; set; }

        //[Display(Name = "缺货数量")]
        //public virtual string MissingCount { get; set; }

        //[Display(Name = "余货数量")]
        //public virtual string ResidueCount { get; set; }
        /// <summary>
        /// 1:开始盘点 2：盘点中断 3：盘点完成 4：盘点撤消 5:完成校对
        /// </summary>
        //public virtual int CheckerState { get; set; }//1:开始盘点 2：盘点中断 3：盘点完成 4：盘点撤消 5:完成校对

        //[Display(Name = "盘点备注")]
        //public virtual string Notes { get; set; }
        #endregion


    }
}
