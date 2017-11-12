using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberCollo
{
    public class ColloArticle
    {
        public ColloArticle()
        {
            ListMemberApproval = new List<MemberApproval>();
            ListMemberComment = new List<MemberComment>();
        }

        /// <summary>
        /// 数据类型 0表示搭配 1表示文章
        /// </summary>
        public int DataType { get; set; }

        /// <summary>
        /// 标识Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 会员名称
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 会员头像
        /// </summary>
        public string MemberImagePath { get; set; }

        /// <summary>
        /// 点赞数量
        /// </summary>
        public int ApprovalCount { get; set; }

        /// <summary>
        /// 评论数量
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 是否点赞
        /// </summary>
        public int IsApproval { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sequence { get; set; }

        /// <summary>
        /// 封面图
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// 点赞会员信息
        /// </summary>
        public List<MemberApproval> ListMemberApproval { get; set; }

        /// <summary>
        /// 评论会员信息
        /// </summary>
        public List<MemberComment> ListMemberComment { get; set; }

        /// <summary>
        /// 图片列表
        /// </summary>
        public List<string> ListImagePath { get; set; }
    }

    /// <summary>
    /// 会员点赞信息
    /// </summary>
    public class MemberApproval
    {         
        /// <summary>
        /// 会员头像
        /// </summary>
        public string MemberImagePath{get;set;}
    }

    ///// <summary>
    ///// 会员评论信息
    ///// </summary>
    //public class MemberComment
    //{
    //    /// <summary>
    //    /// 会员名称
    //    /// </summary>
    //    public string MemberName { get; set; }

    //    /// <summary>
    //    /// 评论内容
    //    /// </summary>
    //    public string Content { get; set; }
    //}
}
