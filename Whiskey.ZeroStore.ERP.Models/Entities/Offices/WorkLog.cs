using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 工作日志
    /// </summary>
    [Description("记录工作内容")]
    public class WorkLog : EntityBase<int>
    {
        [DisplayName("日志名称")]
        [StringLength(50,ErrorMessage="最大长度不能超过{1}")]
        [Required(ErrorMessage="请填写")]
        public virtual string WorkLogName { get; set; }

        [DisplayName("日志类型")]
        public virtual int WorkLogAttributeId { get; set; }

        [DisplayName("员工")]        
        public virtual int? StaffId { get; set; }

        [DisplayName("附件")]
        [StringLength(100)]
        [Description("保存文件的上传地址，如果是多个文件需要，压缩成ZIP或者RAR上传")]
        public virtual string FilePath { get; set; }

        [DisplayName("备注")]
        public virtual string Notes { get; set; }

        [DisplayName("关键词")]
        public virtual string Keys { get; set; }

        [ForeignKey("WorkLogAttributeId")]
        public virtual WorkLogAttribute WorkLogAttribute { get; set; }

        [ForeignKey("StaffId")]
        public virtual Administrator Staff { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
