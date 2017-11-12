using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    [Serializable]
    public class Discount:EntityBase<int>
    {
        [Display(Name="折扣名称")]
        [Required,StringLength(10)]
        public virtual string DiscountName { get; set; }
        [Display(Name="折扣率")]
        [Required]
        public virtual double DiscountRate { get; set; }
    }
}
       