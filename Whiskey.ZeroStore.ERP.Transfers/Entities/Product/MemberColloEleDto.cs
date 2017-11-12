using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    /// <summary>
    /// 搭配图片领域模型
    /// </summary>
    public class MemberColloEleDto: IAddDto, IEditDto<int>
    {
        public MemberColloEleDto()
        {
            MemberColloEleDtos = new List<MemberColloEleDto>();
        }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = ("会员搭配Id"))]
        [Required]
        public  int MemberColloId { get; set; }

        [Display(Name = ("父级Id"))]
        public  int? ParentId { get; set; }

        [Display(Name = ("图片层级"))]
        public  int? Level { get; set; }

        [Display(Name = ("元素类型"))]
        public MemberColloEleFlag EleType { get; set; } //0表示图片元素，1表示文本元素

        [Display(Name = ("图片路径"))]
        [StringLength(300)]
        public string ImagePath { get; set; }
        
        [Display(Name = ("商品Id"))]
        public int? ProductId { get; set; }

        [Display(Name=("商品来源"))]
        public ProductSourceFlag ProductSource { get; set; }
        
        [Display(Name = ("商品来源类型"))]
        public SingleProductFlag ProductType { get; set; }

        [Display(Name = ("元素信息"))]
        [StringLength(100)]
        public  string EleInfo { get; set; } //图片和文字的位置和尺寸信息

        [Display(Name = ("文本内容"))]
        [StringLength(50)]
        public  string TextInfo { get; set; }

        [Display(Name = ("文本颜色"))]
        [StringLength(50)]
        public  string TextColor { get; set; } //文本颜色

        [Display(Name = ("文本字体"))]
        [StringLength(100)]
        public  string TextFont { get; set; } //文本字体

        [Display(Name = ("旋转信息"))]
        public  string SpinInfo { get; set; }//旋转角度和方向信息

        public  ICollection<MemberColloEleDto> MemberColloEleDtos { get; set; }
    }
}
