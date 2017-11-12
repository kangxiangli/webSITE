using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{
    public class StoreRecommend : EntityBase<int>
    {
        public int StoreId { get; set; }

        [ForeignKey("StoreId")]
        public virtual Store Store { get; set; }

        [ForeignKey("OperatorId")]
        public virtual Administrator Operator { get; set; }


        /// <summary>
        /// 商城下推荐的大品类名称
        /// </summary>
        public string BigProdNum { get; set; }


    }
}
