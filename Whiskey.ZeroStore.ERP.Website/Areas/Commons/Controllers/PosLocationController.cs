using System;
using System.IO;
using System.Web;
using Whiskey.Web.Helper;
using Antlr3.ST;
using Antlr3.ST.Language;
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
using Whiskey.ZeroStore.ERP.Website.Hubs;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    [License(CheckMode.Verify)]
    public class PosLocationController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PosLocationController));

        protected readonly IPosLocationContract _PosLocationContract;
        protected readonly IStoreContract _StoreContract;

        public PosLocationController(
            IPosLocationContract _PosLocationContract,
            IStoreContract _StoreContract
            )
        {
            this._PosLocationContract = _PosLocationContract;
            this._StoreContract = _StoreContract;
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
        public ActionResult Create(PosLocationDto dto)
        {
            var result = _PosLocationContract.Insert(dto);
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
        public ActionResult Update(PosLocationDto dto)
        {
            var result = _PosLocationContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _PosLocationContract.Edit(Id);
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
            var result = _PosLocationContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<PosLocation, bool>> predicate = FilterHelper.GetExpression<PosLocation>(request.FilterGroup);
            var count = 0;
            var querystore = _StoreContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted);
            var list = (from s in _PosLocationContract.Entities.Where<PosLocation, int>(predicate, request.PageCondition, out count)
                        let storeName = querystore.Where(w => w.IMEI == s.IMEI).Select(ss => ss.StoreName).FirstOrDefault()
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.CreatedTime,
                            OperatorName = s.Operator.Member.RealName,
                            s.IMEI,
                            s.Longitude,
                            s.Latitude,
                            s.PrevLongitude,
                            s.PrevLatitude,
                            s.PrevUpdatedTime,
                            s.UpdatedTime,
                            s.Address,
                            s.PrevAddress,
                            StoreName = storeName,

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
            var result = _PosLocationContract.DeleteOrRecovery(true, Id);
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
            var result = _PosLocationContract.DeleteOrRecovery(false, Id);
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
            var result = _PosLocationContract.EnableOrDisable(true, Id);
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
            var result = _PosLocationContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
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
            Expression<Func<PosLocation, bool>> predicate = FilterHelper.GetExpression<PosLocation>(request.FilterGroup);
            var querystore = _StoreContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted);
			var query = _PosLocationContract.Entities.Where(predicate);
			var list = (from s in query
                        let storeName = querystore.Where(w => w.IMEI == s.IMEI).Select(ss => ss.StoreName).FirstOrDefault()
						select new
                        {
							s.UpdatedTime,
							OperatorName = s.Operator.Member.RealName,
                            s.IMEI,
                            s.Longitude,
                            s.Latitude,
                            s.PrevLongitude,
                            s.PrevLatitude,
                            s.PrevUpdatedTime,
                            s.Address,
                            s.PrevAddress,
                            StoreName = storeName,

                        }).ToList();
			var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "POS机定位");
        }

        public int RefreshDevice()
        {
            return DeviceHub.FlushAllDeviceLocation();
        }

    }
}

