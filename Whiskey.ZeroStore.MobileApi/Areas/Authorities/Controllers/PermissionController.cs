using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Authorities.Controllers
{
    public class PermissionController : Controller
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PermissionController));

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IPermissionContract _permissionContract;

        public PermissionController(
            IAdministratorContract administratorContract,
            IPermissionContract permissionContract
            )
        {
            this._administratorContract = administratorContract;
            this._permissionContract = permissionContract;
        }
        #endregion

        // GET: Authorities/Permission
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 用户是否拥有某个权限
        /// </summary>
        /// <param name="adminId"></param>
        /// <param name="permissionId"></param>
        /// <returns></returns>
        //[HttpPost]
        public JsonResult HasPermission(int adminId, int moduleId, string identifier)
        {
            if (!_administratorContract.CheckExists(a => a.Id == adminId))
            {
                return Json(new OperationResult(OperationResultType.Error, "该用户不存在"), JsonRequestBehavior.AllowGet);
            }

            var beflag = PermissionHelper.GetOneUserPermission(adminId, _administratorContract, _permissionContract).Where(s => s.IsEnabled && !s.IsDeleted && s.ModuleId == moduleId && s.Identifier == identifier).Any();
            CacheAccess.ClearPermissionCache();
            if (!beflag)
            {
                return Json(new OperationResult(OperationResultType.Error, "暂无权限"), JsonRequestBehavior.AllowGet);
            }
            return Json(new OperationResult(OperationResultType.Success), JsonRequestBehavior.AllowGet);
        }
    }
}