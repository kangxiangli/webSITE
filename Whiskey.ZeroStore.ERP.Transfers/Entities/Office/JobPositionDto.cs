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
    public class JobPositionDto : IAddDto, IEditDto<int>
    {
        [DisplayName("部门")]
        [Required(ErrorMessage = "请选择部门")]
        public virtual int DepartmentId { get; set; }

        [DisplayName("部门名称")]
        [Required(ErrorMessage = "请选择部门")]
        public virtual string DepartmentName { get; set; }

        [DisplayName("工作时间")]
        [Required(ErrorMessage = "请选择工作时间")]
        public virtual int WorkTimeId { get; set; }

        [DisplayName("工作时间名称")]
        [Required(ErrorMessage = "请选择工作时间")]
        public virtual string WorkTimeName { get; set; }

        [DisplayName("年假")]      
        public virtual int AnnualLeaveId { get; set; }

        [DisplayName("年假名称")]
        [Required(ErrorMessage = "请选择年假")]
        public virtual string AnnualLeaveName { get; set; }

        [DisplayName("主管")]
        public virtual bool IsLeader { get; set; }
        //[DisplayName("蝶掌柜")]
        //public virtual bool IsShopkeeper { get; set; }
        [DisplayName("密码登录")]
        public virtual bool AllowPwd { get; set; }
        [DisplayName("登录检测")]
        [DefaultValue(true)]
        public virtual bool CheckLogin { get; set; }

        //[DisplayName("是否设计师")]
        //public virtual bool IsDesigner { get; set; }

        [DisplayName("MAC检测")]
        [DefaultValue(true)]
        public virtual bool CheckMac { get; set; }

        [DisplayName("职位")]
        [Required(ErrorMessage = "职位名称不能为空")]
        [StringLength(15, ErrorMessage = "不能超过最大长度{0}")]
        public virtual string JobPositionName { get; set; }

        [DisplayName("审核")]
        public virtual int? Auditauthority { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{0}")]
        public virtual string Notes { get; set; }

        [DisplayName("可查看部门")]
        public virtual List<int> DepartIds { get; set; }

        [DisplayName("可查看工厂")]
        public virtual List<int> FactoryIds { get; set; }

        [DisplayName("授权App")]
        public virtual List<int> AppVerIds { get; set; }

        [DisplayName("标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
