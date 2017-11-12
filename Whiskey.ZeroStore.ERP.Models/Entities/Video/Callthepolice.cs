using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class Callthepolice : EntityBase<int>
    {
        [Display(Name = "报警消息id")]
        public virtual string alarmId { get; set; }
        [Display(Name = "设备通道号")]
        public virtual int cid { get; set; }
        [Display(Name = "报警类型")]
        //0：人体红外   1：动态监测
        public virtual int type { get; set; }
        [Display(Name = "报警时间")]
        public virtual string time { get; set; }
        [Display(Name = "设备序列号")]
        public virtual string did { get; set; }
        [Display(Name = "通道名称")]
        public virtual string cname { get; set; }
    }
}
