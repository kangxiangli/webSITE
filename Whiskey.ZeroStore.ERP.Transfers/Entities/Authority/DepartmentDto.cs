using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    //yxk 2015-9-16
    /// <summary>
    /// 部门实体
    /// </summary>
    [Serializable]
    public class DepartmentDto : IAddDto, IEditDto<int>
    {
        public DepartmentDto()
        {
            IsDeleted = false;
            IsEnabled = true;
            //ADepartmentPermissionRelations = new List<ADepartmentPermissionRelation>();
        }

        [Display(Name = "部门id")]
        public int Id { get; set; }

        [Display(Name = "上级部门")]
        public virtual int? ParentId { get; set; }

        [Display(Name = "部门名称")]
        [StringLength(20, ErrorMessage = "最长字符不能超过{1}")]
        [Required(ErrorMessage = "请填写部门")]
        public string DepartmentName { get; set; }

        [Display(Name = "部门描述")]
        [StringLength(120, ErrorMessage = "最长字符不能超过{1}")]
        public string Description { get; set; }

        //[Display(Name = "部门负责人")]
        //public int? AdministratorId { get; set; }

        //[Display(Name = "下级部门负责人")]
        //public virtual int? SubordinateId { get; set; }

        //[Display(Name = "负责部门")]
        //public virtual string DepartmentIds { get; set; }

        [Display(Name = "Mac地址")]
        [StringLength(70, ErrorMessage = "最长字符不能超过{1}")]
        public string MacAddress { get; set; }

        [Display(Name = "部门类型")]
        public DepartmentTypeFlag DepartmentType { get; set; }
     
        public bool IsDeleted {get;set;}

        public bool IsEnabled{get;set;}

        ///// <summary>
        ///// 获取或设置 更新时间
        ///// </summary>
        //[Display(Name = "更新时间")]
        //public DateTime UpdatedTime { get; set; }

        ///// <summary>
        ///// 获取或设置 创建时间
        ///// </summary>
        //[Display(Name = "创建时间")]
        //public DateTime CreatedTime { get; set; }

        /// <summary>
        /// 操作员ID
        /// </summary>
        [Display(Name = "操作人员")]
        public int? OperatorId { get; set; }

        //public virtual ICollection<ADepartmentPermissionRelation> ADepartmentPermissionRelations { get; set; }
    }
}
