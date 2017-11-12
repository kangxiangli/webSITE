
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class Game : EntityBase<int>
	{
        public Game()
        {
            GameRecords = new List<GameRecord>();
            MemberShares = new List<MemberShare>();
            GameRules = new List<GameRule>();
        }

		[Display(Name = "名称")]
        [StringLength(20, ErrorMessage = "最大长度不能超过{1}个字符")]
        public virtual string Name { get; set; }

		[Display(Name = "标识")]
        [StringLength(50, ErrorMessage = "最大长度不能超过{1}个字符")]
        [Required]
        [Index]
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
        
        /// <summary>
        /// 每日限玩次数
        /// </summary>
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

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        public virtual ICollection<GameRecord> GameRecords { get; set; }

        public virtual ICollection<MemberShare> MemberShares { get; set; }

        public virtual ICollection<GameRule> GameRules { get; set; }
    }
}

