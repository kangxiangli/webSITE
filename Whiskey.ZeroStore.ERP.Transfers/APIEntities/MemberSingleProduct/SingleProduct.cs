using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct
{
    public class SingleProduct
    {
        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 封面图片
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// 副图
        /// </summary>
        public string ImagePath { get; set; }

        /// <summary>
        /// 一级分类
        /// </summary>
        public string ParentCategoryName { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public int ApproveCount { get; set; }

        /// <summary>
        /// 是否赞
        /// </summary>
        public int IsApprove { get; set; }

        /// <summary>
        /// 数量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 单品类型
        /// </summary>
        public SingleProductTypeEnum SingleProductType { get; set; }
    }


    public enum SingleProductTypeEnum
    {
        /// <summary>
        /// 普通单品
        /// </summary>
        Normal = 0,

        /// <summary>
        /// 推送单品
        /// </summary>
        Recommend = 1
    }
}
