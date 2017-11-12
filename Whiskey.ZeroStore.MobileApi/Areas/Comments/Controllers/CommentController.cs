using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using System.Data.Entity;

namespace Whiskey.ZeroStore.MobileApi.Areas.Comments.Controllers
{
    [License(CheckMode.Verify)]
    public class CommentController : Controller
    {
        #region 初始化业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CommentController));

        //声明业务层操作对象        
        protected readonly ICommentContract _commentContract;


        //构造函数-初始化业务层操作对象
        public CommentController(ICommentContract commentContract)
        {
            _commentContract = commentContract;
        }
        #endregion

        #region 添加评论
        /// <summary>
        /// 添加评论
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Add(string ReplyFromMemberId, string ReplyToCommentId, string Content, string CommentType, string sourceId)
        {
            try
            {
                string strMemberId = ReplyFromMemberId;
                string strContent = Content;
                string strReplyId = ReplyToCommentId;
                string strCommentSource = CommentType;
                string strSourceId = sourceId;

                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "登录异常，请稍后重试！"));
                if (strContent.Length == 0 && strContent.Length > 200 && string.IsNullOrEmpty(strContent)) return Json(new OperationResult(OperationResultType.Error, "评论内容1～200个字符"));
                //判断评论中是否包含敏感词
                bool checkRes = false;//_sensitiveWordContract.CheckComment(strComment);
                if (checkRes)
                {
                    return Json(new OperationResult(OperationResultType.Error, "您的评论包含非法字符，请重新输入！"));
                }
                else
                {
                    CommentDto commentDto = new CommentDto();
                    commentDto.MemberId = int.Parse(strMemberId);
                    commentDto.Content = strContent;
                    commentDto.SourceId = int.Parse(strSourceId);

                    //判断是否是回复一条评论
                    if (!string.IsNullOrEmpty(strReplyId) && strReplyId != "-1")
                    {
                        //校验是否有改条评论
                        var replId = int.Parse(strReplyId);
                        var replEntity = _commentContract.Comments.FirstOrDefault(comment => !comment.IsDeleted && comment.IsEnabled && comment.Id == replId);
                        if (replEntity == null)
                        {
                            return Json(new OperationResult(OperationResultType.Error, "找不到要被回复的那条评论"));
                        }
                        commentDto.ReplyId = replId;
                    }
                    commentDto.CommentSource = int.Parse(strCommentSource);
                    var insertRes = _commentContract.Insert(commentDto);
                    return Json(insertRes, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }
        }
        #endregion
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        [HttpPost]
        public JsonResult GetList(int SourceId, int CommentType, int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                var query = _commentContract.Comments.Where(c => c.SourceId == SourceId && c.CommentSource == CommentType && c.IsDeleted == false && c.IsEnabled == true).Include(c => c.Reply.Member);
                var allCount = query.Count();
                var list = query.OrderBy(c => c.CreatedTime)
                .Skip((PageIndex - 1) * PageSize)
                .Take(PageSize)
                .Select(c => new
                {
                    c.Id,             // commentId
                    c.CommentSource,  // comment type
                    c.SourceId,       // comment source ,an app article or a comment
                    c.ReplyId,        // if a comment's target is also a comment,the replyid  refers to the target comment's id
                    c.Reply,          // if a comment's target is also a comment,the reply refers to the target comment data
                    c.Content,        // comment text
                    c.MemberId,       // the member' id who send the comment
                    c.Member.MemberName, //the member's name who send the comment
                    c.Member.UserPhoto,  //the member's avatar who send the comment
                    c.CreatedTime       //the comment created time
                })
               .ToList();
                var resData = list.Select(item => new
                {
                    Id = item.Id,
                    CommentType = item.CommentSource,
                    SourceId = item.SourceId,
                    ReplyToCommentID = item.ReplyId ?? -1,
                    ReplyToMemberName = item.Reply == null ? string.Empty : item.Reply.Member.MemberName,
                    ReplyFromMemberId = item.MemberId,
                    ReplyFromMemberName = item.MemberName,
                    ReplyFromMemberPhoto = string.IsNullOrEmpty(item.UserPhoto) ? string.Empty : strWebUrl + item.UserPhoto,
                    Content = item.Content,
                    CreatedTime = item.CreatedTime.ToUnixTime()
                }).ToList();

                var res = new PagedOperationResult(OperationResultType.Success, string.Empty, resData) { AllCount = allCount, PageSize = PageSize };
                return Json(res);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！"));
            }

        }

        private class CommentResDTO
        {

            public int Id { get; set; }
            public int CommentSource { get; set; }
            public int SourceId { get; set; }

            public int ReplyFromMemberId { get; set; }
            public string ReplyFromMemberName { get; set; }
            public string ReplyFromMemberPhoto { get; set; }

            public int ReplyToCommentID { get; set; }
            public string ReplyToMemberName { get; set; }

            public string Content { get; set; }
            public long CreatedTime { get; set; }
        }


    }
}