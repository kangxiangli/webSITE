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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    public class HolidayController : Controller
    {
        #region 声明业务层操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(HolidayController));
        
        protected readonly IAdministratorContract _administratorContract;

        protected readonly IHolidayContract _holidayContract;

        public HolidayController(IAdministratorContract administratorContract,
            IHolidayContract holidayContract)
        {
            _administratorContract = administratorContract;
            _holidayContract = holidayContract;             
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
            string strRealName = string.Empty;
            var field = request.FilterGroup.Rules.Where(x => x.Field == "RealName").FirstOrDefault();
            if (field!=null)
            {
                strRealName = field.Value.ToString();
                request.FilterGroup.Rules.Remove(field);
            }
            Expression<Func<Holiday, bool>> predicate = FilterHelper.GetExpression<Holiday>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Holiday> listHoliday = _holidayContract.Holidays;
                 
                var list = listHoliday.Where<Holiday, int>(predicate, request.PageCondition, out count).Select(m => new
                {       
                    m.HolidayName,
                    m.StartTime,
                    m.HolidayDays,
                    m.EndTime,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 添加数据

        #region 出初始化添加界面

        /// <summary>
        /// 出初始化添加界面
        /// </summary>
        /// <returns></returns>
        
        public ActionResult Create()
        {
            return PartialView();
        }
        #endregion

        [HttpPost]
        public JsonResult Create(HolidayDto dto)
        {
            
            OperationResult oper = _holidayContract.Insert(dto);
            
            return Json(oper);
        }
        
        #endregion
 
        #region 编辑数据
        
        public ActionResult Update(int Id)
        {
            var rest = _holidayContract.Edit(Id);
            ViewBag.StartTime = rest.StartTime.ToString("yyyy/MM/dd");
            ViewBag.EndTime = rest.EndTime.ToString("yyyy/MM/dd");
            return PartialView(rest);
        }
        [HttpPost]
        public JsonResult Update(HolidayDto dto)
        {
            OperationResult oper = _holidayContract.Update(dto);             
            return Json(oper);             
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
            var entity = _holidayContract.View(Id);            
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
            var oper = _holidayContract.Remove(Id);             
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
            var res = _holidayContract.Recovery(Id);            
            return Json(res);
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
            var res = _holidayContract.Enable(Id);            
            return Json(res, JsonRequestBehavior.AllowGet);
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
            var result = _holidayContract.Disable(Id);            
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion
        
    }
}