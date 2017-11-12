using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class Sms : EntityBase<int>
    {
        public Sms()
        {
            Members = new List<Member>();
        }

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

        public virtual ICollection<Member> Members { get; set; }

        public virtual ICollection<Store> Stores { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
