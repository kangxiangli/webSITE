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
    public class StoreNoRecommend : EntityBase<int>
    {
        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }

        /// <summary>
        /// 商城下推荐的大品类名称
        /// </summary>
        [Index(IsClustered = false, IsUnique = false)]
        [MaxLength(10)]
        public string BigProdNum { get; set; }


    }
}
