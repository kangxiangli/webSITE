
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
    public class ProductImage : EntityBase<int>
    {
        public ProductImage()
        {
            OriginalPath = "";
            ThumbnailSmallPath = "";
            ThumbnailMediumPath = "";
            ThumbnailLargePath = "";
        }

        [Display(Name = "原始图片")]
        [StringLength(200)]
        public virtual string OriginalPath { get; set; }

        [Display(Name = "小缩略图")]
        [StringLength(200)]
        public virtual string ThumbnailSmallPath { get; set; }

        [Display(Name = "中缩略图")]
        [StringLength(200)]
        public virtual string ThumbnailMediumPath { get; set; }

        [Display(Name = "大缩略图")]
        [StringLength(200)]
        public virtual string ThumbnailLargePath { get; set; }

        public virtual List<Product> Products { get; set; }

        public virtual List<ProductOriginNumber> ProductOrginNumbers { get; set; }
    }
}


