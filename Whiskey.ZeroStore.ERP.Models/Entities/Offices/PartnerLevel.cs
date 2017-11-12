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
    public class PartnerLevel : EntityBase<int>
    {
        
        [Display(Name = "等级名称")]
        [StringLength(20)]         
        public virtual string LevelName { get; set; }

        [Display(Name = "等级")]
        public virtual int Level { get; set; }
        
        [Display(Name = "经验值")]
        public virtual int Experience { get; set; }

        [Display(Name = "购买价格")]
        public virtual decimal Price { get; set; }

        [Display(Name = "优惠价数量")]
        public virtual int CouponQuantity { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
