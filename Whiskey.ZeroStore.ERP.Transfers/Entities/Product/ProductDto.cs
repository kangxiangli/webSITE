using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    [Serializable]
    public class ProductDto : IAddDto, IEditDto<int>
    {
        /// <summary>
        /// 商品款号=品牌2+辅助号3+品类2
        /// </summary>
        [Display(Name = "商品款号")]
        public virtual string BigProdNum { get; set; }

        /// <summary>
        /// 原始款号=出厂商吊牌上的款号
        /// </summary>
        [Display(Name = "原始款号")]
        public virtual string OriginNumber { get; set; }

        /// <summary>
        /// 商品货号=商品款号+颜色+尺码
        /// </summary>
        [Display(Name = "商品货号")]
        public virtual string ProductNumber { get; set; }

        [Display(Name = "商品尺码")]
        [Required]
        [Range(1, 1000000)]
        public Int32 SizeId { get; set; }

        [Display(Name = "商品颜色")]
        [Required]
        [Range(1, 1000000)]
        public Int32 ColorId { get; set; }

        [Display(Name = "商品识别")]
        public virtual Guid ProductGuid { get; set; }

        [Display(Name = "商品类型")]
        public virtual int ProductType { get; set; }//0：直营档案，1：第三方档案

		[Display(Name = "商品标题")]
		public String ProductName { get; set; }

		[Display(Name = "商品主图")]
		public String OriginalPath { get; set; }

		[Display(Name = "缩略图片")]
		public String ThumbnailPath { get; set; }

		[Display(Name = "浏览次数")]
		public Int64 Hits { get; set; }

		[Display(Name = "销售总数")]
		public Int32 Sales { get; set; }

		[Display(Name = "扩展属性")]
        public String Extensions { get; set; }

		[Display(Name = "实体标识")]
		public Int32 Id { get; set; }

		[Display(Name = "排序序号")]
		public Int32 Sequence { get; set; }
       
        [Display(Name="商品搭配图")]
        public string ProductCollocationImg { get; set; }
    }
}
