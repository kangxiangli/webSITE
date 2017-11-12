using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class SignRuleDto : IAddDto, IEditDto<int>,ICloneable
    {        
        [Display(Name = "规则名称")]
        [Required(ErrorMessage = "不能为空")]
        [StringLength(20, ErrorMessage = "最大长度超过{0}")]
        public virtual string SignRuleName { get; set; }

        [Display(Name = "第x天")]
        [Required(ErrorMessage="请选择天数")]
        public virtual int Week { get; set; }

        [Display(Name = "奖品类型")]
        public virtual int PrizeType { get; set; }

        [Display(Name = "优惠券")]
        public virtual int? CouponId { get; set; }

        [Display(Name = "优惠券名称")]
        [Required(ErrorMessage="请选择优惠券")]
        public virtual string CouponName { get; set; }

        [Display(Name = "奖品")]
        public virtual int? PrizeId { get; set; }

        [Display(Name = "奖品名称")]
        [Required(ErrorMessage = "请选择奖品")]
        public virtual string PrizeName { get; set; }

        [Display(Name = "备注")]
        [StringLength(120, ErrorMessage = "最大长度超过{0}")]
        public virtual string Notes { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <returns></returns>
        public SignRuleDto DeepClone()
        {
            
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, this);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as SignRuleDto;
            }
        }

        /// <summary>
        /// 浅复制
        /// </summary>
        /// <returns></returns>
        public SignRuleDto ShallowClone()
        {
            return Clone() as SignRuleDto;
        }
    }
}
