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
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class WorkLogController : BaseController
    {
        #region 初始化业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkLogController));

        protected readonly IWorkLogAttributeContract _workLogAttributeContract;

        protected readonly IWorkLogContract _workLogContract;

        protected readonly IAdministratorContract _adminContract;

        protected readonly IDepartmentContract _departmentContract;

        public WorkLogController(IWorkLogAttributeContract workLogAttributeContract,
            IWorkLogContract workLogContract,
            IAdministratorContract adminContract,
            IDepartmentContract departmentContract)
        {
            _workLogAttributeContract = workLogAttributeContract;
            _workLogContract = workLogContract;
            _adminContract = adminContract;
            _departmentContract = departmentContract;
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
        public async Task<ActionResult> List(int? FilePath)
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
                    m.Notes
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

            return PartialView();
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Create(WorkLogDto dto)
        {
            dto.StaffId = AuthorityHelper.OperatorId ?? 0;
            OperationResult oper = _workLogContract.Insert(dto);

            AddWorkLogCount(dto.WorkLogAttributeId, 1);
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
            WorkLogDto dto = _workLogContract.Edit(Id);
            return PartialView(dto);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [ValidateInput(false)]
        [HttpPost]
        public JsonResult Update(WorkLogDto dto)
        {
            //数据提交时StaffId莫名丢失，在此重新赋值
            WorkLogDto dto_old = _workLogContract.Edit(dto.Id);
            dto.StaffId = dto_old.StaffId;

            OperationResult res = _workLogContract.Update(dto);
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
            WorkLog entity = _workLogContract.View(Id);
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
            WorkLogDto dto = _workLogContract.Edit(Id);
            OperationResult oper = _workLogContract.Remove(Id);

            AddWorkLogCount(dto.WorkLogAttributeId, -1);
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
            WorkLogDto dto = _workLogContract.Edit(Id);
            OperationResult oper = _workLogContract.Recovery(Id);

            AddWorkLogCount(dto.WorkLogAttributeId, -1);
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
            OperationResult result = _workLogContract.Enable(Id);
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
            OperationResult result = _workLogContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传文件
        [HttpPost]
        public ContentResult UploadFiles(HttpPostedFileBase file)
        {
            string strResult = string.Empty;
            if (file != null)
            {
                DateTime current = DateTime.Now;
                string strBase = ConfigurationHelper.GetAppSetting("WorkLogPath");
                strBase = strBase + current.Year.ToString() + "/" + current.Month.ToString() + "/" + current.Day.ToString() + "/";
                string strFileName = file.FileName;
                string strName = strFileName.Split('.')[0];
                string strNewName = RandomHelper.GetRandomCode(6) + "_" + current.ToString("yyyyMMddHHmmss");
                strFileName = strFileName.Replace(strName, strNewName);
                string savePath = FileHelper.UrlToPath(strBase + strFileName);
                file.SaveAs(savePath);
                strResult = strBase + strFileName;
            }
            return Content(strResult);
            //return Json(strResult);
        }
        #endregion

        #region 获取工作类型
        /// <summary>
        /// lamda递归获取工作类型
        /// </summary>
        /// <returns></returns>
        public JsonResult GetWorkLogAttributes()
        {
            Func<List<WorkLogAttribute>, List<M_WorkLogAttribute>> getTree = null;
            Func<WorkLogAttribute, bool> filter = x => { return x.IsEnabled == true && x.IsDeleted == false; };
            getTree = (source) =>
            {
                List<M_WorkLogAttribute> tree = new List<M_WorkLogAttribute>();
                foreach (WorkLogAttribute attr in source)
                {
                    M_WorkLogAttribute entity = new M_WorkLogAttribute()
                    {
                        WorkLogAttributeName = attr.WorkLogAttributeName,
                    };
                    if (attr.ParentId == null)
                    {
                        entity.Id = null;
                    }
                    else
                    {
                        entity.Id = attr.Id;
                    }
                    entity.Children = getTree(attr.Children.ToList());
                    tree.Add(entity);
                }
                return tree;
            };
            List<WorkLogAttribute> listWorkLogAttribute = _workLogAttributeContract.WorkLogAttributes.Where(filter).Where(x => x.ParentId == null).ToList();
            List<M_WorkLogAttribute> list = getTree(listWorkLogAttribute);
            return Json(list);
        }
        #endregion

        #region lamda-获取工作类型
        /// <summary>
        /// 获取工作类型
        /// </summary>
        /// <returns></returns>
        private List<M_WorkLogAttribute> GetData()
        {
            Func<List<WorkLogAttribute>, List<M_WorkLogAttribute>> getTree = null;
            Func<WorkLogAttribute, bool> filter = x => { return x.IsEnabled == true && x.IsDeleted == false; };
            getTree = (source) =>
            {
                List<M_WorkLogAttribute> tree = new List<M_WorkLogAttribute>();
                foreach (WorkLogAttribute attr in source)
                {
                    M_WorkLogAttribute entity = new M_WorkLogAttribute()
                    {
                        Id = attr.ParentId,
                        WorkLogAttributeName = attr.WorkLogAttributeName,
                    };
                    entity.Children = getTree(attr.Children.ToList());
                    tree.Add(entity);
                }
                return tree;
            };
            List<WorkLogAttribute> listWorkLogAttribute = _workLogAttributeContract.WorkLogAttributes.Where(filter).Where(x => x.ParentId == null).ToList();
            List<M_WorkLogAttribute> list = getTree(listWorkLogAttribute);
            return list;
        }
        #endregion

        #region 递归--获取工作类型

        /// <summary>
        /// 生成选择数据框
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        private List<M_WorkLogAttribute> RecursiveData(List<WorkLogAttribute> list)
        {
            List<M_WorkLogAttribute> listWorkLogAttribute = new List<M_WorkLogAttribute>();
            foreach (WorkLogAttribute attr in list)
            {
                M_WorkLogAttribute entity = new M_WorkLogAttribute()
                {
                    Id = attr.ParentId,
                    WorkLogAttributeName = attr.WorkLogAttributeName,
                };
                entity.Children = RecursiveData(attr.Children.ToList());
            }
            return listWorkLogAttribute;
        }
        #endregion

        /// <summary>
        /// 备注展示
        /// </summary>
        /// <returns></returns>
        public ActionResult ShowNotes(int Id)
        {
            WorkLogDto dto = _workLogContract.Edit(Id);
            ViewBag.Notes = dto.Notes;
            return PartialView();
        }

        /// <summary>
        /// 栏目下工作手册数据更改
        /// </summary>
        /// <param name="workLogAttributeId">该手册内容所属栏目</param>
        /// <param name="num">要更改的数量（增加为正，减少为负）</param>
        /// <returns></returns>
        public bool AddWorkLogCount(int workLogAttributeId, int num)
        {
            try
            {
                while (workLogAttributeId > 0)
                {
                    WorkLogAttributeDto workLogAttribute = _workLogAttributeContract.Edit(workLogAttributeId);
                    workLogAttribute.WorkLogCount = (workLogAttribute.WorkLogCount + num) < 0 ? 0 : workLogAttribute.WorkLogCount + num;
                    _workLogAttributeContract.Update(workLogAttribute);
                    workLogAttributeId = workLogAttribute.ParentId == null ? 0 : Convert.ToInt32(workLogAttribute.ParentId);
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}