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
    /// <summary>
    /// 搭配实体
    /// </summary>
    [Serializable]
    public class Collocation : EntityBase<int>
    {
        [Display(Name="搭配师编号")]
        public virtual string Numb { get; set; }
        /// <summary>
        /// 别名，与真实名称和昵称有区别，只是保留字段未使用
        /// </summary>
        [Display(Name="别名")]
        public virtual string CollocationName { get; set; }        

        [Display(Name="备注")]
        public virtual string Notes { get; set; }
       
        /// <summary>
        /// 搭配师状态 0：未预约 1：已经被预约
        /// </summary>
        [Display(Name="状态")]
        public virtual int State{get;set;}
        public virtual int AdminiId { get; set; }
        [ForeignKey("AdminiId")]
        public virtual Administrator Admini { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
        public virtual ICollection<EarningsDetail> EarningsDetail { get; set; }
    }
}
