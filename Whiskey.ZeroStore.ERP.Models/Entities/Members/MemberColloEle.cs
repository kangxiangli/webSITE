using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员搭配素材
    /// </summary>
    [Serializable]
    public class MemberColloEle : EntityBase<int>
    {
        public MemberColloEle()
        {                          
            Children = new List<MemberColloEle>();
        }

        [Display(Name=("会员搭配Id"))]
       
        public virtual int? MemberColloId { get; set; }
        
        [Display(Name=("父级Id"))]
        public virtual int? ParentId { get; set; }

        [Display(Name = ("层级"))]
        public virtual int? Level { get; set; }

        [Display(Name = ("元素类型"))]
        public virtual MemberColloEleFlag EleType { get; set; } //0表示图片元素，1表示文本元素

        [Display(Name=("图片路径"))]
        [StringLength(300)]
        public virtual string ImagePath { get; set; }

        [Display(Name = ("商品Id"))]
        public virtual int? ProductId { get; set; }

        [Display(Name = ("商品来源"))]
        public virtual ProductSourceFlag ProductSource { get; set; }

        [Display(Name = ("商品来源类型"))]
        public virtual SingleProductFlag ProductType { get; set; }

        [Display(Name=("元素信息"))]
        [StringLength(200)]
        public virtual string EleInfo { get; set; } //图片和文字的位置和尺寸信息

        [Display(Name = ("文本内容"))]
        [StringLength(200)]
        public virtual string TextInfo { get; set; } 

        [Display(Name = ("文本颜色"))]
        [StringLength(200)]
        public virtual string TextColor { get; set; } //文本颜色

        [Display(Name = ("文本字体"))]
        [StringLength(200)]
        public virtual string TextFont { get; set; } //文本字体

        [Display(Name=("旋转信息"))]
        public virtual string SpinInfo { get; set; }//旋转角度和方向信息

        [ForeignKey("MemberColloId")]
        public virtual MemberCollocation MemberCollo { get; set; }

        [ForeignKey("ParentId")]
        public virtual MemberColloEle Parent { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }         

        public virtual ICollection<MemberColloEle> Children { get; set; }
    }
}
