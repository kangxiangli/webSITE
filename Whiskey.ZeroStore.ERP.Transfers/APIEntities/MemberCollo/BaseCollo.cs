using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class BaseCollo
    {

        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 位置
        /// </summary>
        public string Frame { get; set; }

        /// <summary>
        /// 旋转角度
        /// </summary>
        public string Spin { get; set; }
    }
}
