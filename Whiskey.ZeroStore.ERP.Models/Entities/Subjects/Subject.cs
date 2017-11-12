
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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
    public class Subject : EntityBase<int>
    {
        public Subject()
        {
        }

        [Display(Name = "专题标题")]
        [StringLength(50)]
        [Required]
        public virtual string SubjectName { get; set; }


        [Display(Name = "专题路径")]
        [StringLength(400)]
        [Required]
        public virtual string Path { get; set; }

        [Display(Name="简介")]
        [StringLength(150)]
        public virtual string Summary { get; set; }

        [Display(Name = "父级专题")]
        public virtual int? ParentId { get; set; }

        #region 注释掉的字段
        //[Display(Name = "定义标签")]
        //public virtual string Tags { get; set; }

        //[Display(Name = "扩展属性")]
        //[Column(TypeName = "xml")]
        //public virtual string Extensions { get; set; }


        //[NotMapped]
        //public virtual XElement XmlExtensions
        //{
        //    get
        //    {
        //        return XElement.Parse(Extensions);
        //    }
        //    set
        //    {
        //        Extensions = value.ToString();
        //    }
        //}
        #endregion

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("ParentId")]
        public virtual Subject Parent { get; set; }

        public virtual ICollection<Subject> Children { get; set; }

        public virtual ICollection<SubjectAttribute> SubjectAttributes { get; set; }

    }
}


