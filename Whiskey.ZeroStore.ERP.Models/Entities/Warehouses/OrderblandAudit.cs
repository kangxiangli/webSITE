using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models
{ /// <summary>
    /// 配货单审核，主要是针对拒绝配货的情况
    /// </summary>
    public class OrderblankAudit : EntityBase<int>
    {
        /// <summary>
        /// 配货单id
        /// </summary>
        public int OrderblankId { get; set; }
       /// <summary>
       /// 0:待审核 1：通过 2：不通过
       /// </summary>
        [Display(Name = "审核是否通过")]
        public int AuditPas { get; set; }

        [Display(Name = "不配货原因")]
        public string NoOrderMessage { get; set; }
       
        [Display(Name = "审核备注")]
        public string Notes { get; set; }
        [ForeignKey("OrderblankId")]
        public Orderblank Orderblank { get; set; }
    }
}
