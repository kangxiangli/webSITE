
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 实体模型
    /// </summary>
    [Serializable]
    public class GalleryAttribute : EntityBase<int>
    {
        public GalleryAttribute()
        {
            AttributeName = "";
            Description = "";
            AttributeLevel = 0;
            Galleries = new List<Gallery>();
            Children = new List<GalleryAttribute>();
        }


        [Display(Name = "父级属性")]
        [Index]
        public virtual int? ParentId { get; set; }

        [Display(Name = "属性名称")]
        [Required(ErrorMessage = "不能为空")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "至少{2}～{1}个字符")]
        public virtual string AttributeName { get; set; }

        [Display(Name = "属性描述")]
        [StringLength(120,ErrorMessage="最大字符长度不能超过{1}")]
        public virtual string Description { get; set; }

        [Display(Name = "属性层级")]
        public virtual int AttributeLevel { get; set; }

        [Display(Name = "图片路径")]
        [StringLength(100, ErrorMessage = "最大字符长度不能超过{1}")]
        public virtual string IconPath { get; set; }

        [ForeignKey("OperatorId")]
        [Index]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("ParentId")]
        [Index]
        public virtual GalleryAttribute Parent { get; set; }

        public virtual ICollection<Gallery> Galleries { get; set; }

        public virtual ICollection<GalleryAttribute> Children { get; set; }

    }
}


