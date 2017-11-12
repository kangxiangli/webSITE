using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
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
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.Text;
using Whiskey.Utility.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Comment;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberCollocationController : BaseController
    {

        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberSingleProductController));
        //声明业务层操作对象


        protected readonly IMemberContract _memberContract;

        protected readonly ICommentContract _singleCommentContract;

        protected readonly IColorContract _colorContract;

        protected readonly ISeasonContract _seasonContract;

        protected readonly ISizeContract _sizeContract;

        protected readonly ICategoryContract _categoryContract;

        protected readonly IProductAttributeContract _productAttrContract;

        protected readonly IMemberCollocationContract _memberCollocationContract;

        protected readonly IMemberColloEleContract _memberColloEleContract;

        protected readonly IApprovalContract _ApprovalContract;
        protected readonly IMemberSingleProductContract _memberSingleProductContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly ICommentContract _commentContract;
        protected readonly IApprovalContract _approvalContract;

        //构造函数-初始化业务层操作对象
        public MemberCollocationController(IMemberCollocationContract memberCollocationContract,
            IMemberContract memberContract,
            ICommentContract singleCommentContract,
            IColorContract colorContract,
            ISeasonContract seasonContract,
            ISizeContract sizeContract,
            ICategoryContract categoryContract,
            IProductAttributeContract productAttrContract,
            IMemberColloEleContract memberColloEleContract,
            IApprovalContract ApprovalContract,
            IMemberSingleProductContract memberSingleProductContract,
            IAdministratorContract administratorContract,
            ICommentContract commentContract,
           IApprovalContract approvalContract)
        {
            _memberCollocationContract = memberCollocationContract;
            _memberContract = memberContract;
            _singleCommentContract = singleCommentContract;
            _colorContract = colorContract;
            _seasonContract = seasonContract;
            _sizeContract = sizeContract;
            _categoryContract = categoryContract;
            _productAttrContract = productAttrContract;
            _memberColloEleContract = memberColloEleContract;
            _ApprovalContract = ApprovalContract;
            _memberSingleProductContract = memberSingleProductContract;
            _administratorContract = administratorContract;
            _commentContract = commentContract;
            _approvalContract = approvalContract;

        }
        #endregion

        #region 初始化操作对象
        /// <summary>
        /// 初始化操作对象
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取数据列表

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            var filterRules = request.FilterGroup.Rules.Where(x => x.Field == "MemberName").FirstOrDefault();
            string memberName = string.Empty;
            if (filterRules != null)
            {
                memberName = filterRules.Value.ToString();
                request.FilterGroup.Rules.Remove(filterRules);
            }
            string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
            Expression<Func<MemberCollocation, bool>> predicate = FilterHelper.GetExpression<MemberCollocation>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                var listMemberCollo = _memberCollocationContract.MemberCollocations.Where<MemberCollocation, int>(predicate, request.PageCondition, out count);

                if (!string.IsNullOrEmpty(memberName))
                {
                    listMemberCollo = listMemberCollo.Where(x => x.Member.MemberName == memberName);
                }
                var commentQuery = _commentContract.Comments.Where(x => !x.IsDeleted && x.IsEnabled && x.CommentSource == (int)CommentSourceFlag.MemberCollocation);
                var approvalQuery = _approvalContract.Approvals.Where(x => !x.IsDeleted && x.IsEnabled && x.ApprovalSource == (int)CommentSourceFlag.MemberCollocation);
                var collocationElementQuery = _memberColloEleContract.MemberColloEles.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Parent == null);
                var result = from memberCollo in listMemberCollo
                             join
                             ColloEle in collocationElementQuery
                             on
                             memberCollo.Id equals ColloEle.MemberColloId

                             select new
                             {
                                 memberCollo.Id,
                                 memberCollo.Member.MemberName,
                                 memberCollo.CollocationName,
                                 memberCollo.CreatedTime,
                                 AdminName = memberCollo.Operator.Member.MemberName,
                                 ImagePath = strApiUrl + ColloEle.ImagePath,           //搭配封面图
                                 memberCollo.IsDeleted,
                                 memberCollo.IsEnabled,
                                 memberCollo.IsRecommend,
                                 ApprovalCount = commentQuery.Count(c => c.SourceId == memberCollo.Id),
                                 CommentCount = approvalQuery.Count(c => c.SourceId == memberCollo.Id),
                                 RecommendMemberCount = memberCollo.RecommendMembers.Count() // 推荐会员数量
                             };

                return new GridData<object>(result, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult MemberSelect(int id, int isLimit = 0)
        {
            var recommendMemberIds = _memberCollocationContract.MemberCollocations.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == id)
                .SelectMany(m => m.RecommendMembers.Select(me => me.MemberId)).ToList();
            ViewBag.RecommendMemberIds = JsonHelper.ToJson(recommendMemberIds);
            ViewBag.Id = id;
            ViewBag.IsLimit = isLimit;
            return PartialView();
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberList(string name, string mobilePhone, string recommendIds, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memberContract.Members;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                query = query.Where(e => e.MemberName.StartsWith(name) || e.RealName.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.MobilePhone.StartsWith(mobilePhone));
            }

            if (!string.IsNullOrEmpty(recommendIds) && recommendIds.Length > 0)
            {

                var arr = recommendIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i));

                query = query.Where(e=>arr.Contains(e.Id));

            }


            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.MemberName,
                                e.RealName,
                                e.MobilePhone,
                                e.Store.StoreName,
                                IsChecked = false
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            MemberCollocation memberCollo = _memberCollocationContract.MemberCollocations.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == Id).FirstOrDefault();
            IQueryable<MemberColloEle> listMemberColloEle = _memberColloEleContract.MemberColloEles.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            Color color = _colorContract.Colors.Where(x => x.Id == memberCollo.ColorId).FirstOrDefault();
            ProductAttribute situation = _productAttrContract.ProductAttributes.Where(x => x.Id == memberCollo.SituationId).FirstOrDefault();
            ViewBag.ImagePath = listMemberColloEle.Where(x => x.MemberColloId == memberCollo.Id && x.ParentId == null).FirstOrDefault().ImagePath;
            ViewBag.Color = color == null ? "无" : color.ColorName;
            ViewBag.ProductAttr = _memberSingleProductContract.GetAttrNames(memberCollo.ProductAttrId);

            ViewBag.Situation = situation == null ? "无" : situation.AttributeName;
            return PartialView(memberCollo);
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
            var result = _memberCollocationContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
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
            var result = _memberCollocationContract.Recommend(Id);
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
            var result = _memberCollocationContract.CancleRecommend(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var isDel = _memberColloEleContract.DeleteByMemberColloId(Id);
            if (isDel.ResultType == OperationResultType.Success)
            {
                var result = _memberCollocationContract.Delete(Id);
                return Json(result, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(isDel, JsonRequestBehavior.AllowGet);
            }
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
            var result = _memberCollocationContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _memberCollocationContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _memberCollocationContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 评论
        /// <summary>
        /// 初始化评论界面
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
            //Expression<Func<SingleComment, bool>> predicate = FilterHelper.GetExpression<SingleComment>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _singleCommentContract.Comments.Where(x => x.SourceId == id && x.CommentSource == (int)CommentSourceFlag.MemberCollocation).Select(m => new
                {
                    Comment = m.Content,
                    m.Id,
                    m.MemberId,
                    m.ReplyId,
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
                                    l.Comment,
                                    l.CreatedTime,
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
            var res = _singleCommentContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 赞
        public ActionResult Approval(int Id)
        {
            ViewBag.SingleProId = Id;
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
            //Expression<Func<SingleComment, bool>> predicate = FilterHelper.GetExpression<SingleComment>(request.FilterGroup);

            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _ApprovalContract.Approvals.Where(x => x.ApprovalSource == id && x.ApprovalSource == (int)CommentSourceFlag.MemberCollocation).Select(m => new
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
            var res = _ApprovalContract.Delete(Id);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        [HttpPost]
        public ActionResult GetMemberProfile()
        {
            var adminId = AuthorityHelper.OperatorId.Value;
            var res = _administratorContract.GetMemberProfile(adminId);
            return Json(res);
        }

        /// <summary>
        /// 更新要推荐的会员
        /// </summary>
        /// <param name="id"></param>
        /// <param name="recommendMemberIds"></param>
        /// <returns></returns>
        public ActionResult SaveMemberId(int id, params int[] recommendMemberIds)
        {
            var res = _memberCollocationContract.SaveRecommendMemberId(id, recommendMemberIds);
            return Json(res);
        }

        public ActionResult Update(int id)
        {
            ViewBag.Id = id;
            ViewBag.MemberId = _memberCollocationContract.View(id).MemberId;
            return PartialView();
        }
    }
}