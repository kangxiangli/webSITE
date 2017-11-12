using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    //采购单审核记录 yxk 2015-11
    public class PurchaseAudit:EntityBase<int>
    {
        /// <summary>
        /// 采购单id
        /// </summary>
        public int PurchaseId { get; set; }
        /// <summary>
        /// /// <summary>
        /// 0:待审核 1:审核通过，2：审核不通过 3:废除 4：撤消通过审核 5：撤消不通过审核
        /// </summary>
        /// </summary>
        public int Status{get;set;}
        /// <summary>
        /// 采购单审核备注
        /// </summary>
        public string Note { get; set; }

        [ForeignKey("PurchaseId")]
        public Purchase Purchase { get; set; }
    }
}
