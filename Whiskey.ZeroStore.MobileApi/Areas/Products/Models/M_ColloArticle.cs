using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.MobileApi.Areas.Members.Models;

namespace Whiskey.ZeroStore.MobileApi.Areas.Products.Models
{
    public class M_ColloArticle
    {
        /// <summary>
        /// 数据标识
        /// </summary>
        public ColloArticleFlag ColloArticleType { get;set;}

        /// <summary>
        /// 标识Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 会员名称
        /// </summary>
        public string MemberName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserPhoto { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// 点赞次数
        /// </summary>
        public int ApprovalCount { get; set; }

        /// <summary>
        /// 图片
        /// </summary>
        public string CoverImagePath { get; set; }

        /// <summary>
        /// 评论次数
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// 知否点赞
        /// </summary>
        public int IsApproval { get; set; }

        /// <summary>
        /// 用户评论头像
        /// </summary>
        public List<string> UserPhotos { get; set; }

        /// <summary>
        /// 图片集合
        /// </summary>
        public List<string> ListImage { get; set; }

        /// <summary>
        /// 评论
        /// </summary>
        public List<M_CommentInfo> CommentInfos { get; set; }
    }
}