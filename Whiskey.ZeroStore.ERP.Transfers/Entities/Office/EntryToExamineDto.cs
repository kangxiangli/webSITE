using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities
{
    public class EntryToExamineDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
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

    }
}
