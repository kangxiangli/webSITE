using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class OvertimeRestItem : EntityBase<int>
    {
        [DisplayName("加班")]
        public virtual int OvertimeId { get; set; }

        [ForeignKey("OvertimeId")]
        public virtual Overtime Overtime { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
