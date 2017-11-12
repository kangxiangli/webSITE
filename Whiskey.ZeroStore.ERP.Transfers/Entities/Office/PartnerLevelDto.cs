using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;


namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class PartnerLevelDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "等级名称")]
        [StringLength(20,ErrorMessage="字符长度不能超过{1}")]
        [Required(ErrorMessage="请填写名称")]
        public virtual string LevelName { get; set; }

        [Display(Name = "等级")]
        [Required(ErrorMessage = "请填写等级")]
        public virtual int Level { get; set; }

        [Display(Name = "经验值")]
        [Required(ErrorMessage = "请填写升级所需经验值")]
        public virtual int Experience { get; set; }

        [Display(Name = "购买价格")]
        [Required(ErrorMessage = "请填写购买价格")]
        public virtual decimal Price { get; set; }

        [Display(Name = "优惠价数量")]
        [Required(ErrorMessage = "请填写可发放优惠价数量")]
        public virtual int CouponQuantity { get; set; }

        [Display(Name = "标识Id")]
        public int Id { get; set; }
    }
}
