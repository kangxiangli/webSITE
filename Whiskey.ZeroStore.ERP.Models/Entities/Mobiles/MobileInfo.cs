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
    [Serializable]
    public class MobileInfo: EntityBase<int>
    {
        [Display(Name = "会员Id")]
        public virtual int MemberId { get; set; }

        [Display(Name = "手机系统")]
        public virtual RegisterFlag MobileSystem { get; set; }

        [Display(Name = "系统版本")]
        [StringLength(20)]
        public virtual string SystemVersion { get; set; }

        [Display(Name = "App版本")]
        [StringLength(20)]
        public virtual string AppVersion { get; set; }

        [Display(Name = "手机型号")]
        [StringLength(20)]
        public virtual string MobileModel { get; set; }

        [Display(Name = "设备唯一标识")]
        [StringLength(50)]
        public virtual string DeviceToken { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
