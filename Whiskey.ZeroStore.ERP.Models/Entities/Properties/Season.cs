
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Entities;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
	[Serializable]
    public class Season : EntityBase<int>
    {
        public Season() {
            SeasonName = "";
            SeasonCode = "";
            Products = new List<ProductOriginNumber>();
        }

        [Display(Name = "季节名称")]
        [Required, StringLength(8)]
        public virtual string SeasonName { get; set; }

        [Display(Name = "季节编码")]
        [StringLength(2, ErrorMessage = "{0}~{1}个字符！")]
        [Required(ErrorMessage = "编码不能为空！")]
        [RegularExpression(@"[A-Za-z0-9]+", ErrorMessage = "编码必须是数字或者字母")]
        public virtual string SeasonCode { get; set; }

        [Display(Name = "季节图标")]
        [StringLength(100)]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<ProductOriginNumber> Products { get; set; }

    }
}


