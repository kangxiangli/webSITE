
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
    public class SubjectAttribute : EntityBase<int>
    {
        public SubjectAttribute()
        {
            AttributeName = "";
            Description = "";
            AttributeLevel = 0;
            Subjects = new List<Subject>();
            Children = new List<SubjectAttribute>();
        }


        [Display(Name = "父级分类")]
        [Index]
        public virtual int? ParentId { get; set; }

        [Display(Name = "分类名称")]
        public virtual string AttributeName { get; set; }

        [Display(Name = "分类描述")]
        public virtual string Description { get; set; }

        [Display(Name = "分类层级")]
        public virtual int AttributeLevel { get; set; }



        [ForeignKey("OperatorId")]
        [Index]
        public virtual Administrator Operator { get; set; }

        [ForeignKey("ParentId")]
        [Index]
        public virtual SubjectAttribute Parent { get; set; }

        public virtual ICollection<Subject> Subjects { get; set; }

        public virtual ICollection<SubjectAttribute> Children { get; set; }

    }
}


