using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class ADepartmentPermissionRelation : EntityBase<int>
    {
        public virtual int? DepartmentId { get; set; }
        public virtual int? PermissionsId { get; set; }
        [ForeignKey("PermissionsId")]
        public virtual Permission Permission { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        public virtual bool? IsShow { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
