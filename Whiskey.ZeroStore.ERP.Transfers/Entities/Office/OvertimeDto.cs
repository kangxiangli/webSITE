﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class OvertimeDto : IAddDto, IEditDto<int>
    {
        [DisplayName("加班人员")]
        public virtual int AdminId { get; set; }

        [DisplayName("加班原因")]
        [StringLength(100, ErrorMessage = "加班原因最大字符长度{0}以内")]
        [Required(ErrorMessage = "加班原因不能为空")]
        public virtual string OvertimeReason { get; set; }

        [DisplayName("加班开始时间")]
        public virtual DateTime StartTime { get; set; }

        [DisplayName("加班结束时间")]
        public virtual DateTime EndTime { get; set; }

        [DisplayName("加班时长")]
        public virtual double OvertimeDays { get; set; }
        [DisplayName("休息时长")]
        public virtual double RestHours { get; set; }
        [DisplayName("应得积分")]
        [DefaultValue(0)]
        public virtual decimal GetPoints { get; set; }
        [DisplayName("实际加班天数")]
        public virtual double RealWorkDays { get; set; }

        [DisplayName("审核状态")]
        public virtual int VerifyType { get; set; }

        [DisplayName("审核人")]
        public virtual int? VerifyAdminId { get; set; }

        [DisplayName("备注")]
        [StringLength(120, ErrorMessage = "备注最大字符长度{0}以内")]
        public virtual string Notes { get; set; }

        [DisplayName("标识Id")]
        public virtual int Id { get; set; }
    }
}