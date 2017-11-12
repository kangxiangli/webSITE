using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using System;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 店铺检查记录
    /// </summary>
    public class StoreCheckRecordDTO:IAddDto,IEditDto<int>
    {
        public StoreCheckRecordDTO()
        {
            CheckTime = DateTime.Now;
        }
        public int Id { get; set; }
        /// <summary>
        /// 检查店铺
        /// </summary>
        [DisplayName("考核店铺")]
        [Required(ErrorMessage ="{0}不能为空")]
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
        public DateTime CheckTime{ get; set; }


        /// <summary>
        /// 上传图片
        /// </summary>
        [DisplayName("上传图片")]

        public string Images { get; set; }


        /// <summary>
        /// 总罚分
        /// </summary>
        [DisplayName("总罚分")]
        public int TotalPunishScore { get; set; }

        /// <summary>
        /// 检查详情
        /// </summary>
        [DisplayName("检查详情")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string CheckDetails { get; set; }


        /// <summary>
        /// 总评价
        /// </summary>
        [DisplayName("总评价")]
        [Range(0, 5, ErrorMessage = "{0}需在0~5之间")]
        public decimal RatingPoints { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        [StringLength(50,ErrorMessage ="{0}不可超过50个字")]
        public string Remark { get; set; }

        /// <summary>
        /// 考核人
        /// </summary>
        public virtual int? OperatorId { get; set; }
       
    }
}
