using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Whiskey.ZeroStore.ERP.Models
{
   public class EntryToExamine : EntityBase<int>
    {
        [DisplayName("入职信息标识")]
        public virtual int EntryId { get; set; }

        [DisplayName("审核人")]
        public virtual int AdminId { get; set; }

        [DisplayName("审核状态")]
        public virtual int AuditStatus { get; set; }

        [DisplayName("审核时间")]
        public virtual DateTime AuditTime { get; set; }

        [DisplayName("备注")]
        public virtual string Reason { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Administrator { get; set; }
    }
}
