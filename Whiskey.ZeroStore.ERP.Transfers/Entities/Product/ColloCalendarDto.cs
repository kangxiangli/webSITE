using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ColloCalendarDto : IAddDto, IEditDto<int>
    {

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "会员搭配")]
        public int ColloId { get; set; }

        [Display(Name = "搭配时间")]
        public DateTime CollocationTime { get; set; }

        [Display(Name = "温度")]
        [StringLength(10)]
        public string Temperature { get; set; }

        [Display(Name = "天气")]
        [StringLength(10)]
        public string Weather { get; set; }

        [Display(Name = "城市")]
        [StringLength(5)]
        public virtual string CityName { get; set; }

        [Display(Name = "标识Id")]
        public Int32 Id { get; set; }
    }
}
