using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Entities.Article
{
    public enum ArticleStatus
    {
        /// <summary>
        /// 审核中
        /// </summary>
        Audit=0,
        /// <summary>
        /// 审核不通过
        /// </summary>
        NotPass=1,
        /// <summary>
        /// 审核通过
        /// </summary>
        AuditThrough=2,
    }
}
