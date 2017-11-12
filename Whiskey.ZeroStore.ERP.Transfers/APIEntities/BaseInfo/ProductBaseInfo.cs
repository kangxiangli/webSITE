using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.BaseInfo
{
    /// <summary>
    /// 商品基类
    /// </summary>
    public class ProductBaseInfo
    {
        /// <summary>
        /// Id标识
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// 价格
        /// </summary>
        public float Price { get; set; }

        /// <summary>
        /// 分类名称
        /// </summary>
        public string CategoryName { get; set; }

        /// <summary>
        /// 季节名称
        /// </summary>
        public string SeasonName { get; set; }


    }
}
