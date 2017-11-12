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
    public class AAdministratorPermissionRelation : EntityBase<int>
    {
        public virtual int? AdministratorId { get; set; }
        public virtual int? PermissionId { get; set; }

        [ForeignKey("AdministratorId")]
        public virtual Administrator Administrator { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }

        public virtual bool? IsShow { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
