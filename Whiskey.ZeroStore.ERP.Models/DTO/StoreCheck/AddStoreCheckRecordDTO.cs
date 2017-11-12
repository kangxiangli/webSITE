using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    public class AddStoreCheckRecordDTO
    {
        public AddStoreCheckRecordDTO()
        {
            
        }

        public int? AdminId { get; set; }

        /// <summary>
        /// 检查店铺
        /// </summary>
        [DisplayName("考核店铺")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string StoreName { get; set; }

        /// <summary>
        /// 检查店铺
        /// </summary>
        [Required(ErrorMessage = "{0}不能为空")]
        public int StoreId { get; set; }

        /// <summary>
        /// 考核日期
        /// </summary>
        [DisplayName("考核日期")]
        public DateTime CheckTime { get; set; }


        /// <summary>
        /// 上传图片
        /// </summary>
        [DisplayName("上传图片")]

        public string Images { get; set; }


        public string CheckDetails { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        [StringLength(50, ErrorMessage = "{0}不可超过50个字")]
        public string Remark { get; set; }


    }
}
