using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Product;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Warehouse;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.ZeroStore.MobileApi.Areas.Circles.Models;
namespace Whiskey.ZeroStore.MobileApi.Areas.Circles.Controllers
{
    [License(CheckMode.Verify)]
    public class CircleController : Controller
    {

        #region 声业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(CircleController));

        protected readonly ICircleContract _circleContract;

        protected readonly ITopicContract _topicContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IMemberContract _memberContract;
        public CircleController(ICircleContract circleContract,
            ITopicContract topicContract,
            ICommentContract commentContract,
            IMemberContract memberContract)
        {
            _circleContract = circleContract;
            _topicContract = topicContract;
            _commentContract = commentContract;
            _memberContract = memberContract;
        }
        #endregion
        string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
        #region 获取圈子列表
        /// <summary>
        /// 获取圈子列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCircleList(int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strMemberId= Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                IQueryable<Circle> listCircle =  _circleContract.Circles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                listCircle = listCircle.OrderBy(x => x.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize);
                var entity = listCircle.Select(x => new {
                    CircleId=x.Id,
                    x.CircleName,
                    IconPath=strApiUrl+x.IconPath,
                    x.Notes,                   
                    JoinType = x.Members.Where(k=>k.Id==memberId).Count()==0?0:1,//0表示没有加入，1标识已经加入
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功", entity));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "获取失败"));                
            }
                
        }
        #endregion

        #region 获取圈子话题
        /// <summary>
        /// 获取圈子话题
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTopicList(int PageIndex = 1, int PageSize = 10)
        {
            try
            {
                string strCircleId = Request["CircleId"];
                string strMemberId = Request["MemberId"];
                int circleId = int.Parse(strCircleId);
                int memberId = int.Parse(strMemberId);
                Circle circle = _circleContract.View(circleId);
                List<Topic> listTopic = new List<Topic>();
                listTopic = circle.Topics.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CircleId == circleId).ToList();
                var list= listTopic.Select(x => new { 
                 TopicId= x.Id,
                 x.TopicName,                 
                 x.MemberId,
                 x.Member.MemberName,
                 x.Member.UserPhoto,                 
                 CreatedTime=x.CreatedTime.ToString("yyyy-MM-dd"),
                 x.TopicImages.FirstOrDefault().ImagePath,
                
                });
                var entity = new {
                    CircleId = circle.Id,
                    circle.CircleName,
                    circle.Notes,
                    JoinType = circle.Members.Where(k => k.Id == memberId).Count() == 0 ? 0 : 1,//0表示没有加入，1标识已经加入
                    Topics = list,
                };
                return Json(new OperationResult(OperationResultType.Success, "获取成功", entity));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
            
        }
        #endregion

        #region 查看话题
        /// <summary>
        /// 查看话题
        /// </summary>
        /// <returns></returns>
        public JsonResult GetTopic(int PageIndex=1,int PageSize=10)
        {
            try
            {
                string strTopicId = Request["TopicId"];
                int topicId = int.Parse(strTopicId);
                Topic topic = _topicContract.View(topicId);
                if (topic==null || topic.IsDeleted==true || topic.IsEnabled==false)
                {
                    return Json(new OperationResult(OperationResultType.Error, "该话题已经不存在"));
                }
                else
                {

                    IQueryable<Comment> listComment = _commentContract.Comments.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.CommentSource == (int)CommentSourceFlag.Topic && x.SourceId == topicId).OrderByDescending(x=>x.CreatedTime);
                    listComment = listComment.Skip((PageIndex - 1) * PageSize).Take(PageSize);
                    IQueryable<Member> listMember = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                    var comments = (from co in listComment
                                   join
                                   me in listMember
                                   on
                                   co.MemberId equals me.Id
                                   select new { 
                                     CommentId= co.Id,
                                     me.MemberName,
                                     me.UserPhoto,
                                     co.Content,
                                     co.CreatedTime,
                                   }).ToList();                    
                    var entity = new {
                        TopicId=topic.Id,
                        topic.TopicName,
                        topic.Content,
                        topic.Member.MemberName,
                        topic.Member.UserPhoto,
                        TopicImages=topic.TopicImages.Select(x=>new {
                           x.ImagePath,
                         }),                        
                        Comments = comments,
                    };
                    return Json(new OperationResult(OperationResultType.Success, "获取成功", entity));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        #region 加入圈子
        /// <summary>
        /// 加入圈子
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddCircle() 
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strCircleId = Request["CircleId"];
                int memberId = int.Parse(strMemberId);
                int circleId = int.Parse(strCircleId);
                var oper = _circleContract.AddCircle(memberId, circleId);
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }         
        }
        #endregion

