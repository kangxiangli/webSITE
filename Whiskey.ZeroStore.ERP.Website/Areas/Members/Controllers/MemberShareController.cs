using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberShareController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberShareController));

        protected readonly IMemberShareContract _MemberShareContract;

        public MemberShareController(
            IMemberShareContract _MemberShareContract
            )
        {
            this._MemberShareContract = _MemberShareContract;
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

        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(MemberShareDto dto)
        {
            var result = _MemberShareContract.Insert(dto);
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
        public ActionResult Update(MemberShareDto dto)
        {
            var result = _MemberShareContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _MemberShareContract.Edit(Id);
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
            var result = _MemberShareContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberShare, bool>> predicate = FilterHelper.GetExpression<MemberShare>(request.FilterGroup);
            var count = 0;

            var list = (from s in _MemberShareContract.Entities.Where<MemberShare, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.CreatedTime,
							s.Name,

                        }).ToList();
            var data = new GridData<object>(list, count, request.RequestInfo);

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
            var result = _MemberShareContract.DeleteOrRecovery(true, Id);
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
            var result = _MemberShareContract.DeleteOrRecovery(false, Id);
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
            var result = _MemberShareContract.EnableOrDisable(true, Id);
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
            var result = _MemberShareContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

    }
}

