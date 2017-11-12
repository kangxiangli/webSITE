using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class PartnerManageCheck : EntityBase<int>
    {
        public PartnerManageCheck()
        {
            Roles = new List<Role>();
        }

        [Display(Name = "部门名称")]
        [StringLength(20,ErrorMessage="最长字符不能超过{1}")]
        public string DepartmentName { get; set; }

        [Display(Name = "店铺名称")]
		[StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
        public string StoreName { get; set; }

        [Display(Name = "仓库名称")]
        public virtual string StorageName { get; set; }

        [Display(Name = "店铺类型")]
        public int StoreTypeId { get; set; }

        public int PartnerManageId { get; set; }

        [ForeignKey("PartnerManageId")]
        public PartnerManage PartnerManage { get; set; }

        public virtual ICollection<Role> Roles { get; set; }

        [ForeignKey("StoreTypeId")]
        public virtual StoreType StoreType { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
