using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using System.Text.RegularExpressions;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Articles.Controllers
{
    //[License(CheckMode.Verify)]
    public class AppArticleController : BaseController
    {
        #region 初始化操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppArticleController));

        protected readonly IAppArticleContract _appArticleContract;

        protected readonly ICommentContract _commentContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IApprovalContract _approvalContract; 

        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public AppArticleController(IAppArticleContract appArticleContract,
            ICommentContract commentContract,
            IMemberContract memberContract,
            IApprovalContract approvalContract)
        {
            _appArticleContract = appArticleContract;
            _commentContract = commentContract;
            _memberContract = memberContract;
            _approvalContract = approvalContract;
		}
        #endregion

        #region 初始化界面
              
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion
       
        #region 获取数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            string strMemberName = string.Empty;
            FilterRule rule= request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "MemberName");
            if (rule!=null)
            {
                strMemberName = rule.Value.ToString();
                request.FilterGroup.Rules.Remove(rule);
            }
            string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
            Expression<Func<AppArticle, bool>> predicate = FilterHelper.GetExpression<AppArticle>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                IQueryable<AppArticle> listAppArticle = _appArticleContract.AppArticles;
                if (!string.IsNullOrEmpty(strMemberName))
                {
                    listAppArticle = listAppArticle.Where(x => x.Member.MemberName.Contains(strMemberName));
                }

                var list = from m in listAppArticle.Where<AppArticle, int>(predicate, request.PageCondition, out count)
                           let a = _approvalContract.Approvals.Where(x => x.SourceId == m.Id && x.ApprovalSource == (int)CommentSourceFlag.AppArticle).Where(w=>w.IsEnabled&&!w.IsDeleted)
                           let c = _commentContract.Comments.Where(x => x.SourceId == m.Id && x.CommentSource == (int)CommentSourceFlag.AppArticle).Where(w => w.IsEnabled && !w.IsDeleted)
                           select new
                           {
                               m.Id,
                               m.ArticleTitle,
                               CoverImagePath = strApiUrl + m.CoverImagePath,
                               m.IsDeleted,
                               m.IsEnabled,
                               m.Sequence,
                               m.UpdatedTime,
                               m.CreatedTime,
                               m.Member.MemberName,
                               m.IsRecommend,
                               CommentCount = c.Count(),
                               ApprovalCount = a.Count()
                           };

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion      

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _appArticleContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _appArticleContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _appArticleContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 推荐到APP
        /// <summary>
        /// 推荐到APP
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recommend(int[] Id)
        {
            var result = _appArticleContract.Recommend(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 取消推荐到APP
        /// <summary>
        /// 推荐到APP
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult CancleRecommend(int[] Id)
        {
            var result = _appArticleContract.CancleRecommend(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取编辑数据
        [HttpGet]
        public ActionResult Update(int Id)
        {
            AppArticleDto dto = _appArticleContract.Edit(Id);
            return PartialView(dto);
        }
        #endregion

        #region 提交编辑数据
        /// <summary>
        /// 提交编辑数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(AppArticleDto dto)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (dto.IsRecommend==true)
            {
                oper = _appArticleContract.Recommend(dto.Id);            
            }
            else
            {
                oper = _appArticleContract.CancleRecommend(dto.Id);      
            }
            return Json(oper);
        }
        #endregion

        #region 单品评论
        /// <summary>
        /// 初始化单品评论界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Comment(int Id)
        {
            ViewBag.SingleProId = Id;
            return PartialView();
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> CommentList()
        {
            GridRequest request = new GridRequest(Request);
            //获取要搜索的字段
            string strMemberName = request.FilterGroup.Rules.Where(x => x.Field == "MemberName").FirstOrDefault().Value.ToString();
            string strComment = request.FilterGroup.Rules.Where(x => x.Field == "Comment").FirstOrDefault().Value.ToString();
            //单品Id
            string strId = request.FilterGroup.Rules.Where(x => x.Field == "Id").FirstOrDefault().Value.ToString();
            int id = int.Parse(strId);
            //获取分页信息
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;
            //Expression<Func<SingleProductComment, bool>> predicate = FilterHelper.GetExpression<SingleProductComment>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _commentContract.Comments.Where(x => x.SourceId == id && x.CommentSource == (int)CommentSourceFlag.AppArticle).Select(m => new
                {
                    Comment = m.Content,
                    m.Id,
                    m.MemberId,
                    m.ReplyId,
                    m.CreatedTime,
                    m.IsEnabled,
                });
                IQueryable<Member> listMember = _memberContract.Members;
                var comments = (from l in list
                                join
                                m in listMember
                                on
                                l.MemberId equals m.Id
                                select new
                                {
                                    m.MemberName,
                                    l.Id,
                                    l.Comment,
                                    l.CreatedTime,
                                    l.IsEnabled,
                                });
                var listComm = comments.Where(x => x.MemberName.Contains(strMemberName) || x.Comment.Contains(strComment));
                count = comments == null ? count : listComm.Count();
                var temp = listComm.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new GridData<object>(temp, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除评论

        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DeleteComment(int Id)
        {
            var res = _commentContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 启用禁用评论

        /// <summary>
        /// 启用禁用评论
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DisableComment(int Id)
        {
            var res = new OperationResult(OperationResultType.Error, "数据不存在");
            var mod = _commentContract.View(Id);
            if (mod.IsNotNull())
            {
                mod.IsEnabled = !mod.IsEnabled;
                res = _commentContract.Update(mod);
            }
            return Json(res);
        }

        #endregion

        #region 赞
        public ActionResult Approval(int Id)
        {
            ViewBag.ArticleId = Id;
            return PartialView();
        }

        /// <summary>
        /// 获取评论列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ApprovalList()
        {
            GridRequest request = new GridRequest(Request);
            //获取要搜索的字段
            string strMemberName = request.FilterGroup.Rules.Where(x => x.Field == "MemberName").FirstOrDefault().Value.ToString();
            //单品Id
            string strId = request.FilterGroup.Rules.Where(x => x.Field == "Id").FirstOrDefault().Value.ToString();
            int id = int.Parse(strId);
            //获取分页信息
            int pageIndex = request.PageCondition.PageIndex;
            int pageSize = request.PageCondition.PageSize;            
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _approvalContract.Approvals.Where(x => x.SourceId == id && x.ApprovalSource == (int)CommentSourceFlag.AppArticle).Select(m => new
                {
                    m.Id,
                    m.MemberId,
                    m.CreatedTime,
                });
                IQueryable<Member> listMember = _memberContract.Members;
                var comments = (from l in list
                                join
                                m in listMember
                                on
                                l.MemberId equals m.Id
                                select new
                                {
                                    m.MemberName,
                                    l.Id,
                                    l.CreatedTime,
                                });
                var listComm = comments.Where(x => x.MemberName.Contains(strMemberName));
                count = comments == null ? count : listComm.Count();
                var temp = listComm.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new GridData<object>(temp, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除赞
        /// <summary>
        /// 删除评论
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult DeleteApproval(int Id)
        {
            var res = _approvalContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}