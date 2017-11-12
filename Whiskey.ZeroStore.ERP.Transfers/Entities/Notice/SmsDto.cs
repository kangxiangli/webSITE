using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class SmsDto : IAddDto, IEditDto<int>
    {
        public SmsDto()
        {
            StoreIds = new List<int>();
        }

        [Display(Name = "实体标识")]
        public Int32 Id { get; set; }

        [Display(Name = "通知标题")]
        [Required(ErrorMessage = "请填写标题")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Title { get; set; }

        [Display(Name = "短信内容")]
        [Required(ErrorMessage = "请填写内容")]
        [StringLength(250, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Description { get; set; }

        [Display(Name = "已发送")]
        public virtual bool IsSend { get; set; }

        [Display(Name = "发送时间")]
        public virtual DateTime? SendTime { get; set; }

        [Display(Name = "店铺Ids")]
        [Required]
        public ICollection<int> StoreIds { get; set; }

    }
}
