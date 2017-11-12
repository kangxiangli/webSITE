using System;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    [Serializable]
    public class ProductAttributeImage : EntityBase<int>
    {
        public ProductAttributeImage()
        {
            OriginalPath = "";
        }

        [Display(Name = "原始图片")]
        [StringLength(200)]
        public virtual string OriginalPath { get; set; }

        public virtual ProductAttribute ProductAttribute { get; set; }
    }
}
