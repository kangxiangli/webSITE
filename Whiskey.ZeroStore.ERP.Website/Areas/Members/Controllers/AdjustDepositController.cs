using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using System.Threading;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    public class AdjustDepositController : BaseController
    {

        #region 初始化操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AdjustDepositController));

        protected readonly IAdjustDepositContract _adjustDepositContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IAdministratorContract _adminContract;
        protected readonly INotificationContract _notificationContract;
        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public AdjustDepositController(IAdjustDepositContract adjustDepositContract,
            IMemberContract memberContract,
            INotificationContract _notificationContract,
            IAdministratorContract adminContract)
        {
            _adjustDepositContract = adjustDepositContract;
            _memberContract = memberContract;
            _adminContract = adminContract;
            this._notificationContract = _notificationContract;

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
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "RealName");
            string strRealName = string.Empty;
            if (rule != null)
            {
                strRealName = rule.Value.ToString();
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<AdjustDeposit, bool>> predicate = FilterHelper.GetExpression<AdjustDeposit>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<AdjustDeposit> listAdjustDeposit = _adjustDepositContract.AdjustDeposits;
                if (!string.IsNullOrEmpty(strRealName))
                {
                    IQueryable<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.Member.RealName.Contains(strRealName));
                    listAdjustDeposit = listAdjustDeposit.Where(x => listAdmin.Where(k => k.Id == x.ApplicantId).Count() > 0);
                }
                var list = listAdjustDeposit.Where<AdjustDeposit, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    Applicant = m.ApplicantId == null ? string.Empty : m.Applicant.Member.RealName,
                    Reviewers = m.ReviewersId == null ? string.Empty : m.Reviewers.Member.RealName,
                    //m.Member.MemberName,
                    m.Member.RealName,
                    m.Member.MobilePhone,
                    m.MemberId,
                    Balance = m.Balance,// > 0 ? m.Balance * -1 : m.Balance,
                    Score = m.Score,// > 0 ? m.Score * -1 : m.Score,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.VerifyType,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        public ActionResult Create(int Id, int VerifyType)
        {
            Member member = _memberContract.View(Id);
            AdjustDepositDto dto = new AdjustDepositDto();
            if (member != null)
            {
                dto.MemberId = member.Id;
                dto.MemberName = member.MemberName;
            }
            dto.VerifyType = VerifyType;
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult Create(AdjustDepositDto dto)
        {
            if (dto.VerifyType == (int)VerifyFlag.Pass)
            {
                dto.ReviewersId = AuthorityHelper.OperatorId;
            }
            else if (dto.VerifyType == (int)VerifyFlag.Verifing)
            {
                dto.ApplicantId = AuthorityHelper.OperatorId;
            }
            OperationResult oper = _adjustDepositContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 编辑数据

        #endregion

        #region 初始化搜索会员界面
        /// <summary>
        /// 初始化搜索会员界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Search(int VerifyType)
        {
            ViewBag.VerifyType = VerifyType;
            return PartialView();
        }

        public async Task<ActionResult> SearchMember(string KeyWord)
        {

            string strKeyWord = string.Empty;
            strKeyWord = KeyWord.Replace("\"", string.Empty);
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Member> listMember = _memberContract.Members;
                if (!string.IsNullOrEmpty(KeyWord))
                {
                    listMember = listMember.Where(x => x.MemberName.Contains(KeyWord) || x.MobilePhone.Contains(KeyWord) || x.CardNumber.Contains(KeyWord));
                }
                var list = listMember.Where<Member, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.MemberName,
                    m.CardNumber,
                    m.MobilePhone,
                }).ToList();
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
            var result = _adjustDepositContract.Remove(Id);
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
            var result = _adjustDepositContract.Delete(Id);
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
            var result = _adjustDepositContract.Recovery(Id);
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
            var result = _adjustDepositContract.Enable(Id);
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
            var result = _adjustDepositContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 审核数据
        public JsonResult Verify(int[] Id)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            
            oper = _adjustDepositContract.Verify(true, sendNotificationAction,Id);

            return Json(oper);
        }
      
        #endregion

        #region 查看详情
        public ActionResult View(int Id)
        {
            AdjustDeposit ad = _adjustDepositContract.View(Id);
            return PartialView(ad);
        }
        #endregion

        public ActionResult GetMemberInfo(int id)
        {
          var memberInfo=  _memberContract.Members.Where(x=>x.Id==id).FirstOrDefault();
            return PartialView(memberInfo);
        }

        public ActionResult GetReadCount()
        {
                IQueryable<AdjustDeposit> listAdjustDeposit = _adjustDepositContract.AdjustDeposits;
                var list = listAdjustDeposit.Where(x=>
                x.VerifyType==0&&x.IsDeleted==false&&x.IsEnabled==true).Select(m => new
                {
                    m.Id
                }).ToList();
            return Json(new OperationResult<int>(OperationResultType.Success, string.Empty, list.Count()));
        }


        public ActionResult Export()
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "RealName");
            string strRealName = string.Empty;
            if (rule != null)
            {
                strRealName = rule.Value.ToString();
                request.FilterGroup.Rules.Remove(rule);
            }
            Expression<Func<AdjustDeposit, bool>> predicate = FilterHelper.GetExpression<AdjustDeposit>(request.FilterGroup);
            IQueryable<AdjustDeposit> listAdjustDeposit = _adjustDepositContract.AdjustDeposits;
            if (!string.IsNullOrEmpty(strRealName))
            {
                IQueryable<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.Member.RealName.Contains(strRealName));
                listAdjustDeposit = listAdjustDeposit.Where(x => listAdmin.Where(k => k.Id == x.ApplicantId).Count() > 0);
            }
            var list = listAdjustDeposit.Where(predicate).Select(m => new
            {
                Applicant = m.ApplicantId == null ? string.Empty : m.Applicant.Member.RealName,
                Reviewers = m.ReviewersId == null ? string.Empty : m.Reviewers.Member.RealName,
                //m.Member.MemberName,
                m.Member.RealName,
                m.Member.MobilePhone,
                m.MemberId,
                Balance = m.Balance,// > 0 ? m.Balance * -1 : m.Balance,
                Score = m.Score,// > 0 ? m.Score * -1 : m.Score,
                m.UpdatedTime,
                m.VerifyType,
            }).ToList();

            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "储值积分维护");
        }
    }
}