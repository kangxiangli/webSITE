using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using System;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 店铺检查记录
    /// </summary>
    public class StoreCheckRecord : EntityBase<int>
    {
        /// <summary>
        /// 考核店铺
        /// </summary>
        [DisplayName("考核店铺")]
        [Required(ErrorMessage = "{0}不能为空")]

        public string StoreName { get; set; }

        /// <summary>
        /// 考核店铺id
        /// </summary>
        [Required(ErrorMessage ="{0}不能为空")]
        public int StoreId { get; set; }


        /// <summary>
        /// 总评价
        /// </summary>
        [DisplayName("总评价")]
        [Range(0,5,ErrorMessage ="{0}需在0~5之间")]
        public decimal RatingPoints { get; set; }


        /// <summary>
        /// 总罚分
        /// </summary>
        [DisplayName("总罚分")]
        public int TotalPunishScore { get; set; }

        /// <summary>
        /// 检查详情
        /// </summary>
        [DisplayName("检查详情")]
        [Required(ErrorMessage ="{0}不能为空")]
        public string CheckDetails { get; set; }


        /// <summary>
        /// 考核人
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 上传图片
        /// </summary>
        [DisplayName("上传图片")]

        public string Images { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        [StringLength(50, ErrorMessage = "{0}不可超过50个字")]
        public string Remark { get; set; }

        /// <summary>
        /// 考核时间
        /// </summary>
        [DisplayName("考核时间")]
        public DateTime CheckTime { get; set; }
    }
}
