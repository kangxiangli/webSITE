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
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class MemberSingleProduct : EntityBase<int>
    {
        public MemberSingleProduct()
        {
            CoverImage = string.Empty;            
            Price = 0;
            Brand = string.Empty;                      
        }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "单品名称")]
        [Required(ErrorMessage="不能为空！")]
        [MaxLength(100)]
        public virtual string ProductName { get; set; }

        [Display(Name = "封面图片")]
        [StringLength(100)]
        public virtual string CoverImage { get; set; }

        [Display(Name = "图片")]
        [StringLength(100)]
        public virtual string Image { get; set; }

        [Display(Name = "颜色名称")]        
        public virtual int ColorId { get; set; }

        [Display(Name = "尺码")]        
        public virtual int SizeId { get; set; }

        [Display(Name = "季节")]        
        public virtual int SeasonId { get; set; }

        [Display(Name = "风格")]
        public virtual int ProductAttrId { get; set; }

        [Display(Name = "分类")]
        public virtual int CategoryId { get; set; } 

        [Display(Name = "价格")]
        public virtual decimal Price { get; set; }

        [Display(Name = "品牌")]
        [StringLength(15)]
        public virtual string Brand { get; set; }
                

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
