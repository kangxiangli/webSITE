using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Web.Helper;
using Whiskey.Web.VideoConfig;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Video.Controllers
{
    [License(CheckMode.Verify)]
    public class MonitorPlaybackController : BaseController
    {
        protected readonly IVideoEquipmentContract _videoEquipmentContract;
        protected readonly IVideoJurisdictionContract _videoJurisdictionContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _administratorContract;
        public MonitorPlaybackController(IVideoEquipmentContract videoEquipmentContract,
            IVideoJurisdictionContract videoJurisdictionContract,
            IMemberContract memberContract,
            IStoreContract storeContract,
            IAdministratorContract administratorContract)
        {
            _videoEquipmentContract = videoEquipmentContract;
            _videoJurisdictionContract = videoJurisdictionContract;
            _memberContract = memberContract;
            _storeContract = storeContract;
            _administratorContract = administratorContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult List()
        {

            List<int> s_List = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            List<object> list = new List<object>();
            var datasource = _videoEquipmentContract.VideoEquipments.Where(x => x.IsDeleted == false && x.IsEnabled == true
            && s_List.Contains(x.StoreId)).Select(x => new
            {
                x.snNumber,
                x.VideoName,
                x.Store.StoreName
            });

            foreach (var item in datasource)
            {
                var da = new
                {
                    item.VideoName,
                    item.StoreName,
                    item.snNumber,
                    isOnline = CheckIsOnline(item.snNumber),
                    channelPicUrl = GetChannelPicUrl(item.snNumber)
                };
                list.Add(da);
            }
            return Json(list);
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
            catch (Exception e)
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
            catch (Exception e)
            {


            }
            return channelPicUrl;
        }
    }
}