using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.Entities.Notices;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Website.Areas.Notices.Models;
using Whiskey.Web.Helper;
using System.Data.Entity;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class AuthController : Controller
    {

        protected readonly IWorkLogContract _workLogContract;
        protected readonly IWorkLogAttributeContract _workLogAttributeContract;
        protected readonly IStoreSpendStatisticsContract _storeSpendStatisticsContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IArticleItemContract _articleItemContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IPermissionContract _permissionContract;

        public AuthController(
            IStorageContract storageContract, IAdministratorContract adminContract, IModuleContract moduleContract,
            IPermissionContract permissionContract)
        {

            _storageContract = storageContract;
            _adminContract = adminContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;
        }

        enum ModuleName
        {
            信息记录 = 0,
            发布通知 = 1
        }

        [HttpPost]
        public ActionResult HasPower(int? adminId, int? typeId)
        {
            try
            {
                //获取模块的id
                if (!adminId.HasValue || !typeId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }
                ModuleName modueEnum;
                if (!Enum.TryParse(typeId.Value.ToString(), out modueEnum))
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数无效"));
                }
                Module moduleEntity = null;
                switch (modueEnum)
                {
                    case ModuleName.信息记录:
                        moduleEntity = _moduleContract.Modules.FirstOrDefault(m => !m.IsDeleted && m.IsEnabled && m.ModuleName == "运营统计");
                        break;
                    case ModuleName.发布通知:
                        moduleEntity = _moduleContract.Modules.FirstOrDefault(m => !m.IsDeleted && m.IsEnabled && m.ModuleName == "通知管理");

                        break;
                    default:
                        break;
                }
                if (moduleEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "未找到模块信息"));
                }
                var hasPermission = PermissionHelper.HasModulePermission(adminId.Value, moduleEntity.Id, _adminContract,_permissionContract);
                return Json(new OperationResult(OperationResultType.Success, hasPermission ? "yes" : "no"));

            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, "系统错误"));
            }

        }





    }
}