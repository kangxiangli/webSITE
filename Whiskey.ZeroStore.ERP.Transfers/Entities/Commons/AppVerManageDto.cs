
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class AppVerManageDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        [Display(Name = "APP类别")]
        public virtual AppTypeFlag AppType { get; set; }

        [Display(Name = "版本V1")]
        [Required]
        public virtual int V1 { get; set; }

        [Display(Name = "版本V2")]
        [Required]
        public virtual int V2 { get; set; }

        [Display(Name = "版本V3")]
        [Required]
        public virtual int V3 { get; set; }

        [Display(Name = "当前版本")]
        public virtual string Version { get; set; }

        [Display(Name = "访问地址")]
        [StringLength(300, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string AccessPath { get; set; }

        [Display(Name = "下载地址")]
        [StringLength(300, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string DownloadPath { get; set; }

        [Display(Name = "保存路径")]
        [StringLength(300, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string SavePath { get; set; }

        [Display(Name = "文件标识")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string FileTag { get; set; }
    }
}


