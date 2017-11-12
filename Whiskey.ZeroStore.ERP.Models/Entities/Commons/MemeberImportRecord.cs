



using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Entity;
using System.ComponentModel;

namespace Whiskey.ZeroStore.ERP.Models
{

    public class MemberImportRecord : EntityBase<int>
    {
        /// <summary>
        /// 导入总数
        /// </summary>
        [DisplayName("导入总数")]
        public int TotalCount { get; set; }

        /// <summary>
        /// 导入成功数量
        /// </summary>
        [DisplayName("导入成功数量")]
        public int SuccessCount { get; set; }

        /// <summary>
        /// 导入店铺
        /// </summary>
        [DisplayName("导入店铺")]
        public int? StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        /// <summary>
        /// 创建开始日期
        /// </summary>
        [DisplayName("创建开始日期")]
        public DateTime? CreateStartDate { get; set; }


        /// <summary>
        /// 创建结束日期
        /// </summary>
        [DisplayName("创建结束日期")]

        public DateTime? CreateEndtDate { get; set; }

        /// <summary>
        /// 导入成功的会员
        /// </summary>
        public virtual ICollection<Member> Members { get; set; }

        /// <summary>
        /// 操作人
        /// </summary>
        [DisplayName("操作人")]
        public virtual Administrator Operator { get; set; }

    }

    public class MemberImportRecordConfig : EntityConfigurationBase<MemberImportRecord, int>
    {
        public MemberImportRecordConfig()
        {
            ToTable("MemberImportRecord");
            Property(b => b.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

        }
    }


}


