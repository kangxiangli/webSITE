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
    [Serializable]
    public class QrLogin : EntityBase<int>
    {
        [Display(Name = "二维码标识")]
        [Required(ErrorMessage = "请输入二维码标识")]
        public virtual string QrCode { get; set; }

        [Display(Name = "二维码图片保存路径")]
        public virtual string QrImgPath { get; set; }

        [Display(Name = "扫码人")]
        public virtual int? AdminId { get; set; }

        [Display(Name = "扫码会员")]
        public virtual int? MemberId { get; set; }

        [ForeignKey("AdminId")]
        public virtual Administrator Administrator { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
