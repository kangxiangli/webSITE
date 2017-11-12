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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using Whiskey.ZeroStore.ERP.Transfers.OfficeInfo;
using Whiskey.Utility.Helper;


namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{

    /// <summary>
    /// 工作时间
    /// </summary>
    [License(CheckMode.Verify)]
    public class WorkTimeController : BaseController
    {
        #region 初始化业务层操作对象
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkTimeController));

        protected readonly IWorkTimeContract _workTimeContract;

        protected readonly IAdministratorContract _adminContract;

        public WorkTimeController(IWorkTimeContract workTimeContract,
            IAdministratorContract adminContract)
        {
            _workTimeContract = workTimeContract;
            _adminContract = adminContract;
        }
        #endregion

        #region 初始化工作时间界面
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
            Expression<Func<WorkTime, bool>> predicate = FilterHelper.GetExpression<WorkTime>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<int> listId = new List<int>();
                List<Administrator> listAdmin = _adminContract.Administrators.Where(x => x.WorkTimeId != null).ToList();
                IQueryable<WorkTime> listWorkTime=_workTimeContract.WorkTimes;
                if (listAdmin!=null)
                {
                    listId = listAdmin.Select(x => x.WorkTimeId??0).ToList();
                    listWorkTime =listWorkTime.Where(x=>!listId.Contains(x.Id));
                }
                var list = listWorkTime.Where<WorkTime, int>(predicate, request.PageCondition, out count).OrderByDescending(x => x.CreatedTime).Select(x => new
                {
                    x.Id,
                    x.WorkTimeName,
                    x.WorkHour,
                    x.IsVacations,
                    x.AmStartTime,
                    x.PmEndTime,                    
                    x.IsDeleted,
                    x.IsEnabled,
                    x.IsFlexibleWork,
                    x.WorkTimeType,
                    x.AmEndTime
                });
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加数据界面
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
        public JsonResult Create(WorkTimeDto dto)
        {
            string strWorkWeeks = Request["WorkWeek"];
            dto.WorkWeek = strWorkWeeks;
            var res =  _workTimeContract.Insert(dto);
            return Json(res);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改数据界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            
            var entity = _workTimeContract.Edit(Id);
            ViewBag.WorkHour = entity.WorkHour.ToString();
            return PartialView(entity);
        }

        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Update(WorkTimeDto dto)
        {
            string strWorkWeeks = Request["WorkWeek"];
            dto.WorkWeek = strWorkWeeks;
            dto.IsPersonalTime = false;
            if (dto.IsFlexibleWork)
            {
                dto.PmStartTime = "00:00";
                dto.PmEndTime = "00:00";
                dto.AmStartTime = "00:00";
                dto.AmEndTime = "00:00";
            }
            dto.AmEndTime = string.IsNullOrEmpty(dto.AmEndTime) ? "00:00" : Convert.ToDateTime(dto.AmEndTime).ToShortTimeString();
            dto.AmStartTime = string.IsNullOrEmpty(dto.AmStartTime) ? "00:00" : Convert.ToDateTime(dto.AmStartTime).ToShortTimeString();
            dto.PmEndTime = string.IsNullOrEmpty(dto.PmEndTime) ? "00:00" : Convert.ToDateTime(dto.PmEndTime).ToShortTimeString();
            dto.PmStartTime = string.IsNullOrEmpty(dto.PmStartTime) ? "00:00" : Convert.ToDateTime(dto.PmStartTime).ToShortTimeString();
            dto.AdminId = AuthorityHelper.OperatorId;
            var res = _workTimeContract.Update(dto);
            return Json(res);
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
            var result = _workTimeContract.Remove(Id);
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
            var result = _workTimeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看数据
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity=_workTimeContract.View(Id);
            return PartialView(entity);
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
            var result = _workTimeContract.Enable(Id);
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
            var result = _workTimeContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 部分视图界面
        public ActionResult WorkTime()
        {
            return PartialView();
        }
        #endregion
    }
}