using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class GameRecord : EntityBase<int>
    {
        [Display(Name ="会员Id")]
        public virtual int MemberId { get; set; }

        [Display(Name ="获得积分")]
        public virtual decimal Score { get; set; }

        [Display(Name ="游戏Id")]
        public virtual int GameId { get; set; }

        [ForeignKey("GameId")]
        public virtual Game Game { get; set; }

        [ForeignKey("MemberId")]
        public virtual Member Member { get; set; }
    }
}
