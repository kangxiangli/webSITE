using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Models
{
    public class M_CommentInfo
    {
        /// <summary>
        /// 会员
        /// </summary>
        public int MemberId { get; set; }

        /// <summary>
        /// 会员名称
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public string Content { get; set; }
    }
}