using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.StoreCollocation;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities.StoreCollocation
{
   public class StoreCollocationDto : IAddDto, IEditDto<int>
    {
        public StoreCollocationDto()
        {
            storeCollocationInfos = new List<StoreCollocationInfo>();
        }
        [Display(Name = "店铺")]
        public virtual string StoreId { get; set; }

        [Display(Name = "搭配名称")]
        [Required(ErrorMessage = "搭配名称不能为空")]
        [StringLength(15)]
        public virtual string CollocationName { get; set; }

        [Display(Name = "搭配风格")]
        public virtual string Styles { get; set; }

        [Display(Name = "场合")]
        public virtual string Situation { get; set; }

        [Display(Name = "颜色")]
        public virtual string Colour { get; set; }

        [Display(Name = "季节")]
        public virtual string Season { get; set; }

        [Display(Name = "体形")]
        [StringLength(50)]
        public virtual string Shape { get; set; }
        [Display(Name = "效果")]
        public virtual string Effect { get; set; }

        [Display(Name = "图片")]
        public virtual string ThumbnailPath { get; set; }

        [Display(Name = "是否推荐")]
        public virtual bool IsRecommend { get; set; }

        [Display(Name = "标识")]
        public virtual string Guid { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }


        [Display(Name = "实体标识")]
        public virtual Int32 Id { get; set; }

        public virtual ICollection<StoreCollocationInfo> storeCollocationInfos { get; set; }
    }
}
