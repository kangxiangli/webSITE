using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Collocations;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class MissionItem : EntityBase<int>
    {
        [Display(Name = "任务")]
        public virtual int MissionId { get; set; }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "任务完成类型")]
        public virtual int MissionAttrType { get; set; }

        [Display(Name = "任务进度")]
        public virtual int ScheduleType { get; set; }

        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }

        [Display(Name = "结束时间")]
        public virtual DateTime? EndTime { get; set; }        

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("MissionId")]
        public virtual Mission Mission { get; set; }
    }
}
