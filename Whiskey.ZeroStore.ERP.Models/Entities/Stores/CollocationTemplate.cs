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

    public class CollocationTemplate : EntityBase<int>
    {
        /// <summary>
        /// 模版名称
        /// </summary>
        [StringLength(20)]
        public string Name { get; set; }

        /// <summary>
        /// 搭配规则
        /// </summary>
        public string CollocationRules { get; set; }


        /// <summary>
        /// 规则数量
        /// </summary>
        public int RuleCount { get; set; }


        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

    }

    public class CollocationRulesEntry
    {
        public CollocationRulesEntry()
        {
            Tags = new List<string>();
        }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public List<string> Tags { get; set; }


        public string ProductCollocationImg { get; set; }
        public string ProductNumber { get; set; }


    }

    public class CollocationTemplateConfig : EntityConfigurationBase<CollocationTemplate, int>
    {
        public CollocationTemplateConfig()
        {
            ToTable("CollocationTemplate");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


}
