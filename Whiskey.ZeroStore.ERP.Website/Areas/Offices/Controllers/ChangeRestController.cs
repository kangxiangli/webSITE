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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class ChangeRestController : BaseController
    {

        #region 声明业务层操作对象
                
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ChangeRestController));
        
        protected readonly IAdministratorContract _administratorContract;
        
        protected readonly IDepartmentContract _departmentContract;

        protected readonly IAttendanceContract _attendanceContract;

        protected readonly IRestContract _restContract;
        public ChangeRestController(IAdministratorContract administratorContract,
            IDepartmentContract departmentContract,
            IAttendanceContract attendanceContract,
            IRestContract restContract)
        {
            _administratorContract = administratorContract;
            _departmentContract = departmentContract;
            _attendanceContract = attendanceContract;
            _restContract = restContract;
        }
        #endregion

        #region 初始化操作界面
                
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
            int adminId = AuthorityHelper.OperatorId ?? 0;
            GridRequest request = new GridRequest(Request);
            Expression<Func<Rest, bool>> predicate = FilterHelper.GetExpression<Rest>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _restContract.Rests.Where(x => x.AdminId == adminId).Where<Rest, int>(predicate, request.PageCondition, out count);
                IQueryable<Department> listDepartment = _departmentContract.Departments;
                IQueryable<Administrator> listAdmin = _administratorContract.Administrators;
                var listEntity = from at in list
                                 join
                                  ad in listAdmin                                 
                                 on
                                 at.AdminId equals ad.Id
                                 join
                                 de in listDepartment
                                 on
                                 ad.DepartmentId equals de.Id
                                 select new
                                 {
                                     at.Id,
                                     de.DepartmentName,
                                     ad.Member.MemberName,
                                     
                                 };
                return new GridData<object>(listEntity, count, request.RequestInfo);
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
        public JsonResult Create(RestDto dto)
        {            
            var res = _restContract.Insert(dto);
            return Json(res);
        }
        #endregion
    }
}