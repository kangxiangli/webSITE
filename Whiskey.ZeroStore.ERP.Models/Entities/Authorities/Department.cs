using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;


namespace Whiskey.ZeroStore.ERP.Models
{
    //yxk 2015-9-16
    /// <summary>
    /// 部门实体
    /// </summary>
    [Serializable]
    public class Department : EntityBase<int>
    {
        public Department()
        {
            Stores = new List<Store>();
            JobPositions = new List<JobPosition>();
        }

        [Display(Name = "上级部门")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "部门名称")]
        [StringLength(20,ErrorMessage="最长字符不能超过{1}")]
        [Required(ErrorMessage="请填写部门")]
        public string DepartmentName { get; set; }

        [Display(Name = "部门描述")]
        [StringLength(120, ErrorMessage = "最长字符不能超过{1}")]
        public string Description { get; set; }

        //[Display(Name = "部门负责人")]
        //public virtual int? AdministratorId { get; set; }

        [Display(Name = "下级部门负责人")]
        public virtual int? SubordinateId { get; set; }

        [Display(Name = "Mac地址")]
        [StringLength(70, ErrorMessage = "最长字符不能超过{1}")]        
        public string MacAddress { get; set; }

        [Display(Name = "部门类型")]
        public DepartmentTypeFlag DepartmentType { get; set; }

        [ForeignKey("ParentId")]
        public virtual Department Parent { get; set; }

        //[ForeignKey("AdministratorId")]
        //public Administrator Administrator { get; set; }

        [ForeignKey("SubordinateId")]
        public virtual Administrator Subordinate { get; set; }

        public virtual ICollection<Administrator> Administrators { get; set; }
        public virtual ICollection<Store> Stores{ get; set; }
        //public virtual ICollection<Permission> Permissions { get; set; }
        public virtual ICollection<AnnualLeave> AnnualLeaves { get; set; }

        public virtual ICollection<Department> Children { get; set; }

        public virtual ICollection<JobPosition> JobPositions { get; set; }

        public virtual ICollection<Attendance> Attendances { get; set; }

        //public virtual ICollection<ADepartmentPermissionRelation> ADepartmentPermissionRelations { get; set; }
    }
}
