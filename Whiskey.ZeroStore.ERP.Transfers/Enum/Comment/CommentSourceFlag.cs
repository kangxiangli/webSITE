using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.Enum.Comment
{
    public enum CommentSourceFlag
    {
        /// <summary>
        /// 会员单品
        /// </summary>
        MemberSingleProduct,

        /// <summary>
        /// 会员搭配
        /// </summary>
        MemberCollocation,

        /// <summary>
        /// 商城商品
        /// </summary>
        StoreProduct,

        /// <summary>
        /// 文章
        /// </summary>
        Article,

        /// <summary>
        /// App文章
        /// </summary>
        AppArticle,

        /// <summary>
        /// 话题
        /// </summary>
        Topic,
        ///// <summary>
        ///// 用户上传商品
        ///// </summary>
        //UploadProduct
    }
}
