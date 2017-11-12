using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Results;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class ApiStoreController : BaseController
    {
        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");

        protected readonly IStoreContract _storeContract;
        protected readonly IJobPositionContract _jobPositionContract;
        protected readonly IAdministratorContract _adminContract;
        public ApiStoreController(
            IStoreContract _storeContract
            , IJobPositionContract _jobPositionContract
            , IAdministratorContract _adminContract

            )
        {
            this._storeContract = _storeContract;
            this._jobPositionContract = _jobPositionContract;
            this._adminContract = _adminContract;
        }


        public JsonResult GetStoreInfo(int StoreId)
        {
            var modStore = _storeContract.View(StoreId);

            var jobPositionId = _jobPositionContract.JobPositions.Where(x => !x.IsDeleted && x.IsEnabled && x.DepartmentId == modStore.DepartmentId && x.IsLeader).Select(x => x.Id).FirstOrDefault();
            var modAdminLeader = _adminContract.Administrators.Where(x => !x.IsDeleted && x.IsEnabled && x.JobPositionId == jobPositionId).Select(x => new { x.Member.MemberName, x.Member.UserPhoto }).FirstOrDefault();

            var rdata = new
            {
                modStore.StoreName,
                StorePhoto = modStore.StorePhoto.IsNotNullAndEmpty() ? strWebUrl + modStore.StorePhoto : "",
                LeaderName = modAdminLeader?.MemberName ?? "",
                LeaderPhoto = modAdminLeader?.UserPhoto.IsNotNullAndEmpty() == true ? strWebUrl + modAdminLeader.UserPhoto : ""
            };
            return Json(rdata, JsonRequestBehavior.AllowGet);
        }
    }
}