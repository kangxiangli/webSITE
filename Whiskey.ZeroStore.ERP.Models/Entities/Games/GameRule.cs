
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class GameRule : EntityBase<int>
	{
		[Display(Name = "游戏Id")]
        public virtual int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }

		[Display(Name = "序号")]
        public virtual int GIndex { get; set; }

        [Display(Name = "积分")]
        public virtual decimal Score { get; set; }

        [Display(Name = "概率")]
        [Range(0,100)]
        public virtual decimal Rate { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }
    }
}

