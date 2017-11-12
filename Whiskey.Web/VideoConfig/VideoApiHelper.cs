using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Utility.Data;

namespace Whiskey.Web.VideoConfig
{
    public class VideoApiHelper
    {
        public VideoApiHelper()
        {

        }
        //获取管理员token
        public string accessToken()
        {
            string videoDateTime = System.Web.Configuration.WebConfigurationManager.AppSettings["videoDateTime"];
            string accessToken = System.Web.Configuration.WebConfigurationManager.AppSettings["videoToken"];
            if (DateTime.Now.Subtract(Convert.ToDateTime(videoDateTime)).Days > 1||string.IsNullOrEmpty(accessToken)) {
                System.Web.Configuration.WebConfigurationManager.AppSettings.Set("videoDateTime", DateTime.Now.ToShortDateString());
                string url = "https://openapi.lechange.cn:443/openapi/accessToken";

                string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
                string nonce = VideoData.GetNonce();
                string appId = VideoEquipmentConfig.appId;
                string appSecret = VideoEquipmentConfig.appSecret;
                VideoData vd = new VideoData();
                vd.SetValue("phone", VideoEquipmentConfig.phone);
                string sign = vd.MakeSign(time, nonce, appSecret);
                string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"phone\": \"13811245611\"},\"id\": \"88\"}";
                var result = HttpService.Post(jsonStr, url, 6);
                if (result != "")
                {
                    dynamic json = JToken.Parse(result) as dynamic;
                    if (json.result.code == "0")
                    {
                        accessToken = json.result.data.accessToken;
                        TimeSpan ts = DateTime.Now.AddDays(2).Subtract(DateTime.Now);
                    }
                }
                System.Web.Configuration.WebConfigurationManager.AppSettings.Set("videoToken", accessToken);
            }
            return accessToken;
        }

        //设备是否已绑定
        public string checkDeviceBindOrNot(string deviceId)
        {
            string url = "https://openapi.lechange.cn:443/openapi/checkDeviceBindOrNot";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //获取设备在线状态
        public string deviceOnline(string deviceId)
        {
            string url = "https://openapi.lechange.cn:443/openapi/deviceOnline";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //设备绑定
        public string bindDevice(string deviceId, string code)
        {
            string url = "https://openapi.lechange.cn:443/openapi/bindDevice";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            vd.SetValue("code", "");
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\",\"code\":\"" + code + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //设备解绑
        public string unBindDevice(string deviceId)
        {
            string url = "https://openapi.lechange.cn:443/openapi/unBindDevice";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //单个设备信息获取
        public string bindDeviceInfo(string deviceId)
        {
            string url = "https://openapi.lechange.cn:443/openapi/bindDeviceInfo";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //设备/通道名称修改
        public string modifyDeviceName(string deviceId, string ChannelName)
        {
            string url = "https://openapi.lechange.cn:443/openapi/modifyDeviceName";
            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            vd.SetValue("deviceId", deviceId);
            string token = accessToken();
            vd.SetValue("token", token);
            vd.SetValue("channelId", "");
            vd.SetValue("name", ChannelName);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"deviceId\":\"" + deviceId + "\",\"token\": \"" + token + "\",\"channelId\":\"\",\"name\":\"" + ChannelName + "\"},\"id\": \"88\"}";
            var result = HttpService.Post(jsonStr, url, 6);
            return result;
        }

        //分页获取绑定的设备列表
        public string deviceList(string queryRange)
        {
            string url = "https://openapi.lechange.cn:443/openapi/deviceList";

            string time = Convert.ToInt64((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds).ToString();
            string nonce = VideoData.GetNonce();
            string appId = VideoEquipmentConfig.appId;
            string appSecret = VideoEquipmentConfig.appSecret;
            VideoData vd = new VideoData();
            string token = accessToken();
            vd.SetValue("token", token);
            vd.SetValue("queryRange", queryRange);
            string sign = vd.MakeSign(time, nonce, appSecret);
            string jsonStr = "{\"system\": {\"ver\": \"1.0\",\"sign\": \"" + sign + "\",\"appId\": \"" + appId + "\",\"time\": \"" + time + "\",\"nonce\": \"" + nonce + "\"},\"params\": {\"token\":\"At_6912a599fda24d018947001c74154ba8\",\"queryRange\":\"" + queryRange + "\"},\"id\": \"88\"}";
            string restult = HttpService.Post(jsonStr, url, 6);
            return restult;
        }

    }
}
