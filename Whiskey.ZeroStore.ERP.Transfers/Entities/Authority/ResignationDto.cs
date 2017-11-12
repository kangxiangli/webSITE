using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ResignationDto : IAddDto, IEditDto<int>
    {
        [DisplayName("离职人")]
        public virtual int? ResignationId { get; set; }

        [DisplayName("离职日期")]
        [Required(ErrorMessage = "请选择离职日期")]
        public virtual DateTime ResignationDate { get; set; }

        [DisplayName("离职原因")]
        [StringLength(500, ErrorMessage = "不能超过{1}个字符")]
        [Required(ErrorMessage = "请填写离职原因")]
        public virtual string ResignationReason { get; set; }
        [DisplayName("交接人部门")]
        public virtual int? DepartmentId { get; set; }
        public virtual int? operationId { get; set; }
        [DisplayName("文档资料")]
        public virtual bool? Documentation { get; set; }

        [DisplayName("交接人")]
        public virtual int? HandoverManId { get; set; }
        [DisplayName("办公书籍")]
        public virtual bool? OfficeBooks { get; set; }
        [DisplayName("办公用品")]
        public virtual bool? OfficeSupplies { get; set; }
        [DisplayName("部门经理确认")]
        public virtual bool? DepartmentManagerConfirmed { get; set; }
        [DisplayName("数据资料")]
        public virtual bool? Data { get; set; }
        [DisplayName("工卡")]
        public virtual bool? JobCard { get; set; }
        [DisplayName("办公设备")]
        public virtual bool? OfficeEquipment { get; set; }
        [DisplayName("钥匙")]
        public virtual bool? OfficeKey { get; set; }
        [DisplayName("电话机")]
        public virtual bool? Telephone { get; set; }
        [DisplayName("其他")]
        public virtual bool? Other { get; set; }
        [DisplayName("其他描述")]
        [StringLength(500, ErrorMessage = "不能超过{1}个字符")]
        public virtual string OtherDesc { get; set; }
        [DisplayName("注销系统")]
        public virtual bool? CancellationSystem { get; set; }
        [DisplayName("清除指纹")]
        public virtual bool? ClearFingerprint { get; set; }
        [DisplayName("回收帐号")]
        public virtual bool? RecycleAccount { get; set; }
        [DisplayName("社保停交")]
        public virtual bool? SocialSecurityStop { get; set; }
        [DisplayName("其他福利停止")]
        public virtual bool? OtherBenefitsStop { get; set; }
        [DisplayName("工资结算")]
        public virtual bool? WageSettlement { get; set; }
        [DisplayName("借款清算")]
        public virtual bool? LoanSettlement { get; set; }
        [DisplayName("报销完结")]
        public virtual bool? ReimbursementEnd { get; set; }
        [Display(Name = "审核结果")]
        public virtual int ToExamineResult { get; set; }
        [DisplayName("离职类型")]
        public virtual int? TurnoverType { get; set; }
        [DisplayName("标识Id")]
        public virtual int Id { get; set; }
    }
}
