using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    /// <summary>
    /// 搭配图片尺寸
    /// </summary>
    public class TextList : BaseCollo
    {
        /// <summary>
        /// 文字内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 文字字号
        /// </summary>
        public string FontSize { get; set; }

        /// <summary>
        /// 颜色
        /// </summary>
        public string Color { get; set; }
        
    }
}
