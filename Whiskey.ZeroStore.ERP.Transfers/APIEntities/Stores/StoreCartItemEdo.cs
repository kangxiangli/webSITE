using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.Stores
{
    
    /// <summary>
    /// 选购车输出数据对象
    /// </summary>
    public class StoreCartItemEdo
    {
        /// <summary>
        /// 父级(用大货号代替)
        /// </summary>
        public string ParentId {get;set;}

        /// <summary>
        /// 标识
        /// </summary>
        public string Id {get;set;}
               
        /// <summary>
        /// 货号
        /// </summary>
         
        public string ProductNumber {get;set;}

        /// <summary>
        /// 缩略图
        /// </summary>
        public string ThumbnailPath {get;set;}

        /// <summary>
        /// 颜色图片路径
        /// </summary>
        public string ColorIconPath {get;set;}

        /// <summary>
        /// 尺码名称
        /// </summary>
        public string SizeName {get;set;}

        /// <summary>
        /// 数量
        /// </summary>
        public int Quantity { get; set; }
    }
}
