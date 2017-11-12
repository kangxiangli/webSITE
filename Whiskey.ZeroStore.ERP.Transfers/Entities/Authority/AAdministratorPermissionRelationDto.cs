using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AAdministratorPermissionRelationDto : IAddDto, IEditDto<int>
    {

        [Display(Name = "实体标识")]
        public int Id { get; set; }
        public virtual int? AdministratorId { get; set; }
        public virtual int? PermissionId { get; set; }

        [ForeignKey("AdministratorId")]
        public virtual Administrator Administrator { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }

        public virtual bool? IsShow { get; set; }
    }
}
