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
    public class PartnerLevelOrder : EntityBase<int>
    {

        [Display(Name = "合作商")]
        public virtual int PartnerId { get; set; }

        [Display(Name = "购买等级")]
        public virtual int LevelId { get; set; }

        [Display(Name = "购买价格")]
        public virtual decimal Price { get; set; }        

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
