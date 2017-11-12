
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class GameDto : IAddDto, IEditDto<int>
    {
        public GameDto()
        {
            GameRuleDtos = new List<GameRule>();
        }

        public int Id { get; set; }

        [Display(Name = "名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Name { get; set; }

        [Display(Name = "标识")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Required]
        public virtual string Tag { get; set; }

        [Display(Name = "开始时间")]
        public virtual DateTime StartTime { get; set; }

        /// <summary>
        /// 结束时间，Null时表示无限期
        /// </summary>
        [Display(Name = "结束时间")]
        public virtual DateTime? EndTime { get; set; }

        /// <summary>
        /// 限玩总次数,0代表不限次
        /// </summary>
        [Display(Name = "限玩次数")]
        public virtual int LimitCount { get; set; }

        [Display(Name = "限日次数")]
        public virtual int LimitDayCount { get; set; }

        /// <summary>
        /// 分享次数,0代表不限次
        /// </summary>
        [Display(Name = "分享次数")]
        public virtual int LimitShareCount { get; set; }

        [Display(Name = "日分享数")]
        public virtual int LimitShareDayCount { get; set; }

        [Display(Name = "游戏简介")]
        public virtual string Introduce { get; set; }

        [Display(Name = "备注")]
        [StringLength(200, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Notes { get; set; }

        public virtual List<GameRule> GameRuleDtos { get; set; }
    }
}


