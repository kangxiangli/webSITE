using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Models.Enums
{
    public enum ProductSourceFlag
    {
        /// <summary>
        /// 会员单品
        /// </summary>
        MemberProduct = 0,

        /// <summary>
        /// 商城单品(被推荐的商品)
        /// </summary>
        StoreProduct = 1,
        
        /// <summary>
        /// 粉丝单品
        /// </summary>
        FansProduct = 2,

        /// <summary>
        /// 上传单品
        /// </summary>
        UploadProduct = 3,

        /// <summary>
        /// 素材单品
        /// </summary>
        MaterialProduct = 4,
    }
}
