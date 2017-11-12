using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class Group : EntityBase<int>
    {
        [Display(Name = "组名称")]
        public virtual string GroupName { get; set; }
        [Display(Name = "组描述")]
        public virtual string Description { get; set; }
        public virtual ICollection<Administrator> Admins { get; set; }
        public virtual ICollection<Role> Roles { get; set; }
        //public virtual ICollection<Permission> Permissions { get; set; }
        //public virtual ICollection<AGroupPermissionRelation> AGroupPermissionRelations { get; set; }
    }
}
