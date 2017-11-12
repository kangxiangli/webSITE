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
    public class ADepartmentPermissionRelationDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "实体标识")]
        public int Id { get; set; }
        public virtual int? DepartmentId { get; set; }
        public virtual int? PermissionsId { get; set; }
        [ForeignKey("PermissionsId")]
        public virtual Permission Permission { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }
        public virtual bool? IsShow { get; set; }
    }
}
