
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Role : EntityBase<int>
    {
        public Role() {
            RoleName = "";
            Description = "";
            Weight = 0;
            Administrators = new List<Administrator>();
            //Permissions = new List<Permission>();
            ARolePermissionRelations = new List<ARolePermissionRelation>();
        }
			

        [Display(Name = "角色名称")]
        [Required, StringLength(50)]
        public virtual string RoleName { get; set; }

        [Display(Name = "角色简介")]
        public virtual string Description { get; set; }

        [Display(Name = "角色权重")]
        public virtual int Weight { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Administrator> Administrators { get; set; }

        //public virtual ICollection<Permission> Permissions { get; set; }
        //public virtual ICollection<Group> Groups { get; set; }

        public virtual ICollection<ARolePermissionRelation> ARolePermissionRelations { get; set; }
    }
}


