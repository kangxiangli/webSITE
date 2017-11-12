using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreType : EntityBase<int>
    {
        [Display(Name = "类型名称")]
        [Required(ErrorMessage = "不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public string TypeName { get; set; }

        [Display(Name = "需要支付")]
        public bool IsPay { get; set; }


        /// <summary>
        /// 独立库存
        /// </summary>
        [Display(Name = "独立库存")]
        public bool IndependentStorage { get; set; }


        [Display(Name = "是否分销店铺")]
        public virtual bool IsReseller { get; set; }

        public virtual Administrator Operator { get; set; }
    }
}
