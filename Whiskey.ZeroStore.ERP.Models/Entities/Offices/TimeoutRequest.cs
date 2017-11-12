using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{

    public class TimeoutRequest : EntityBase<int>
    {
        [DisplayName("超时类型")]
        public int TimeoutSettingId { get; set; }
        public int DepartmentId { get; set; }


        /// <summary>
        /// 编号,订单号,或配货单号
        /// </summary>
        [StringLength(maximumLength: 20, ErrorMessage = "{0}不能超过{1}")]
        [DisplayName("订单号")]
        [Required]
        [Index(IsClustered =false,IsUnique =false)]
        public string Number { get; set; }

        /// <summary>
        /// 超时时间,单位(秒)
        /// </summary>
        public int Timeouts { get; set; }


        [DisplayName("审核状态")]
        public TimeoutRequestState State { get; set; }


        /// <summary>
        /// 是否已使用
        /// </summary>
        public bool IsUsed { get; set; }


        /// <summary>
        /// 备注
        /// </summary>
        [DisplayName("备注")]
        [Required]
        [StringLength(maximumLength: 200)]
        public string Notes { get; set; }

        /// <summary>
        /// 申请人
        /// </summary>
        public int RequestAdminId { get; set; }

        [ForeignKey("RequestAdminId")]
        public virtual Administrator RequestAdmin { get; set; }


        /// <summary>
        /// 审核人
        /// </summary>
        public int? VerifyAdminId { get; set; }

        [ForeignKey("VerifyAdminId")]
        public virtual Administrator VerifyAdmin { get; set; }



        /// <summary>
        /// 超时类型
        /// </summary>
        
        [ForeignKey("TimeoutSettingId")]
        public virtual TimeoutSetting TimeoutType { get; set; }

        [ForeignKey("DepartmentId")]
        public virtual Department Department { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }

    public enum TimeoutRequestState
    {
        审核中 = 0,
        已通过 = 1,
        未通过 = 2
    }


    public class TimeoutRequestConfig : EntityConfigurationBase<TimeoutRequest, int>
    {
        public TimeoutRequestConfig()
        {
            ToTable("T_TimeoutRequest");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


}
