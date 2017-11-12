
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{
    /// <summary>
    /// 操作超时扣积分记录
    /// </summary>
    [Serializable]
    public class PunishScoreRecord : EntityBase<int>
    {
        /// <summary>
        /// 违规类型
        /// </summary>
        [Display(Name = "违规类型")]
        [Required(ErrorMessage ="{0}不能为空")]
        public PunishTypeEnum? PunishType { get; set; }

        /// <summary>
        /// 处罚积分
        /// </summary>
        [Display(Name = "处罚积分")]
        public int PunishScore { get; set; }

        /// <summary>
        /// 受处罚adminId
        /// </summary>
        public int PunishAdminId { get; set; }

        [StringLength(200,ErrorMessage ="备注信息最长{0}个字符")]
        public string Remarks { get; set; }


        [ForeignKey("PunishAdminId")]
        public virtual Administrator PunishAdmin { get; set; }


        [ForeignKey("OperatorId")]

        public Administrator Operator{ get; set; }


    }

    /// <summary>
    /// 超时类型
    /// </summary>
    public enum PunishTypeEnum
    {
        配货单发货超时 = 0,
        配货单撤销超时 = 1,
        配货单确认收货超时 = 2,
        配货单拒绝收货超时 = 3,
        零售单退货超时 = 4
    }
}


