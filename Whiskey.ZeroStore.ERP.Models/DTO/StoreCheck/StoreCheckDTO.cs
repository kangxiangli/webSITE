using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 店铺检查项
    /// </summary>
    public class StoreCheckDTO : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }

        /// <summary>
        /// 项目名称
        /// </summary>
        [DisplayName("项目名称")]
        [Required(ErrorMessage = "{0}不能为空")]
        public string CheckName { get; set; }

        [DisplayName("项目描述")]
        [StringLength(100, ErrorMessage = "{0}最多不可超过100个字")]
        public string Desc { get; set; }

        /// <summary>
        /// 得分标准，需满足（勾选）多少项才能得分
        /// </summary>
        [DisplayName("标准")]
        [Required(ErrorMessage = "{0}不能为空")]
        public int Standard { get; set; }


        [DisplayName("罚分")]
        [Range(1, 99999, ErrorMessage = "处罚积分需在1~99999之间")]
        [Required(ErrorMessage = "{0}不能为空")]
        [RegularExpression(@"\d+", ErrorMessage ="{0}必须为数字")]
        /// <summary>
        /// 处罚积分
        /// </summary>
        public int PunishScore { get; set; }

        /// <summary>
        /// 勾选项 [{name}]
        /// </summary>
        public string Items { get; set; }


        /// <summary>
        /// 考核项目数量
        /// </summary>
        [DisplayName("考核项目数量")]
        //public int ItemsCount { get { return Items.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Length; } }
        public int ItemsCount { get; set; }

        public virtual int? OperatorId { get; set; }

    }
}
