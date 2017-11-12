using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class ProductOriginNumberDto
    {
        [Display(Name = "商品款号")]
        [StringLength(10)]
        public virtual string BigProdNum { get; set; }

        [Display(Name = "辅助号")]
        public virtual string AssistantNum { get; set; }

        [Display(Name = "辅助号Int")]
        public virtual int AssistantNumberOfInt { get; set; }

        [Display(Name = "原始款号")]
        [StringLength(20, ErrorMessage = "原始款号不超过20个字符")]
        public virtual string OriginNumber { get; set; }

        [Display(Name = "商品品类")]
        [Range(1, 1000000)]
        public Int32 CategoryId { get; set; }

        [Display(Name = "商品品牌")]
        [Range(1, 1000000)]
        public Int32 BrandId { get; set; }

        [Display(Name = "商品季节")]
        [Range(1, 1000000)]
        public Int32 SeasonId { get; set; }

        [Display(Name = "面向人群")]
        [DefaultValue(1)]
        public virtual int CrowdId { get; set; }

        [Display(Name = "商品类型")]
        public virtual int ProductType { get; set; }//0：直营档案，1：第三方档案

        [Display(Name = "商品标题")]
        [StringLength(50, ErrorMessage = "不超过50字符")]
        public String ProductName { get; set; }

        [Display(Name = "促销标题")]
        [StringLength(100)]
        public String Summary { get; set; }

        [Display(Name = "商品介绍")]
        public String Description { get; set; }

        [Display(Name = "吊牌价格")]
        [Range(0, 1000000.00)]
        public float TagPrice { get; set; }

        [Display(Name = "批发价格")]
        public float WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        [Range(0, 1000000.00)]
        public float PurchasePrice { get; set; }


        [Display(Name = "备注信息")]
        [StringLength(200)]
        public String Notes { get; set; }

        [Display(Name = "PC模板Id")]
        public int TemplateId { get; set; }

        [Display(Name = "PC静态页路径")]
        [StringLength(100)]
        public virtual string HtmlPath { get; set; }

        [Display(Name = "手机模板Id")]
        public int TemplatePhoneId { get; set; }

        [Display(Name = "手机静态页路径")]
        [StringLength(100)]
        public virtual string HtmlPhonePath { get; set; }

        [Display(Name = "跳转链接")]
        [StringLength(200)]
        public virtual string JumpLink { get; set; }

        [Display(Name = "买手说内容")]
        public string BuysaidText { get; set; }

        [Display(Name = "是否审核通过")]
        public virtual bool IsVerified { get; set; }

        [Display(Name = "是否推荐")]
        public virtual bool? IsRecommend { get; set; }
    }
}
