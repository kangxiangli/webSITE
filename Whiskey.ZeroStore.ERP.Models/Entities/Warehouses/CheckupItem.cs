using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{

    /// <summary>
    /// 盘点校验
    /// </summary>
    [DisplayName("盘点校验")]
    [Description("对盘点数据的校验")]
    public class CheckupItem:EntityBase<int>
    {

        [Display(Name = "盘点标识符")]
        [StringLength(20)]
        public virtual string CheckGuid { get; set; }

        [Display(Name = "盘点详情")]                       
        public virtual int? CheckerItemId { get; set; }

        /// <summary>
        /// 枚举值，参考OpertaionFlag  缺货删除；余货插入
        /// </summary>
        [DisplayName("校验类型")]
        [Description("枚举值，参考OpertaionFlag")]
        public virtual int CheckupType { get; set; }


        [ForeignKey("CheckerItemId")]
        public virtual CheckerItem CheckerItem { get; set; }


        #region 注销代码

        //商品编号
        //public virtual string ProductNum { get; set; }
        //public int CheckupBeforeCou { get; set; } //校对前数量
        //public int CheckupAfterCou { get; set; } //校对后数量
        //public int CheckupBeforeMissCou { get; set; } //校对前缺货
        //public int CheckupAfterMissCou { get; set; }//校对后缺货
        //public int CheckupBeforeResCou { get; set; }//校对前余货
        //public int CheckupAfterResCou { get; set; }//校对后余货
        //[DisplayName("备注")]
        //[StringLength(200)]
        //public virtual string Notes { get; set; }
        #endregion

    }
}
