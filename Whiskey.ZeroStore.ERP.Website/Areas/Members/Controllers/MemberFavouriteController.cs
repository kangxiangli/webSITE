﻿




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

    [License(CheckMode.Verify)]
	public class MemberFavouriteController : BaseController
	{
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberFavouriteController));

        protected readonly IMemberFavouriteContract _memberfavouriteContract;

		public MemberFavouriteController(IMemberFavouriteContract memberfavouriteContract) {
			_memberfavouriteContract = memberfavouriteContract;
		}


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
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


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(MemberFavouriteDto dto)
        {
            var result = _memberfavouriteContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MemberFavouriteDto dto)
        {
            var result = _memberfavouriteContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result= _memberfavouriteContract.Edit(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _memberfavouriteContract.View(Id);
            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberFavourite, bool>> predicate = FilterHelper.GetExpression<MemberFavourite>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberfavouriteContract.MemberFavourites.Where<MemberFavourite, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.MemberId,
                    m.ProductId,
                    m.TagPrice,
                    m.RetailPrice,
                    m.WholesalePrice,
                    m.PurchasePrice,
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


        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Remove(int[] Id)
        {
			var result = _memberfavouriteContract.Remove(Id);
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
			var result = _memberfavouriteContract.Delete(Id);
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
			var result = _memberfavouriteContract.Recovery(Id);
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
			var result = _memberfavouriteContract.Enable(Id);
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
			var result = _memberfavouriteContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _memberfavouriteContract.MemberFavourites.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _memberfavouriteContract.MemberFavourites.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }










    }
}
