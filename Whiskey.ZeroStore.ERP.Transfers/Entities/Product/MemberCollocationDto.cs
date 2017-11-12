using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberCollocationDto : IAddDto, IEditDto<int>
    {
        public MemberCollocationDto() 
        {
            MemberColloEles = new List<MemberColloEle>();            
        }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "搭配名称")]
        [Required(ErrorMessage = "搭配名称不能为空")]
        [StringLength(12)]
        public virtual string CollocationName { get; set; }

        [Display(Name = "搭配风格")]
        public virtual string ProductAttrId { get; set; }

        [Display(Name = "场合")]
        public virtual int SituationId { get; set; }

        [Display(Name = "颜色")]
        public virtual int ColorId { get; set; }

        [Display(Name = "季节")]
        public virtual int SeasonId { get; set; }

        [Display(Name = "体型")]
        public string FigureIds { get; set; }

        [Display(Name = "是否推荐")]
        public  virtual bool IsCommond { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }
        
        [Display(Name = "实体标识")]
        public virtual Int32 Id { get; set; }

        public virtual ICollection<MemberColloEle> MemberColloEles { get; set; }
         
    }
}
