using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers 
{
    public class HtmlItemDto : IAddDto, IEditDto<int>
    {
        [Display(Name = "名称")]
        [Required, StringLength(50, ErrorMessage = "名称在50个字符以内")]
        public virtual string ItemName { get; set; }

        [Display(Name = "文件类型")]
        public virtual int HtmlItemType { get; set; }

        [Display(Name = "路径")]
        [Required, StringLength(150)]
        public virtual string SavePath { get; set; }

        [Display(Name = "简介")]
        [StringLength(120, ErrorMessage = ("简介在150个字符以内"))]
        public virtual string Notes { get; set; }

        [Display(Name = "标识Id")]
        public virtual Int32 Id { get; set; }
    }
}
