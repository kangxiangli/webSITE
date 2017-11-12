using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OnlinePurchaseProductDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "推送标题")]
        [StringLength(25,ErrorMessage="最大长度不能超过{1}个")]
        [Required(ErrorMessage="请填写推送标题")]
        public virtual string NoticeTitle { get; set; }

        [Display(Name = "推送次数")]
        public virtual int NoticeQuantity { get; set; }

        [Display(Name = "编码")]
        [StringLength(20)]
        [Description("根据当前的时期yyyyMMddhhmmss+随机数4位生成唯一标识")]
        public virtual string UniqueCode { get; set; }

        [Display(Name = "开始日期")]
        [Required(ErrorMessage = "活动开始日期不能为空")]
        public virtual DateTime StartDate { get; set; }

        [Display(Name = "结束日期")]
        [Required(ErrorMessage = "活动结束日期不能为空")]        
        public virtual DateTime EndDate { get; set; }

        [Display(Name = "推送内容")]
        [StringLength(120,ErrorMessage="最大字符长度不能超过{1}")]
        [Required(ErrorMessage = "请填写推送内容")]
        public virtual string NoticeContent { get; set; }

        [Display(Name = "标识")]
        public virtual int Id { get; set; }
    }
}
