using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Notices
{
    /// <summary>
    /// 通知消息问答题问题类型
    /// </summary>
    public enum QuestionTypeFlag
    {
        /// <summary>
        /// 选择题
        /// </summary>
        [Description("选择题")]
        Choice,

        /// <summary>
        /// 填空题
        /// </summary>
        [Description("填空题")]
        FillIn,

        /// <summary>
        /// 判断题
        /// </summary>
        [Description("判断题")]
        Judgment
    }
}
