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
    [Serializable]
    public class ARolePermissionRelation : EntityBase<int>
    {

        public virtual int? RoleId { get; set; }
        public virtual int? PermissionsId { get; set; }

        [ForeignKey("PermissionsId")]
        public virtual Permission Permission { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }
        public virtual bool? IsShow { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
