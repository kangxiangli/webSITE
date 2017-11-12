using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.VideoConfig;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.MobileApi.Areas.Videos.Controllers
{
    public class VideoDataController : Controller
    {
        protected readonly IVideoEquipmentContract _videoEquipmentContract;
        protected readonly IVideoJurisdictionContract _videoJurisdictionContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IPermissionContract _permissionContract;
        public VideoDataController(IVideoEquipmentContract videoEquipmentContract,
    IVideoJurisdictionContract videoJurisdictionContract,
    IMemberContract memberContract,
    IStoreContract storeContract,
    IAdministratorContract administratorContract,
    IPermissionContract _permissionContract)
        {
            _videoEquipmentContract = videoEquipmentContract;
            _videoJurisdictionContract = videoJurisdictionContract;
            _memberContract = memberContract;
            _storeContract = storeContract;
            _administratorContract = administratorContract;
            this._permissionContract = _permissionContract;
        }
        public JsonResult List(int? adminId, int pageSize, int pageIndex)
        {
            pageIndex = pageIndex - 1;
            bool restult = PermissionHelper.HasModulePermission(adminId.Value, 2141, _administratorContract, _permissionContract);
            List<object> list = new List<object>();
            if (restult)
            {
                var s_List = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId ?? adminId ?? 0);
                var datasource = _videoEquipmentContract.VideoEquipments.Where(x => x.IsDeleted == false && x.IsEnabled == true
                && s_List.Contains(x.StoreId))
                .OrderBy(x => x.UpdatedTime).Skip(pageIndex * pageSize).Take(pageSize)
                .Select(x => new
                {
                    x.snNumber,
                    x.VideoName,
                    x.Store.StoreName
                }).ToList();
                VideoApiHelper api = new VideoApiHelper();
                string accessToken = api.accessToken();
                foreach (var item in datasource)
                {
                    var da = new
                    {
                        item.VideoName,
                        item.StoreName,
                        item.snNumber,
                        isOnline = CheckIsOnline(item.snNumber),
                        channelPicUrl = GetChannelPicUrl(item.snNumber),
                        accessToken = accessToken
                    };
                    list.Add(da);
                }
            }
            return Json(new OperationResult(OperationResultType.Success, "获取成功！", list), JsonRequestBehavior.AllowGet);
        }
        public string CheckIsOnline(string deviceId)
        {
            string isOnline = "";
            VideoApiHelper api = new VideoApiHelper();
            string result = api.deviceOnline(deviceId);
            try
            {
                dynamic json = JToken.Parse(result) as dynamic;
                if (json.result.code == "0")
                {
                    string isOnlineDesc = json.result.data.onLine;
                    if (isOnlineDesc == "0")
                    {
                        isOnline = "离线";
                    }
                    else if (isOnlineDesc == "1")
                    {
                        isOnline = "在线";
                    }
                }
            }
            catch (Exception)
            {
                isOnline = "未知";

            }
            return isOnline;
        }

        public string GetChannelPicUrl(string deviceId)
        {
            //bindDeviceInfo
            VideoApiHelper api = new VideoApiHelper();
            string result = api.bindDeviceInfo(deviceId);
            string channelPicUrl = string.Empty;
            try
            {
                dynamic json = JToken.Parse(result) as dynamic;
                if (json.result.code == "0")
                {
                    channelPicUrl = json.result.data.channels[0].channelPicUrl;
                }
            }
            catch (Exception)
            {


            }
            return channelPicUrl;
        }

        public JsonResult HasModulePermission(int adminId, int moduleId)
        {
            bool restult = PermissionHelper.HasModulePermission(adminId, moduleId, _administratorContract, _permissionContract);

            return Json(restult, JsonRequestBehavior.AllowGet);
        }
    }
}