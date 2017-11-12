
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class Gallery : EntityBase<int>
    {
        public Gallery()
        {
           PictureName = "";
           Description = "";
           OriginalPath = "";
           ThumbnailPath = "";
           Tags = "";
           Colors = new List<Color>();
           //GalleryAttributes = new List<GalleryAttribute>();
        }

        [Display(Name = "图片名称")]
        public virtual string PictureName { get; set; }

        [Display(Name = "图片类型")]
        public virtual GalleryFlag GalleryType { get; set; }

        [Display(Name = "图片描述")]
        public virtual string Description { get; set; }

        [Display(Name = "定义标签")]
        public virtual string Tags { get; set; }

        [Display(Name = "原始图片")]
        public virtual string OriginalPath { get; set; }

        [Display(Name = "缩略图片")]
        public virtual string ThumbnailPath { get; set; }

        //yxk 2015-9-8
        /// <summary>
        /// 存放图片属性id，id之间用逗号隔开，比如：1,2,3,
        /// </summary>
        public string AttributeId { get; set; }


        [Display(Name = "扩展属性")]
        [Column(TypeName = "xml")]
        [StringLength(200)]
        public virtual string Extensions { get; set; }


        [NotMapped]
        public virtual XElement XmlExtensions
        {
            get
            {
                return XElement.Parse(Extensions);
            }
            set
            {
                Extensions = value.ToString();
            }
        }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
      

        public virtual ICollection<Color> Colors { get; set; }

        public virtual ICollection<GalleryAttribute> GalleryAttributes { get; set; }




    }
}


