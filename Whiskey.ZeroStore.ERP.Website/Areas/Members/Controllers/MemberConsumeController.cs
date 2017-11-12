




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
    public class MemberConsumeController : BaseController
    {

        #region 初始化操作对象

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberConsumeController));

        protected readonly IMemberConsumeContract _memberconsumeContract;

        protected readonly IOrderContract _orderContract;


        public MemberConsumeController(IMemberConsumeContract memberconsumeContract,
            IOrderContract orderContract)
        {
            _memberconsumeContract = memberconsumeContract;
            _orderContract = orderContract;
        }
        #endregion

        #region 初始化页面

        /// <summary>
        /// 视图数据
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
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            try
            {
                GridRequest request = new GridRequest(Request);
                Expression<Func<MemberConsume, bool>> predicate = FilterHelper.GetExpression<MemberConsume>(request.FilterGroup);
                var count = 0;
                var dataList = _memberconsumeContract.MemberConsumes
                    .OrderByDescending(c => c.CreatedTime)
                    .Where<MemberConsume, int>(predicate, request.PageCondition, out count)
                .ToList();
                var list = dataList.Select(m => new
                {
                    OrderType = m.OrderType.HasValue ? m.OrderType.ToString() : string.Empty,
                    ConsumeContext = m.ConsumeContext.HasValue ? m.ConsumeContext.ToString() : string.Empty,
                    m.MemberId,
                    m.Member.RealName,
                    m.Member.MobilePhone,
                    m.StoreId,
                    m.Store.StoreName,
                    m.BalanceConsume,
                    m.ScoreConsume,
                    m.RelatedOrderNumber,
                    ConsumeType = m.ConsumeType.ToDescription(),
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();

                var data = new GridData<object>(list, count, request.RequestInfo);

                return Json(data, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));

            }

        }
        #endregion


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
        public ActionResult Create(MemberConsumeDto dto)
        {
            var result = _memberconsumeContract.Insert(dto);
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
        public ActionResult Update(MemberConsumeDto dto)
        {
            var result = _memberconsumeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _memberconsumeContract.Edit(Id);
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
            var result = _memberconsumeContract.View(Id);
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
            var result = _memberconsumeContract.Remove(Id);
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
            var result = _memberconsumeContract.Delete(Id);
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
            var result = _memberconsumeContract.Recovery(Id);
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
            var result = _memberconsumeContract.Enable(Id);
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
            var result = _memberconsumeContract.Disable(Id);
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
            var list = _memberconsumeContract.MemberConsumes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
        public ActionResult Export()
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberConsume, bool>> predicate = FilterHelper.GetExpression<MemberConsume>(request.FilterGroup);
            var dataList = _memberconsumeContract.MemberConsumes.OrderByDescending(c => c.CreatedTime).Where(predicate).ToList();
            var list = dataList.Select(m => new
            {
                OrderType = m.OrderType.HasValue ? m.OrderType.ToString() : string.Empty,
                ConsumeContext = m.ConsumeContext.HasValue ? m.ConsumeContext.ToString() : string.Empty,
                m.MemberId,
                m.Member.RealName,
                m.Member.MobilePhone,
                m.StoreId,
                m.Store.StoreName,
                m.BalanceConsume,
                m.ScoreConsume,
                m.RelatedOrderNumber,
                ConsumeType = m.ConsumeType.ToDescription(),
                m.UpdatedTime,
                m.Operator.Member.MemberName,
            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "消费记录管理");
        }

    }
}
