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
using System.Web;
using System.IO;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{
    [License(CheckMode.Verify)]
    public class AppVerManageController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(AppVerManageController));

        protected readonly IAppVerManageContract _AppVerManageContract;

        public AppVerManageController(
            IAppVerManageContract _AppVerManageContract
            )
        {
            this._AppVerManageContract = _AppVerManageContract;
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
        public ActionResult Create(AppVerManageDto dto)
        {
            var result = _AppVerManageContract.Insert(dto);
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
        public ActionResult Update(AppVerManageDto dto)
        {
            var result = _AppVerManageContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _AppVerManageContract.Edit(Id);
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
            var result = _AppVerManageContract.View(Id);
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<AppVerManage, bool>> predicate = FilterHelper.GetExpression<AppVerManage>(request.FilterGroup);
            var count = 0;

            var list = (from s in _AppVerManageContract.Entities.Where<AppVerManage, int>(predicate, request.PageCondition, out count)
                        select new
                        {
                            s.Id,
                            s.IsDeleted,
                            s.IsEnabled,
                            s.UpdatedTime,
                            AppType = s.AppType + "",
                            s.Version,
                            s.AccessPath,
                            s.DownloadPath,

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
            var result = _AppVerManageContract.DeleteOrRecovery(true, Id);
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
            var result = _AppVerManageContract.DeleteOrRecovery(false, Id);
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
            var result = _AppVerManageContract.EnableOrDisable(true, Id);
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
            var result = _AppVerManageContract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

		[Log]
        public JsonResult UploadFile(string FileTag, string SavePath, string Version)
        {
            if (FileTag.IsNullOrEmpty() || SavePath.IsNullOrEmpty() || Version.IsNullOrEmpty())
            {
                return Json(OperationHelper.ReturnOperationResult(false, "参数无效"));
            }
            HttpPostedFileBase file = Request.Files[0];
            var SavePathDir = Server.MapPath(SavePath);
            if (!Directory.Exists(SavePathDir))
            {
                Directory.CreateDirectory(SavePathDir);
            }
            var filename = $"{FileTag}{Version}.apk";
            var fullname = Path.Combine(SavePathDir, filename);
            file.SaveAs(fullname);
            return Json(OperationHelper.ReturnOperationResult(true, "上传成功", new { FilePath = FileHelper.Map2VirtualPath(fullname) }));
        }

    }
}

