using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class MemberFigureDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [Display(Name = "服装尺码")]
        [StringLength(15)]
        public virtual string ApparelSize { get; set; }

        [Display(Name = "身材类型")]
        [StringLength(2)]
        public virtual string FigureType { get; set; }

        [Display(Name = "性别")]
        public virtual int Gender { get; set; }

        [Display(Name = "生日")]
        public virtual DateTime? Birthday { get; set; }

        [Display(Name = "身高")]
        public virtual short Height { get; set; }

        [Display(Name = "体重")]
        public virtual short Weight { get; set; }

        [Display(Name = "肩宽")]
        public virtual short Shoulder { get; set; }

        [Display(Name = "胸围")]
        public virtual short Bust { get; set; }

        [Display(Name = "腰围")]
        public virtual short Waistline { get; set; }

        [Display(Name = "臀围")]
        public virtual short Hips { get; set; }
         
        [Display(Name = "喜欢颜色")]
        [StringLength(15)]
        public virtual string PreferenceColor { get; set; }

        [Display(Name = "身材描述")]
        [StringLength(50)]
        public virtual string FigureDes { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
