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
    /// <summary>
    /// 敏感词实体
    /// </summary>
    public class SensitiveWord :EntityBase<int>
    {
        public SensitiveWord()
        {
            WordPattern = string.Empty;
            IsForbid = true;
            IsNeutral = true;
            ReplaceWord = string.Empty;
        }

        [Display(Name="敏感词")]
        [StringLength(50)]
        public virtual string WordPattern { get; set; }

        [Display(Name = "是否准许")]
        public virtual bool IsForbid { get; set; }

        [Display(Name = "是否中立")]
        public virtual bool IsNeutral { get; set; }

        [Display(Name = "替换词")]
        [StringLength(50)]
        public virtual string ReplaceWord { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
