
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
    public class Location : EntityBase<int>
    {
        public Location() {
            Products = new List<Product>();
            Children = new List<Brand>();
        }

        [Display(Name = "父级分类")]
        public virtual int ParentId { get; set; }

        [Display(Name = "货位名称")]
        public virtual string LocationName { get; set; }

        [Display(Name = "货位编码")]
        public virtual string LocationCode { get; set; }

        [Display(Name = "货位层级")]
        public virtual int LocationLevel { get; set; }


        [ForeignKey("ParentId")]
        public virtual Brand Parent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<Brand> Children { get; set; }

        public virtual ICollection<Product> Products { get; set; }


    }
}


