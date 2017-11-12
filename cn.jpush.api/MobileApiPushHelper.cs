using Whiskey.jpush.api.common;
using Whiskey.jpush.api.common.resp;
using Whiskey.jpush.api.push.mode;
using Whiskey.jpush.api.push.notification;
using Whiskey.jpush.api.schedule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Whiskey.Utility.Helper;

namespace Whiskey.jpush.api
{
    /// <summary>
    /// 零时尚APP端推送
    /// </summary>
    public static class MobileAPIPushHelper
    {
        /// <summary>
        /// 推送环境配置,默认使用正式环境,开发环境下设置为false
        /// </summary>
        private static readonly bool APNS_PRODUCTION = true;
        private const string APP_KEY = "21ba9e2f6f03bf2a3cd42c14";
        private const string MASTER_SECRET = "3bae387d358d06274221f418";
        private static JPushClient _pushClient;

        

        static MobileAPIPushHelper()
        {
            if (ConfigurationHelper.IsDevelopment())
            {
                APNS_PRODUCTION = false;
            }
            _pushClient = new JPushClient(APP_KEY, MASTER_SECRET);
        }

        /// <summary>
        /// pc端登录确认推送
        /// </summary>
        /// <param name="registrationId"></param>
        public static bool PushConfirmLogin(string registrationId)
        {
            var payload = JsonConvert.SerializeObject(new
            {
                platform = "all",
                audience = new {
                    registration_id  = new HashSet<string> { registrationId}
                },
                notification = new
                {
                    alert = "PC 登录确认",
                    ios = new
                    {
                        alert = "PC 登录确认",
                        sound = "default",
                        extras = new { context = "CONFIRM_LOGIN" }
                    },
                    android = new
                    {
                        alert = "PC 登录确认",
                        title = "PC 登录确认",
                        extras = new { context = "CONFIRM_LOGIN"}
                    }
                },
                options = new
                {
                    time_to_live = 300,
                    apns_production = APNS_PRODUCTION
                }
            });
            var res = _pushClient.SendPush(payload);
            return res.isResultOK();
        }


    }
}
