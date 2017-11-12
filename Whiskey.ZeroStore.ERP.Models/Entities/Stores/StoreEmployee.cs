using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Models.Entities
{
    public class StoreEmployee : EntityBase<int>
    {
        /// <summary>
        /// 所属店铺id
        /// </summary>
        public int StoreId { get; set; }
        /// <summary>
        /// 员工id
        /// </summary>
        public int EmployeeId { get; set; }
        /// <summary>
        /// 员工类型
        /// </summary>
        public int EmployeeType { get; set; }

        [ForeignKey("StoreId")]
        public Store Store { get; set; }
        [ForeignKey("EmployeeId")]
        public Administrator Employee { get; set; }

    }
}
