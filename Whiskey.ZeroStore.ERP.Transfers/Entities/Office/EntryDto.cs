using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Transfers
{
    public class EntryDto : IAddDto, IEditDto<int>
    {
        public int Id { get; set; }
        [DisplayName("员工Id")]
        public virtual int MemberId { get; set; }

        [DisplayName("Mac地址")]
        [StringLength(50, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string MacAddress { get; set; }

        [DisplayName("银行卡")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string BankcardImgPath { get; set; }
        [DisplayName("简历")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string ResumeImgPath { get; set; }
        [DisplayName("身份证")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string IdCardImgPath { get; set; }

        [DisplayName("健康证")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string HealthCertificateImgPath { get; set; }
        public virtual int operationId { get; set; }
        [DisplayName("照片")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string PhotoImgPath { get; set; }

        [DisplayName("劳动合同")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string LaborContractImgPath { get; set; }

        [Display(Name = "入职时间")]
        [Required(ErrorMessage = "请选择入职时间")]
        public virtual DateTime EntryTime { get; set; }
        [DisplayName("部门Id")]
        public virtual int? DepartmentId { get; set; }

        [DisplayName("角色权限")]
        [StringLength(200, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string RoleJurisdiction { get; set; }

        [DisplayName("职位Id")]
        public virtual int? JobPositionId { get; set; }

        [Display(Name = "审核结果")]
        public virtual int ToExamineResult { get; set; }

        [DisplayName("面试评价")]
        [StringLength(1000, ErrorMessage = "不能超过最大长度{1}")]
        public virtual string InterviewEvaluation { get; set; }

    }
}
