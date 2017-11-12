using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class SensitiveWordDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "敏感词")]
        [StringLength(50)]
        public  string WordPattern { get; set; }

        [Display(Name = "是否准许")]
        public  bool IsForbid { get; set; }

        [Display(Name = "是否中立")]
        public  bool IsNeutral { get; set; }

        [Display(Name = "替换词")]
        [StringLength(50)]
        public  string ReplaceWord { get; set; }

        [Display(Name = "唯一标识")]
        public Int32 Id { get; set; }
    }
}
