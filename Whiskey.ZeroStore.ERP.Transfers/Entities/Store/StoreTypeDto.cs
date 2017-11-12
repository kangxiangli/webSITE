using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class StoreTypeDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "类型名称")]
        [Required(ErrorMessage = "不能为空")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string TypeName { get; set; }

        [Display(Name = "需要支付")]
        public virtual bool IsPay { get; set; }

        [Display(Name = "独立库存")]
        public virtual bool IndependentStorage { get; set; }


        [Display(Name = "是否分销店铺")]
        public virtual bool IsReseller { get; set; }
    }
}
