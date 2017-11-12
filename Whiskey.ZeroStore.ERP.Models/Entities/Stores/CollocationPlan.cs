using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;

namespace Whiskey.ZeroStore.ERP.Models
{

    /// <summary>
    /// 搭配方案
    /// </summary>
    public class CollocationPlan : EntityBase<int>
    {
        /// <summary>
        /// 方案名
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 方案描述
        /// </summary>
        [StringLength(200)]
        public string Desc { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [StringLength(200)]
        public string CoverImg { get; set; }


        /// <summary>
        /// 规则数量
        /// </summary>
        public int RuleCount { get; set; }


        /// <summary>
        /// 搭配建议数量
        /// </summary>
        public int SuggestionCount { get; set; }


        /// <summary>
        /// 使用次数
        /// </summary>
        public int UseCount { get; set; }

        /// <summary>
        /// 搭配属性
        /// </summary>
        public string Tags { get; set; }

        public string Rules { get; set; }

        public string Suggestions { get; set; }


        /// <summary>
        /// 关联的预约
        /// </summary>
        public virtual ICollection<Appointment> Appointments { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }


    public class CollocationPlanDto {
        public int Id { get; set; }
        /// <summary>
        /// 方案名
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 方案描述
        /// </summary>
        [StringLength(200)]
        public string Desc { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        [StringLength(200)]
        public string CoverImg { get; set; }


        /// <summary>
        /// 搭配属性
        /// </summary>
        public string[] Tags { get; set; }

        public CollocationRulesEntry[] Rules { get; set; }

        public SuggestionEntry[] Suggestions { get; set; }



    }

    public class SuggestionEntry
    {
        public int MemberCollocationId { get; set; }
        public string MemberCollocationImagePath { get; set; }
        public int[] RuleOrder { get; set; }
    }


    public class CollocationPlanConfig : EntityConfigurationBase<CollocationPlan, int>
    {
        public CollocationPlanConfig()
        {
            ToTable("C_CollocationPlan");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);


        }
    }


}
