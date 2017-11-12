using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员搭配实体
    /// </summary>
    [Serializable]
    public class MemberCollocation:EntityBase<int>
    {
        public MemberCollocation()
        {
             
        }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name="搭配名称")]
        [Required(ErrorMessage="搭配名称不能为空")]
        [StringLength(15)]
        public virtual string CollocationName {get;set;}

        [Display(Name = "搭配风格")]        
        public virtual int ProductAttrId { get; set; }

        [Display(Name = "场合")]        
        public virtual int SituationId { get; set; }

        [Display(Name = "颜色")]        
        public virtual int ColorId { get; set; }

        [Display(Name = "季节")]
        public virtual int SeasonId { get; set; }

        [Display(Name = "是否推荐")]
        public virtual bool IsRecommend { get; set; }

        [Display(Name = "备注")]
        [StringLength(120)]
        public virtual string Notes { get; set; }        

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
        public virtual ICollection<MemberColloEle> MemberColloEles { get; set; }
    }
}
