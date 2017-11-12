using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Threading.Tasks;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class JobPositionController : BaseController
    {
        #region 声明业务层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(JobPositionController));

        protected readonly IJobPositionContract _jobPositionContract;
        protected readonly IAppVerManageContract _appVerManageContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IAnnualLeaveContract _annualLeaveContract;
        protected readonly IFactorysContract _factorysContract;
        protected readonly IConfigureContract _configureContract;

        public JobPositionController(IJobPositionContract jobPositionContract,
            IDepartmentContract departmentContract,
            IWorkTimeContract workTimeContract,
            IFactorysContract factorysContract,
            IAppVerManageContract appVerManageContract,
            IAnnualLeaveContract annualLeaveContract,
            IConfigureContract configureContract)
        {
            _jobPositionContract = jobPositionContract;
            _departmentContract = departmentContract;
            _workTimeContract = workTimeContract;
            _annualLeaveContract = annualLeaveContract;
            _factorysContract = factorysContract;
            _appVerManageContract = appVerManageContract;
            _configureContract = configureContract;
        }
        #endregion

        #region 初始化界面
        [Layout]
        public ActionResult Index()
        {
            string title = "请选择";
            var listEntity = _departmentContract.SelectList(title);
            ViewBag.Departments = listEntity;
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
            Expression<Func<JobPosition, bool>> predicate = FilterHelper.GetExpression<JobPosition>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<JobPosition> liatJob = _jobPositionContract.JobPositions.Where<JobPosition, int>(predicate, request.PageCondition, out count);
                var list = liatJob.Select(x => new
                {
                    x.Id,
                    x.JobPositionName,
                    x.Department.DepartmentName,
                    WorkTimeName = x.WorkTime != null ? x.WorkTime.WorkTimeName : "",
                    x.AnnualLeave.AnnualLeaveName,
                    ManagementScope = x.IsLeader ? (x.Departments.Contains(x.Department) ? x.Departments.Count : x.Departments.Count + 1) : x.Departments.Count,
                    FactoryScope = x.Factorys.Count,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.Sequence,
                    x.IsLeader,
                    AppVerScope = x.AppVerManages.Count,
                    x.AllowPwd,
                    x.CheckLogin,
                    x.CheckMac,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult ManagementScope(int id)
        {
            ViewBag.JobPositionId = id;
            return PartialView();
        }
        public ActionResult AppVerScope(int id)
        {
            ViewBag.JobPositionId = id;
            return PartialView();
        }

        public ActionResult ManagementFactoryScope(int Id)
        {
            ViewBag.JobPositionId = Id;
            return PartialView();
        }

        public ActionResult GetFactoryScopeInfo(int Id)
        {
            var model = _jobPositionContract.JobPositions.Where(x => x.Id == Id).FirstOrDefault();
            List<object> list = new List<object>();
            if (model.IsNotNull())
            {
                var lista = model.Factorys.Select(s => new { FactoryName = s.FactoryName }).ToList();
                list.AddRange(lista);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetManagementScopeInfo()
        {
            int id = 0;
            Int32.TryParse(Request["id"], out id);
            var model = _jobPositionContract.JobPositions.Where(x => x.Id == id).FirstOrDefault();
            List<object> list = new List<object>();
            if (model != null)
            {
                if (model.IsLeader)
                {
                    list.Add(new { DepartmentName = model.Department.DepartmentName });
                }
                foreach (var item in model.Departments)
                {
                    if (list.Count > 0)
                    {
                        if (item.DepartmentName != model.Department.DepartmentName)
                        {
                            var da = new
                            {
                                DepartmentName = item.DepartmentName
                            };
                            list.Add(da);
                        }
                    }
                    else
                    {
                        var da = new
                        {
                            DepartmentName = item.DepartmentName
                        };
                        list.Add(da);
                    }
                }
                list = list.Distinct().ToList();
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAppVerScopeInfo()
        {
            int id = 0;
            Int32.TryParse(Request["id"], out id);
            var model = _jobPositionContract.JobPositions.Where(x => x.Id == id).FirstOrDefault();
            List<object> list = new List<object>();
            if (model != null)
            {
                var lista = model.AppVerManages.Where(w => w.IsEnabled && !w.IsDeleted).DistinctBy(d => d.AppType).Select(s => new { AppName = s.AppType + "" }).ToList();
                list.AddRange(lista);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }

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
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(JobPositionDto dto)
        {
            string AuditauthorityStr = Request["Auditauthority"];
            int Auditauthority = 0;
            if (!string.IsNullOrEmpty(AuditauthorityStr))
            {
                switch (AuditauthorityStr)
                {
                    case "1":
                        Auditauthority = 1;
                        break;
                    case "2":
                        Auditauthority = 2;
                        break;
                    case "3":
                        Auditauthority = 3;
                        break;
                    case "1,2":
                        Auditauthority = 4;
                        break;
                    case "1,3":
                        Auditauthority = 5;
                        break;
                    case "2,3":
                        Auditauthority = 6;
                        break;
                    case "1,2,3":
                        Auditauthority = 7;
                        break;
                }
            }
            dto.Auditauthority = Auditauthority;
            var res = _jobPositionContract.Insert(dto);
            return Json(res);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var dto = _jobPositionContract.Edit(Id);
            var data = _departmentContract.Departments.Where(x => dto.DepartIds.Contains(x.Id)).Select(x => new SelectListItem()
            {
                Value = x.Id + "",
                Text = x.DepartmentName,
                Selected = true,
            }).ToList();
            ViewBag.Departs = data;

            var data2 = _factorysContract.SelectFactorys.Where(x => dto.FactoryIds.Contains(x.Id)).Select(x => new SelectListItem()
            {
                Value = x.Id + "",
                Text = x.FactoryName,
                Selected = true,
            }).ToList();
            ViewBag.Factorys = data2;

            var data3 = _appVerManageContract.Entities.Where(x => dto.AppVerIds.Contains(x.Id)).DistinctQueryBy(d => d.AppType).Select(x => new SelectListItem()
            {
                Value = x.Id + "",
                Text = x.AppType + "",
                Selected = true,
            }).ToList();
            ViewBag.AppVers = data3;

            return PartialView(dto);
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(JobPositionDto dto)
        {
            string AuditauthorityStr = Request["Auditauthority"];
            int Auditauthority = 0;
            if (!string.IsNullOrEmpty(AuditauthorityStr))
            {
                switch (AuditauthorityStr)
                {
                    case "1":
                        Auditauthority = 1;
                        break;
                    case "2":
                        Auditauthority = 2;
                        break;
                    case "3":
                        Auditauthority = 3;
                        break;
                    case "1,2":
                        Auditauthority = 4;
                        break;
                    case "1,3":
                        Auditauthority = 5;
                        break;
                    case "2,3":
                        Auditauthority = 6;
                        break;
                    case "1,2,3":
                        Auditauthority = 7;
                        break;
                }
            }
            dto.Auditauthority = Auditauthority;
            var res = _jobPositionContract.Update(dto);
            return Json(res);
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
            var entity = _jobPositionContract.View(Id);
            StringBuilder sbName = new StringBuilder();
            if (entity.Departments != null && entity.Departments.Count > 0)
            {
                foreach (Department item in entity.Departments)
                {
                    sbName.Append(item.DepartmentName + ",");
                }
            }
            ViewBag.Name = sbName.ToString();

            var listf = entity.Factorys.Select(s => s.FactoryName).ToList();
            ViewBag.FacName = string.Join(",", listf);

            var lista = entity.AppVerManages.DistinctBy(d => d.AppType).Select(s => s.AppType + "").ToList();
            ViewBag.AppNames = string.Join(",", lista);

            return PartialView(entity);
        }
        #endregion

        #region 移除和恢复数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _jobPositionContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _jobPositionContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用和禁用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _jobPositionContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _jobPositionContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 部门列表
        public ActionResult Depart()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> DepartList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Department, bool>> predicate = FilterHelper.GetExpression<Department>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                IQueryable<Department> listDepart = _departmentContract.Departments.Where<Department, int>(predicate, request.PageCondition, out count);
                var list = listDepart.Select(x => new
                {
                    x.Id,
                    x.DepartmentName,
                    x.IsDeleted,
                    x.IsEnabled,
                    x.Sequence
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 工厂列表

        public ActionResult Factory()
        {
            return PartialView();
        }


        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> FactoryList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Factorys, bool>> predicate = FilterHelper.GetExpression<Factorys>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                IQueryable<Factorys> listDepart = _factorysContract.SelectFactorys.Where<Factorys, int>(predicate, request.PageCondition, out count);
                var list = listDepart.Select(x => new
                {
                    x.Id,
                    x.FactoryName,
                    x.FactoryAddress,
                    x.Leader,
                    x.IsDeleted,
                    x.IsEnabled,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllFactorySelectList()
        {
            var li = _factorysContract.SelectFactorys.Where(c => c.IsDeleted == false && c.IsEnabled == true).Select(m => new SelectListItem
            {
                Text = m.FactoryName,
                Value = m.Id + "",
            }).ToList();
            return Json(li, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 授权App

        public ActionResult AppVer()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> AppVerList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<AppVerManage, bool>> predicate = FilterHelper.GetExpression<AppVerManage>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                var query = _appVerManageContract.Entities.Where<AppVerManage, int>(predicate, request.PageCondition, out count);
                var list = query.Select(x => new
                {
                    x.Id,
                    AppName = x.AppType + "",
                    x.IsDeleted,
                    x.IsEnabled,
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetAllAppVerSelectList()
        {
            var li = _appVerManageContract.Entities.Where(c => c.IsDeleted == false && c.IsEnabled == true).DistinctQueryBy(b => b.AppType).Select(m => new SelectListItem
            {
                Text = m.AppType + "",
                Value = m.Id + "",
            }).ToList();
            return Json(li, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 登录检测配置

        public ActionResult CheckLoginConfig()
        {
            XmlHelper helper = new XmlHelper("JobPosition", "CheckLoginConfig");
            var xmlDay = helper.GetElement("DayCount");
            ViewBag.DayCount = xmlDay?.Value ?? "7";
            return PartialView();
        }

        [HttpPost]
        public ActionResult CheckLoginConfig(string DayCount)
        {
            var result = OperationHelper.Try((opera) =>
            {
                XmlHelper helper = new XmlHelper("JobPosition", "CheckLoginConfig", true);
                helper.ModifyElement("DayCount", DayCount);
                return OperationHelper.ReturnOperationResult(true, opera);
            }, Operation.Update);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion
    }
}