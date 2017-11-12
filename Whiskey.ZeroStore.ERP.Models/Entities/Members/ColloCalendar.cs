using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员搭配日历
    /// </summary>
    [Serializable]
    public class ColloCalendar : EntityBase<int>
    {
        public ColloCalendar()
        {
             
        }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "会员搭配")]
        public virtual int ColloId { get; set; }

        [Display(Name = "搭配时间")]
        public virtual DateTime CollocationTime { get; set; }

        [Display(Name = "温度")]
        [StringLength(30)]
        public virtual string Temperature { get; set; }

        [Display(Name = "天气")]
        [StringLength(30)]
        public virtual string Weather { get; set; }

        [Display(Name = "城市")]
        [StringLength(30)]
        public virtual string CityName { get; set; }

        [ForeignKey("ColloId")]
        public virtual MemberCollocation MemberColl { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
