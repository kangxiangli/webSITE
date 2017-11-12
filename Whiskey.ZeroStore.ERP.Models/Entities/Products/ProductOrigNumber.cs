using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Products;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    /// <summary>
    /// 商品基础
    /// </summary>
    public class ProductOriginNumber : EntityBase<int>
    {
        public ProductOriginNumber()
        {
            ProductAttributes = new List<ProductAttribute>();
            BuysaidAttributes = new List<BuysaidAttribute>();
            MaintainExtends = new List<MaintainExtend>();
            ProductImages = new List<ProductImage>();
            Products = new List<Product>();
            MemberColloEles = new List<MemberColloEle>();
            SalesCampaigns = new List<SalesCampaign>();
            ProdcutOrigNumberProductDetails = new List<ProdcutOrigNumberProductDetail>();
            RecommendMemberSingleProducts = new List<RecommendMemberSingleProduct>();
        }

        /// <summary>
        /// 商品款号 = 品牌2位+辅助号3位+品类2位
        /// </summary>
        [Display(Name = "商品款号")]
        [StringLength(10)]
        [Index(IsClustered = false, IsUnique = true)]
        public virtual string BigProdNum { get; set; }

        [Display(Name = "原始款号")]
        [StringLength(20, ErrorMessage = "原始款号不超过20个字符")]
        [Index(IsClustered = false, IsUnique = true)]
        public virtual string OriginNumber { get; set; }

        [Display(Name = "辅助号")]
        public string AssistantNum { get; set; }

        [Display(Name = "辅助号Int")]
        public int AssistantNumberOfInt { get; set; }

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

        [Display(Name = "商品主图")]
        [StringLength(100)]
        public String OriginalPath { get; set; }

        [Display(Name = "缩略图片")]
        [StringLength(100)]
        public String ThumbnailPath { get; set; }

        [Display(Name = "吊牌价格")]
        [Range(0, 1000000.00)]
        public float TagPrice { get; set; }

        /// <summary>
        /// 进货价格
        /// </summary>
        [Display(Name = "进货价格")]
        public float WholesalePrice { get; set; }

        [Display(Name = "采购价格")]
        [Range(0, 1000000.00)]
        public float PurchasePrice { get; set; }

        [Display(Name = "备注信息")]
        [StringLength(200)]
        public String Notes { get; set; }

        [Display(Name = "扩展属性")]
        [StringLength(200)]
        public String Extensions { get; set; }

        [Display(Name = "PC模板")]
        public int TemplateId { get; set; }

        //[Display(Name = "PC模板内容")]
        //public virtual string TemplateContent { get; set; }

        [Display(Name = "PC静态页路径")]
        [StringLength(100)]
        public virtual string HtmlPath { get; set; }

        [Display(Name = "手机模板")]
        public int? TemplatePhoneId { get; set; }

        //[Display(Name = "手机模板内容")]
        //public virtual string TemplatePhoneContent { get; set; }

        [Display(Name = "手机静态页路径")]
        [StringLength(100)]
        public virtual string HtmlPhonePath { get; set; }

        [Display(Name = "商品搭配图")]
        [StringLength(100)]
        public string ProductCollocationImg { get; set; }


        [Display(Name = "跳转链接")]
        [StringLength(200)]
        public virtual string JumpLink { get; set; }

        [Display(Name = "买手说内容")]
        public string BuysaidText { get; set; }

        [ForeignKey("CategoryId")]
        public virtual Category Category { get; set; }

        [ForeignKey("BrandId")]
        public virtual Brand Brand { get; set; }

        [ForeignKey("SeasonId")]
        public virtual Season Season { get; set; }

        [ForeignKey("CrowdId")]
        public virtual ProductCrowd ProductCrowd { get; set; }

        [Display(Name = "是否审核通过")]
        [Index]
        public virtual CheckStatusFlag IsVerified { get; set; }

        [Display(Name = "审核拒绝原因")]
        [StringLength(500)]
        public virtual string RefuseReason { get; set; }

        [Display(Name = "审核人ID")]
        public int? VerifiedMembId { get; set; }

        [Display(Name = "审核人")]
        [ForeignKey("VerifiedMembId")]
        public virtual Administrator VerifiedMemb { get; set; }

        #region 工厂样品管理

        [Display(Name = "是否已发布")]
        public virtual bool IsPublish { get; set; }

        #endregion


        /// <summary>
        /// 是否推荐,商城只显示被推荐款号的商品
        /// </summary>  
        [Display(Name = "是否推荐")]
        public virtual bool? IsRecommend { get; set; }


        /// <summary>
        /// 推荐时间,用于判断款号的新品状态
        /// </summary>
        [Display(Name = "推荐时间")]
        public DateTime? RecommendTime { get; set; }


        /// <summary>
        /// 推荐店铺
        /// </summary>
        [Display(Name = "推荐店铺")]
        public string RecommendStoreIds { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ProductOriginNumberTag ProductOriginNumberTag { get; set; }

        public virtual ICollection<BuysaidAttribute> BuysaidAttributes { get; set; }
        public virtual ICollection<MaintainExtend> MaintainExtends { get; set; }
        public virtual ICollection<ProductAttribute> ProductAttributes { get; set; }
        public virtual ICollection<ProductImage> ProductImages { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<MemberColloEle> MemberColloEles { get; set; }
        public virtual ICollection<SalesCampaign> SalesCampaigns { get; internal set; }
        /// <summary>
        /// 宝贝模板填写过的内容
        /// </summary>
        public virtual ICollection<ProdcutOrigNumberProductDetail> ProdcutOrigNumberProductDetails { get; set; }

        #region 设计师相关

        public virtual int? DesignerId { get; set; }

        [ForeignKey("DesignerId")]
        public virtual Designer Designer { get; set; }

        #endregion
        public virtual ICollection<RecommendMemberSingleProduct> RecommendMemberSingleProducts { get; set; }

    }

    public class ProductBigNumberAttr : EntityBase<int>
    {
        public string ProductNumber { get; set; }
    }
}