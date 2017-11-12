using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ClassApplicationDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
        [DisplayName("员工Id")]
        public virtual int AdminId { get; set; }

        public virtual int Day { get; set; }

        public virtual int OffDay { get; set; }
        public virtual int SuccessionDepId { get; set; }
        public virtual int SuccessionId { get; set; }
        public virtual string desc { get; set; }
        [Display(Name = "审核结果")]
        public virtual int ToExamineResult { get; set; }
    }
}
