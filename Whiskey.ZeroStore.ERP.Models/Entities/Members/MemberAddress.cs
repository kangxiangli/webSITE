



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class MemberAddress : EntityBase<int>
    {
        public MemberAddress()
        {

        }

        [Display(Name="所属会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "收件人")]
        [StringLength(25)]
        public virtual string Receiver { get; set; }

        [Display(Name = "所在省份")]
        public virtual int ProvinceId { get; set; }

        [Display(Name = "所在城市")]
        public virtual int CityId { get; set; }

        [Display(Name = "所在县/区")]
        public virtual int CountyId { get; set; }

        [Display(Name = "家庭住址")]
        [StringLength(50)]
        public virtual string HomeAddress { get; set; }

        [Display(Name = "固定电话")]
        [StringLength(13)]
        public virtual string Telephone { get; set; }

        [Display(Name = "手机号码")]
        [StringLength(13)]
        public virtual string MobilePhone { get; set; }

        [Display(Name = "邮政编码")]
        [StringLength(6)]
        public virtual string ZipCode { get; set; }

        [Display(Name = "默认地址")]
        public virtual bool IsDefault{get;set;}        

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("ProvinceId")]
        public virtual AreaItem Province { get; set; }

        [ForeignKey("CityId")]
        public virtual AreaItem City { get; set; }

        [ForeignKey("CountyId")]
        public virtual AreaItem County { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


    }
}


