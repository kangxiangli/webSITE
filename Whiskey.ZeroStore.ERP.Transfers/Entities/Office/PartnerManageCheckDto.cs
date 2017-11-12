using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PartnerManageCheckDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "部门名称")]
        [StringLength(20, ErrorMessage = "最长字符不能超过{1}")]
        public string DepartmentName { get; set; }

        [Display(Name = "店铺名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
        public string StoreName { get; set; }

        [Display(Name = "仓库名称")]
        public virtual string StorageName { get; set; }

        [Display(Name = "店铺类型")]
        public int StoreTypeId { get; set; }

        public int PartnerManageId { get; set; }
    }
}