        #region 留言
        /// <summary>
        /// 留言
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult LeaveMsg()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strTopicId = Request["TopicId"];
                string strContent = Request["Content"];
                int memberId = int.Parse(strMemberId);
                int topicId = int.Parse(strTopicId);
                if(string.IsNullOrEmpty(strContent) || strContent.Length>200)
                {
                    return Json(new OperationResult(OperationResultType.Error, "留言内容在4～200个字符之间"));
                }
                else
                {
                    CommentDto dto = new CommentDto()
                    {
                        CommentSource = (int)CommentSourceFlag.Topic,
                        Content = strContent,
                        MemberId = memberId,
                        SourceId = topicId,
                    };
                    var res =  _commentContract.Insert(dto);
                    return Json(res);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        #region 添加话题
        /// <summary>
        /// 添加话题
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddTopic(M_Topic m_Topic)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error,"服务器忙，请稍后重试");
            try
            {            
                
                oper = this.CheckParameter(m_Topic);
                if (oper.ResultType==OperationResultType.Success)
                {
                    TopicDto dto = oper.Data as TopicDto;
                    HttpFileCollectionBase files =Request.Files;
                    oper = this.SaveImage(files);
                    if (oper.ResultType==OperationResultType.Success)
                    {
                         List<TopicImage>  listTopicImage = oper.Data as List<TopicImage>;
                         oper = _topicContract.Insert(dto);
                         return Json(oper);   
                    }
                    else
                    {
                        return Json(oper);
                    }
                }
                else
                {
                    return Json(oper);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());                
                return Json(oper);
            }
        }
        #endregion

        #region 校验参数
        private OperationResult CheckParameter(M_Topic m_Topic)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            string strMemberId = m_Topic.MemberId;
            string strCircleId = m_Topic.CircleId;
            string strTopicName = m_Topic.TopicName;
            string strContent = m_Topic.Content;
            int memberId = int.Parse(strMemberId);
            int circleId = int.Parse(strCircleId);
            TopicDto dto = new TopicDto();
            if (string.IsNullOrEmpty(strTopicName) || strTopicName.Length > 20)
            {
                oper.Message = "标题长度在1-20字符之间";
                return oper;
            }
            if (string.IsNullOrEmpty(strContent) || strContent.Length > 200)
            {
                oper.Message = "内容长度在1-200字符之间";
                return oper;
            }            
            dto.CircleId = circleId;
            dto.MemberId = memberId;
            dto.Content = strContent;
            dto.TopicName = strTopicName;
            oper.ResultType = OperationResultType.Success;
            oper.Data = dto;
            return oper;
        }
        #endregion

        #region 保存图片
        private OperationResult SaveImage(HttpFileCollectionBase files)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            //HttpFileCollectionBase files = Request.Files;
            List<TopicImage>  listTopicImage = new List<TopicImage>();
            if (files.Count == 0)
            {
                oper.Message = "请选择图片";
                return oper;
            }
            else
            {
                string strPath = ConfigurationHelper.GetAppSetting("TopicImagePath");
                StringBuilder sbPath = new StringBuilder();
                for (int i = 0; i < files.Count; i++)
                {                    
                    sbPath.Append(strPath + files[i].InputStream.ToString().MD5Hash() + ".jpg");
                    string res = ImageHelper.MakeThumbnail(files[i].InputStream, sbPath.ToString(), 500, 500, "H", "Jpg");
                    if (string.IsNullOrEmpty(res))
                    {
                        oper.Message = "上传图片失败";
                        return oper;
                    }
                    else
                    {
                        listTopicImage.Add(new TopicImage()
                        {
                            ImagePath = sbPath.ToString(),
                        });
                    }
                    sbPath.Clear();
                }
                oper.ResultType = OperationResultType.Success;
                oper.Data = listTopicImage;
                return oper;
            }
        }
        #endregion
    }
}