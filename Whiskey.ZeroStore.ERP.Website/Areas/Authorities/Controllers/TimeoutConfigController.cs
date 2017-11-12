using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    [License(CheckMode.Verify)]
    public class TimeoutConfigController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(TimeoutConfigController));
        protected readonly ITimeoutConfigContract _timeoutConfigContract;
        public TimeoutConfigController(ITimeoutConfigContract _timeoutConfigContract)
        {
            this._timeoutConfigContract = _timeoutConfigContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<TimeoutConfig, bool>> predicate = FilterHelper.GetExpression<TimeoutConfig>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;

                var list = _timeoutConfigContract.TimeoutConfigs.Where<TimeoutConfig, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.TaskName,
                    m.ActionPath,
                    m.TimeoutDay,
                    m.TimeoutHour,
                    m.TimeoutMinute,
                    m.TimeoutSecond,
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

        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(TimeoutConfigDto ro)
        {
            var result = _timeoutConfigContract.Insert(ro);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _timeoutConfigContract.Edit(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(TimeoutConfigDto dto)
        {
            var result = _timeoutConfigContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Log]
        public ActionResult View(int Id)
        {
            var result = _timeoutConfigContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _timeoutConfigContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _timeoutConfigContract.Delete(Id);
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
            var result = _timeoutConfigContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _timeoutConfigContract.Enable(Id);
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
            var result = _timeoutConfigContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}