using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MobileInfoDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "会员Id")]
        public  int MemberId { get; set; }

        [Display(Name = "手机系统")]
        public  int MobileSystem { get; set; } //0:iOS;1Android

        [Display(Name = "系统版本")]
        [StringLength(20)]
        public  string SystemVersion { get; set; }

        [Display(Name = "App版本")]
        [StringLength(20)]
        public  string AppVersion { get; set; }

        [Display(Name = "手机型号")]
        [StringLength(20)]
        public  string MobileModel { get; set; }

        [Display(Name = "设备唯一标识")]
        [StringLength(120)]
        public  string DeviceToken { get; set; }

        [Display(Name = "主键Id")]
        public Int32 Id { get; set; }
    }
}
