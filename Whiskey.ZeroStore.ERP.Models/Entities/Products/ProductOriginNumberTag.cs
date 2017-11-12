using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 商品吊牌属性
    /// </summary>
    public class ProductOriginNumberTag : EntityBase<int>
    {
        [Display(Name = "执行标准")]
        [StringLength(200)]
        public virtual string Standard { get; set; }

        [Display(Name = "等级")]
        [StringLength(20)]
        public virtual string Level { get; set; }

        [Display(Name = "检验员")]
        [StringLength(20)]
        public virtual string Inspector { get; set; }

        [Display(Name = "地址")]
        [StringLength(100)]
        public virtual string ProductionPlace { get; set; }

        [Display(Name = "安全类别")]
        [StringLength(50)]
        public virtual string Category { get; set; }

        [Display(Name = "面料")]
        [StringLength(100)]
        public virtual string Fabric { get; set; }

        [Display(Name = "里料")]
        [StringLength(100)]
        public virtual string Material { get; set; }

        [Display(Name = "配料")]
        [StringLength(100)]
        public virtual string batching { get; set; }

        [Display(Name = "填充物")]
        [StringLength(100)]
        public virtual string Stuffing { get; set; }

        [Display(Name = "水洗符号")]
        [StringLength(200)]
        public virtual string WashingSymbols { get; set; }

        [Display(Name = "制造商")]
        [StringLength(100)]
        public virtual string Manufacturer { get; set; }

        [Display(Name = "邮编")]
        [StringLength(20)]
        public virtual string PostCode { get; set; }

        [Display(Name = "品名")]
        [StringLength(20)]
        public virtual string CateName { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
