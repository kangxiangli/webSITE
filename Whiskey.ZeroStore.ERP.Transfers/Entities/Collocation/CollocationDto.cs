using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class CollocationDto : IAddDto, IEditDto<int>
    {
       

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "搭配师编号")]
        public virtual string Numb { get; set; }
        [Display(Name = "别名")]
        public virtual string CollocationName { get; set; }
        public virtual string Notes { get; set; }
        /// <summary>
        /// 搭配师状态 0：未预约 1：已经被预约
        /// </summary>
        public virtual int State { get; set; }
      
    }
}
