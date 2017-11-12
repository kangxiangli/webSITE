using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Authorities
{
    public class DepartInfo
    {
        /// <summary>
        /// 部门Id
        /// </summary>
        public int Id {get;set;}
        /// <summary>
        /// 部门名称
        /// </summary>
        public string DepartName { get; set; }

        /// <summary>
        /// 部门员工
        /// </summary>
        public List<AdminInfo> Admins { get; set; }
    }
}
