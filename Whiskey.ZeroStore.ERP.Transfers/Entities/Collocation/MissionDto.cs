using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MissionDto : IAddDto, IEditDto<int>
    {
        public MissionDto()
        {
            Categorys = new List<Category>();
            MissionItems = new List<MissionItem>();
        }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "任务类型")]
        public virtual int MissionType { get; set; }

        [Display(Name = "任务属性类型")]
        public virtual int MissionAttrType { get; set; }

        [Display(Name = "任务进度")]
        public virtual int ScheduleType { get; set; }

        [Display(Name = "颜色")]
        public virtual int ColorId { get; set; }

        [Display(Name = "场合")]
        public virtual int SituationId { get; set; }

        [Display(Name = "季节")]
        public virtual int SeasonId { get; set; }

        [Display(Name = "风格")]
        public virtual int StyleId { get; set; }

        [Display(Name = "价格范围")]
        [StringLength(20)]
        [Required]
        public virtual string PriceRange { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }

        [Display(Name = "标识Id")]        
        public virtual Int32 Id { get; set; }

        public virtual ICollection<Category> Categorys { get; set; }

        public virtual ICollection<MissionItem> MissionItems { get; set; }
    }
}
