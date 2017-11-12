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
    public class ScoreRule : EntityBase<int>
    {
        //yxk 
        //积分规则
        /// <summary>
        /// 积分类型： 消费积分
        /// </summary>
        public int ScoreType { get; set; }
        /// <summary>
        /// 100001-999999 六位数编号
        /// </summary>
        [Display(Name = "积分规则编号")]
        public int ScoreNumber { get; set; }
        [Display(Name = "积分规则名称")]
        [MaxLength(20)]
        public string ScoreName{ get; set; }
        [Display(Name = "积分规则描述")]
        [MaxLength(200)]
        public string Descrip { get; set; }

        [Display(Name="最低消费")]
        public float MinConsum { get; set; }
        /// <summary>
        /// 单位金额
        ///消费金额：获取积分= ConsumeUnit:ScoreUnit
        /// </summary>
        public float ConsumeUnit { get; set; }
        /// <summary>
        /// 单位积分
        /// </summary>
        public float ScoreUnit { get; set; }
        /// <summary>
        /// 消费积分是否获取积分
        /// </summary>
        [Display(Name = "消费积分是否获取积分")]
        public bool IsConsumeScoreGetScore { get; set; }
        /// <summary>
        /// 消费储值是否获取积分
        /// </summary>
          [Display(Name = "消费储值是否获取积分")]
        public bool IsConsumeCardMoneyGetScore { get; set; }


        /// <summary>
        /// 是否可以在非归属店铺下使用积分
        /// </summary>
        [Display(Name = "是否可以在非归属店铺下使用积分")]
        public bool CanUseScoreWhenNotBelongToStore { get; set; }

        /// <summary>
        /// 在非归属店铺下消费是否能获得积分
        /// </summary>
        [Display(Name = "在非归属店铺下消费是否能获得积分")]
        public bool CanGetScoreWhenNotBelongToStore { get; set; }

        [ForeignKey("OperatorId")]
        public Administrator Operator { get; set; }
    }
}
