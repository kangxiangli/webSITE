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
  public  class ClassApplication : EntityBase<int>
    {
        [DisplayName("员工Id")]
        public virtual int AdminId { get; set; }

        public virtual int Day { get; set; }

        public virtual int OffDay { get; set; }

        public virtual int SuccessionDepId { get; set; }
        public virtual int SuccessionId { get; set; }
        [Display(Name = "审核结果")]
        public virtual int ToExamineResult { get; set; }

        public virtual string desc { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Admin { get; set; }
        [ForeignKey("SuccessionId")]
        public virtual Administrator Succession { get; set; }
        [ForeignKey("SuccessionDepId")]
        public virtual Department SuccessionDep { get; set; }
    }
}
