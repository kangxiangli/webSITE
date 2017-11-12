using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 会员人脸信息
    /// </summary>
    public class MemberFace : EntityBase<int>
    {
        /// <summary>
        /// 人脸标识
        /// </summary>
        [Display(Name = "标识")]
        [StringLength(100)]
        public string FaceToken { get; set; }
        /// <summary>
        /// 图像路径
        /// </summary>
        [Display(Name = "图像")]
        [StringLength(300)]
        public string ImgPath { get; set; }

        [Display(Name = "会员")]
        public virtual int MemberId { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [Display(Name = "店铺")]
        public virtual int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

    }
}
