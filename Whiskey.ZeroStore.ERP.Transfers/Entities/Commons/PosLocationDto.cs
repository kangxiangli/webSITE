
using System;
using Whiskey.Core.Data;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PosLocationDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "设备标识")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string IMEI { get; set; }

        /// <summary>
        /// 上次经度
        /// </summary>
        [Display(Name = "上次经度")]
        public virtual double PrevLongitude { get; set; }
        /// <summary>
        /// 上次纬度
        /// </summary>
        [Display(Name = "上次纬度")]
        public virtual double PrevLatitude { get; set; }

        /// <summary>
        /// 上次地理位置
        /// </summary>
        [Display(Name = "上次位置")]
        public virtual string PrevAddress { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        [Display(Name = "经度")]
        public virtual double Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        [Display(Name = "纬度")]
        public virtual double Latitude { get; set; }

        /// <summary>
        /// 地理位置
        /// </summary>
        [Display(Name = "位置")]
        public virtual string Address { get; set; }
        /// <summary>
        /// 上次更新时间
        /// </summary>
        [Display(Name = "上次记录时间")]
        public virtual DateTime PrevUpdatedTime { get; set; }

    }
}


