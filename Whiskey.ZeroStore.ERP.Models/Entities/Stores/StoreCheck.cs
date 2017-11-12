using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 店铺检查项
    /// </summary>
    public class StoreCheckItem : EntityBase<int>
    {
        /// <summary>
        /// 项目名称
        /// </summary>
        [DisplayName("项目名称")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string CheckName { get; set; }

        [DisplayName("描述")]
        [StringLength(100, ErrorMessage = "{0}最多不可超过100个字")]
        public string Desc { get; set; }

        /// <summary>
        /// 考核项目数量
        /// </summary>
        [DisplayName("考核项目数量")]
        public int ItemsCount { get; set; }


        /// <summary>
        /// 得分标准，需满足（勾选）多少项才能得分
        /// </summary>
        [DisplayName("标准")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int Standard { get; set; }

        /// <summary>
        /// 勾选项 [{name:value},...]
        /// </summary>
        public string Items { get; set; }

        /// <summary>
        /// 处罚积分
        /// </summary>
        [DisplayName("罚分")]
        [Range(1, 99999, ErrorMessage = "处罚积分需在1~99999之间")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int PunishScore { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}
