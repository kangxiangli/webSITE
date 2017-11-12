using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Notices.Models
{
    public class StoreSpendStatisticsDto
    {
          [Display(Name = "统计店铺id")]
        public int StoreId { get; set; }
        /// <summary>
        /// 开支类型，具体见SpendType枚举
        /// </summary>
        public int SpendType { get; set; }
        /// <summary>
        /// 金额支出
        /// </summary>
        public float Amount { get; set; }
        /// <summary>
        /// 单据凭证图片
        /// </summary>
        public string OrderImg { get; set; }

        /// <summary>
        /// 起始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
       ///结束时间
        public DateTime EndTime { get; set; }
        
        public string Notes { get; set; }
        public string Title { get; set; }
        /// <summary>
        /// 关键字，便于后期检索
        /// </summary>
        public string KeyWord { get; set; }

        public int? OperatorId { get; set; }

    }
}

    
