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
    public class JobPosition : EntityBase<int>
    {
        public JobPosition()
        {
            Departments = new List<Department>();
            Administrators = new List<Administrator>();
            Factorys = new List<Factorys>();
            AppVerManages = new List<AppVerManage>();
        }
        [DisplayName("部门")]
        [Required(ErrorMessage = "请选择部门")]
        public virtual int DepartmentId { get; set; }

        [DisplayName("工作时间")]
        [Required(ErrorMessage = "请选择工作时间")]
        public virtual int WorkTimeId { get; set; }

        [DisplayName("年假")]
        [Required(ErrorMessage = "请选择年假")]
        public virtual int AnnualLeaveId { get; set; }

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
        [DisplayName("审核")]
        public virtual int? Auditauthority { get; set; }
        [DisplayName("职位")]
        [Required(ErrorMessage = "职位名称不能为空")]
        [StringLength(15, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string JobPositionName { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string Notes { get; set; }

        [ForeignKey("WorkTimeId")]
        public virtual WorkTime WorkTime { get; set; }

        [ForeignKey("AnnualLeaveId")]
        public virtual AnnualLeave AnnualLeave { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        public virtual ICollection<Department> Departments { get; set; }

        public virtual ICollection<Administrator> Administrators { get; set; }
        /// <summary>
        /// 职位可管理的工厂
        /// </summary>
        public virtual ICollection<Factorys> Factorys { get; set; }
        /// <summary>
        /// 授权APP
        /// </summary>
        public virtual ICollection<AppVerManage> AppVerManages { get; set; }

    }
}
