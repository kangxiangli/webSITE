using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Models
{
    public class M_Address
    {

        /// <summary>
        /// 地址
        /// </summary>
        public int AddressId { get; set; }

        /// <summary>
        ///会员
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string ProvinceId { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string CityId { get; set; }

        /// <summary>
        /// 县
        /// </summary>
        public string CountyId { get; set; }

        /// <summary>
        /// 收件人
        /// </summary>
        public string Receiver { get; set; }

        /// <summary>
        /// 家庭住址
        /// </summary>
        public string HomeAddress { get; set; }

        /// <summary>
        /// 固定电话
        /// </summary>
        public string Telephone { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string MobilePhone { get; set; }
        
        /// <summary>
        /// 邮编
        /// </summary>
        public string ZipCode { get; set; }

        /// <summary>
        /// 是否为默认
        /// </summary>
        public virtual int IsDefault { get; set; } 
    }
}