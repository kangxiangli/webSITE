using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class ColloText :TextList
    {
       

        /// <summary>
        ///操作类型 0添加；1修改；3删除；4没变化
        /// </summary>
        public int OperationType { get; set; }

    }
}
