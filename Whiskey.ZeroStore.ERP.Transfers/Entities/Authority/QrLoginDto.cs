using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class QrLoginDto : IAddDto, IEditDto<int>
    {
		[Display(Name = "实体标识")]
        public int Id { get; set; }

        [Display(Name = "二维码标识")]
        [Required(ErrorMessage = "请输入二维码标识")]
        public virtual string QrCode { get; set; }

        [Display(Name = "二维码图片保存路径")]
        public virtual string QrImgPath { get; set; }

        [Display(Name = "扫码人")]
        public virtual int? AdminId { get; set; }

        [Display(Name = "扫码会员")]
        public virtual int? MemberId { get; set; }
    }
}
