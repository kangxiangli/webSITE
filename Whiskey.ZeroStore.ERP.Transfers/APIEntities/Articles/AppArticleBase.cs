using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Articles
{
    /// <summary>
    /// App文章基础类
    /// </summary>
    public class AppArticleBase
    {

        public int OperationType { get; set; }

        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 位置
        /// </summary>
        public string Position { get; set; }

        /// <summary>
        /// 比例
        /// </summary>
        public string Ratio { get; set; }

        /// <summary>
        /// 旋转
        /// </summary>
        public string Rotation { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }
    }
}
