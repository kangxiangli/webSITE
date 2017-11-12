using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
   public class ResignationToExamineDto : IAddDto, IEditDto<int>
    {
        [DisplayName("离职信息标识")]
        public virtual int ResignationId { get; set; }

        [DisplayName("审核人")]
        public virtual int AdminId { get; set; }

        [DisplayName("审核状态")]
        public virtual int AuditStatus { get; set; }

        [DisplayName("审核时间")]
        public virtual DateTime AuditTime { get; set; }

        [DisplayName("备注")]
        public virtual string Reason { get; set; }
        [DisplayName("标识Id")]
        public virtual int Id { get; set; }
    }
}
