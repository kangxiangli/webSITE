using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Authorities
{
    public class AdminInfo
    {
        /// <summary>
        /// 员工Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 会员名称
        /// </summary>
        public string AdminName { get; set; }

        /// <summary>
        /// 职位
        /// </summary>
        public string JobPositonName { get; set; }

        /// <summary>
        /// 用户照片
        /// </summary>
        public string UserPhoto { get; set; }

        /// <summary>
        /// 手机号码
        /// </summary>
        public string PhoneNum { get; set; }

        /// <summary>
        /// 部门Id
        /// </summary>
        public int DepartId { get; set; }
    }
}
