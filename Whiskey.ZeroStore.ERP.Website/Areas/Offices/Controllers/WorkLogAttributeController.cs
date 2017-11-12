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
using Whiskey.Utility.Helper;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class WorkLogAttributeController : BaseController
    {
        #region 初始化业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkLogAttributeController));

        protected readonly IWorkLogAttributeContract _workLogAttributeContract;
        protected readonly IWorkLogContract _workLogContract;
        protected readonly IAdministratorContract _adminContract;

        public WorkLogAttributeController(IWorkLogAttributeContract workLogAttributeContract
            , IWorkLogContract workLogContract
            , IAdministratorContract adminContract)
        {
            _workLogAttributeContract = workLogAttributeContract;
            _workLogContract = workLogContract;
            _adminContract = adminContract;
        }
        #endregion

        #region 初始化界面

        /// <summary>
        /// 初始化界面
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
            Expression<Func<WorkLogAttribute, bool>> predicate = FilterHelper.GetExpression<WorkLogAttribute>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<WorkLogAttribute>, List<WorkLogAttribute>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<WorkLogAttribute> tree = new List<WorkLogAttribute>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
                int count = 0;
                var parents = _workLogAttributeContract.WorkLogAttributes.Where(m => m.ParentId == null).Where<WorkLogAttribute, int>(predicate, request.PageCondition, out count).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.WorkLogAttributeName,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.WorkLogCount
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据

        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            string title = "请选择";
            List<SelectListItem> parent = _workLogAttributeContract.SelectList(title);
            ViewBag.Parents = parent;
            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(WorkLogAttributeDto dto)
        {
            OperationResult oper = _workLogAttributeContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            string title = "请选择";
            List<SelectListItem> parent = _workLogAttributeContract.SelectList(title);
            ViewBag.Parents = parent;
            WorkLogAttributeDto dto = _workLogAttributeContract.Edit(Id);
            return PartialView(dto);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(WorkLogAttributeDto dto)
        {
            OperationResult res = _workLogAttributeContract.Update(dto);
            return Json(res);
        }
        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            WorkLogAttribute entity = _workLogAttributeContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Remove(int Id)
        {
            OperationResult oper = _workLogAttributeContract.Remove(Id);
            return Json(oper);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult Recovery(int Id)
        {
            OperationResult oper = _workLogAttributeContract.Recovery(Id);
            return Json(oper);
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
            OperationResult result = _workLogAttributeContract.Enable(Id);
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
            OperationResult result = _workLogAttributeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 工作手册查看
        /// <summary>
        /// 获取该目录及其子目录的ID集合
        /// </summary>
        /// <param name="workLogAttributeId">要获取的目录</param>
        /// <param name="isFirst">是否首次调用该方法</param>
        /// <param name="workLogAttributeIds">返回的目录ID集合</param>
        /// <returns></returns>
        private IEnumerable<int> GetWorkLogIds(int workLogAttributeId, bool isFirst, ref IEnumerable<int> workLogAttributeIds)
        {
            if (isFirst)
            {
                workLogAttributeIds = new int[] { };
            }
            WorkLogAttribute workLogAttribute = _workLogAttributeContract.WorkLogAttributes.Single(w => w.Id == workLogAttributeId);

            if (workLogAttribute == null)
            {
                return workLogAttributeIds.DistinctBy(i => i);
            }
            workLogAttributeIds = workLogAttributeIds.Concat(new int[] { workLogAttribute.Id });

            if (workLogAttribute.Children == null || workLogAttribute.Children.Count() == 0)
            {
                return workLogAttributeIds.DistinctBy(i => i);
            }
            foreach (var _workLogAttribute in workLogAttribute.Children)
            {
                GetWorkLogIds(_workLogAttribute.Id, false, ref workLogAttributeIds);
            }
            return workLogAttributeIds.DistinctBy(i => i);
        }

        public ActionResult GetSelectWorkLog(int id)
        {
            ViewBag.WorkLogAttributeId = id;
            return PartialView();
        }

        public async Task<ActionResult> GetSelectWorkLogList(int? FilePath)
        {
            GridRequest request = new GridRequest(Request);
            FilterRule rule = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "RealName");
            string strRealName = string.Empty;
            if (rule != null)
            {
                strRealName = rule.Value.ToString();
                request.FilterGroup.Rules.Remove(rule);
            }
            FilterRule rule_file = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "FilePath");
            if (rule_file != null)
            {
                request.FilterGroup.Rules.Remove(rule_file);
            }
            FilterRule rule_workLogAttributeId = request.FilterGroup.Rules.FirstOrDefault(x => x.Field == "WorkLogAttributeId");
            IEnumerable<int> workLogAttributeIds = new int[] { };
            if (rule_workLogAttributeId != null)
            {
                request.FilterGroup.Rules.Remove(rule_workLogAttributeId);
                GetWorkLogIds(Convert.ToInt32(rule_workLogAttributeId.Value), true, ref workLogAttributeIds).ToArray();
            }
            Expression<Func<WorkLog, bool>> predicate = FilterHelper.GetExpression<WorkLog>(request.FilterGroup);
            //只有是部门领导才可以查看部门下的工作
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _adminContract.View(adminId);
            List<int> listId = new List<int>() { adminId };
            if (admin != null && admin.JobPosition.IsLeader == true)
            {
                ICollection<Administrator> listAdmins = admin.Department.Administrators;
                if (!string.IsNullOrEmpty(strRealName))
                {
                    listAdmins = listAdmins.Where(x => x.Member.RealName.Contains(strRealName)).ToList();
                }
                listId = listAdmins.Select(x => x.Id).ToList();
            }

            if (FilePath.HasValue)
            {
                if (FilePath.Value == 1)
                {
                    predicate = predicate.And(p => !string.IsNullOrEmpty(p.FilePath));
                }
                else
                {
                    predicate = predicate.And(p => string.IsNullOrEmpty(p.FilePath));
                }
            }

            if (workLogAttributeIds != null && workLogAttributeIds.Count() > 0)
            {
                predicate = predicate.And(p => workLogAttributeIds.Contains(p.WorkLogAttributeId));
            }

            var data = await Task.Run(() =>
            {
                int count;
                IQueryable<WorkLog> listWorkLog = _workLogContract.WorkLogs;
                if (listId != null && listId.Count() > 0)
                {
                    listWorkLog = listWorkLog.Where(x => listId.Contains(x.StaffId ?? 0));
                }
                var list = listWorkLog.Where<WorkLog, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.WorkLogName,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    WorkLogAttributeName = m.WorkLogAttribute.WorkLogAttributeName,
                    m.FilePath,
                    RealName = m.StaffId == null ? string.Empty : m.Staff.Member.RealName,
                    m.Keys,
                    m.Notes,
                    m.WorkLogAttributeId
                }).ToList();

                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion
    }
}