using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.DTO
{
    /// <summary>
    /// 检查项目下每一项的勾选详情
    /// </summary>
    public class CheckDetail
    {
        /// <summary>
        /// 选项名称
        /// </summary>
        public string OptionName { get; set; }

        /// <summary>
        /// 是否勾选
        /// </summary>
        public bool IsCheck { get; set; }
    }
}
