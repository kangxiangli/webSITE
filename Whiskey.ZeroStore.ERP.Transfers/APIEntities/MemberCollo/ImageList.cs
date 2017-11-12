using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    /// <summary>
    /// 搭配图片
    /// </summary>
    public class ImageList : BaseCollo
    {
        /// <summary>
        /// 图片数据
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// 商品Id
        /// </summary>
        public int? ProductId { get; set; }        

        /// <summary>
        /// 商品来源类型 //单品 商城 粉丝 素材
        /// </summary>
        public ProductSourceFlag ProductSource { get; set; }

        /// <summary>
        /// 商品类型 //用户上传  用户购买的
        /// </summary>
        public SingleProductFlag ProductType { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int? Level { get; set; }

        /// <summary>
        /// 图片路径
        /// </summary>
        public string ImagePath { get; set; }
    }
}
