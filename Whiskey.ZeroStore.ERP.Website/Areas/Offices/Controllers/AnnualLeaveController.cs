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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{

    /// <summary>
    /// 年假
    /// </summary>
    [License(CheckMode.Verify)]
    public class AnnualLeaveController : BaseController
    {

        #region 声明业务层操作对象
                
        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(ChangeRestController));

        protected readonly IAnnualLeaveContract _annualLeaveContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IDepartmentContract _departmentContract;

        protected readonly IJobPositionContract _jobPositionContract;

        public AnnualLeaveController(IAnnualLeaveContract annualLeaveContract,
            IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IJobPositionContract jobPositionContract)
        {
            _annualLeaveContract = annualLeaveContract;
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _jobPositionContract = jobPositionContract;
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
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<AnnualLeave, bool>> predicate = FilterHelper.GetExpression<AnnualLeave>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<AnnualLeave>, List<AnnualLeave>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.Where(o=>!o.IsDeleted && o.IsEnabled).OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<AnnualLeave> tree = new List<AnnualLeave>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));  
                    }
                    return tree;
                };

                var count = 0;
                List<AnnualLeave> liatAnn=_annualLeaveContract.AnnualLeaves.Where(x=>x.ParentId==null).Where<AnnualLeave, int>(predicate, request.PageCondition, out count).ToList();
                var list = getTree(liatAnn).AsQueryable().Select(x => new
                { 
                  x.Id,
                  x.ParentId,
                  x.AnnualLeaveName,
                  x.StartYear,
                  x.EndYear,
                  x.Days,
                  x.IsDeleted,
                  x.IsEnabled,
                  x.Sequence
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
            ViewBag.AnnualLeaves = _annualLeaveContract.SelectList("请选择");             
            return PartialView();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(AnnualLeaveDto dto)
        {
            var res = _annualLeaveContract.Insert(dto);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化编辑界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            string title = string.Empty;
            ViewBag.AnnualLeaves = _annualLeaveContract.SelectList("请选择");
            ViewBag.Departments = _departmentContract.SelectList(title);
            var dto = _annualLeaveContract.Edit(Id);
            return PartialView(dto);
        }
       
        [HttpPost] 
        public JsonResult Update(AnnualLeaveDto dto)
        {
            var res = _annualLeaveContract.Update(dto);
            return Json(res, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 初始化部门界面
        /// </summary>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            AnnualLeave entity = _annualLeaveContract.View(Id);
            return PartialView(entity);
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
            var result = _annualLeaveContract.Remove(Id);
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
            var result = _annualLeaveContract.Recovery(Id);
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
            var result = _annualLeaveContract.Enable(Id);
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
            var result = _annualLeaveContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 根据父类获取部门

        /// <summary>
        /// 根据父类获取部门
        /// </summary>
        /// <param name="Id">父级Id</param>
        /// <returns></returns>
        public JsonResult GetDepartment()
        {
            string strParentId = Request["ParentId"];
            List<SelectListItem> list = new List<SelectListItem>();
            if (string.IsNullOrEmpty(strParentId))
            {
                IQueryable<Department> listDep= _departmentContract.Departments.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                var entity = listDep.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text= x.DepartmentName,
                }).ToList();
                list = entity;
            }
            else
            {
                int id = int.Parse(strParentId);
                AnnualLeave ann = _annualLeaveContract.AnnualLeaves.Where(x => x.Id == id).FirstOrDefault();
                var entity = new SelectListItem
                {
                    //Value = ann.Department.Id.ToString(),                    
                    //Text = ann.Department.DepartmentName,
                };
                list.Add(entity);
            }
            return Json(list, JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 根据部门获取职位
        public JsonResult GetJobPosition() 
        {
            try
            {
                string strDepartmentId = Request["DepartmentId"];
                int departmentId = int.Parse(strDepartmentId);
                IQueryable<JobPosition> listJobPosition = _jobPositionContract.JobPositions.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == departmentId);
                var entities = listJobPosition.Select(x => new { 
                  x.Id,
                  x.JobPositionName,
                });
                return Json(new OperationResult(OperationResultType.Success, "获取成功", entities), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "获取失败"),JsonRequestBehavior.AllowGet);
            }
            
        }
        #endregion

        #region 部门视图界面
        public ActionResult AnnualLeave()
        {
            return PartialView();
        }
        #endregion
    }
}