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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    // GET: Members/MemberHeat
    [License(CheckMode.Verify)]
    public class MemberHeatController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberHeatController));

        protected readonly IMemberHeatContract _memberHeatContract;

        public MemberHeatController(
            IMemberHeatContract _memberHeatContract
            )
        {
            this._memberHeatContract = _memberHeatContract;
        }


        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }


        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberHeat, bool>> predicate = FilterHelper.GetExpression<MemberHeat>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberHeatContract.Entities.Where<MemberHeat, int>(predicate, request.PageCondition, out count)
                .ToList()
                .Select(m => new
                {
                    m.Id,
                    m.HeatName,
                    m.DayStart,
                    m.DayEnd,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.IconPath,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(MemberHeatDto dto)
        {
            var result = _memberHeatContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MemberHeatDto dto)
        {
            var result = _memberHeatContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Update(int Id)
        {
            var result = _memberHeatContract.Edit(Id);
            return PartialView(result);
        }

        public ActionResult View(int Id)
        {
            var result = _memberHeatContract.View(Id);
            return PartialView(result);
        }

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _memberHeatContract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _memberHeatContract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _memberHeatContract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _memberHeatContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}