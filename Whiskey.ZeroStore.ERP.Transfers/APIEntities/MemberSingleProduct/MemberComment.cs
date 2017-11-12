using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.ZeroStore.ERP.Transfers.APIEntities.MemberSingleProduct
{
    public class MemberComment
    {
        /// <summary>
        /// 单品Id
        /// </summary>
        public int ProductId { get; set; }

        /// <summary>
        /// 会员Id
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 评论Id
        /// </summary>
        public int CommentId { get; set; }

        /// <summary>
        /// 评论内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 回复评论Id
        /// </summary>
        public int ReplyId { get; set; }

        /// <summary>
        /// 会员昵称
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 回复会员昵称
        /// </summary>
        public string ReplyMemberName { get; set; }

        /// <summary>
        /// 评论时间
        /// </summary>
        public DateTime CommentTime { get; set; }
    }
}
