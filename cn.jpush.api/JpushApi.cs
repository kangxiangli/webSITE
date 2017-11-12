using Whiskey.jpush.api.push.mode;
using Whiskey.jpush.api.schedule;
using System;

namespace Whiskey.jpush.api
{
    public enum JpushApiPlatform
    {
        All,
        Android,
        Ios,
        WindowsPhone,
        Android_Ios,
        Android_WindowsPhone,
        Ios_WindowsPhone
    }
    public class JpushApi
    {
        public static string app_key = "5d4e70f9b30a0517a8e7d1c1";
        public static string master_secret = "c9cb4a9bfd3b3ff9e566513c";
        public static string app_key_0fashion = "21ba9e2f6f03bf2a3cd42c14";
        public static string master_secret_0fashion = "3bae387d358d06274221f418";

        public delegate bool PushInformationHandler(DateTime? sendTime, JpushApiPlatform platform, Audience audience, string content, string title = null, string msg_content = null, bool contentToTitle = false, string Flag = null);

        public static PushInformationHandler FASHION = new JpushApiBase(app_key_0fashion, master_secret_0fashion).PushInformation;
        public static PushInformationHandler XIAODIE = new JpushApiBase(app_key, master_secret).PushInformation;

    }

    public class JpushApiBase
    {
        //run the DeviceApiExample first,it will add mobile,tags,alias to the device:
        //首先运行DeviceApiExample，为设备添加手机号码，标签别名，再运行JPushApiExample,ScheduleApiExample，步骤如下：
        //1.设置Whiskey.jpush.api.example为启动项
        //2.在Whiskey.jpush.api.example项目，右键选择属性，然后选择应用程序，最后在启动对象下拉框中选择DeviceApiExample
        //3.按照2的步骤设置，运行JPushApiExample,ScheduleApiExample.

        string TITLE = "this is my msg";
        string ALERT = "saaa";
        string MSG_CONTENT = "wwww";
        string REGISTRATION_ID = "0900e8d85ef";
        string SMSMESSAGE = "Test from C# v3 sdk - SMSMESSAGE";
        int DELAY_TIME = 1;
        string TAG = "tag_api";
        string app_key = "5d4e70f9b30a0517a8e7d1c1";//小蝶办公
        string master_secret = "c9cb4a9bfd3b3ff9e566513c";//小蝶办公

        public JpushApiBase(string app_key, string master_secret)
        {
            this.app_key = app_key;
            this.master_secret = master_secret;
        }
        /// <summary>
        /// 推送
        /// </summary>
        /// <param name="sendTime">推送时间</param>
        /// <param name="platform"></param>
        /// <param name="audience"></param>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="msg_content"></param>
        /// <param name="contentToTitle">将内容替换为Title【只作用于IOS】</param>
        /// <param name="Flag">APP内部跳转页面使用</param>
        /// <returns></returns>
        public bool PushInformation(DateTime? sendTime, JpushApiPlatform platform, Audience audience, string content, string title = null, string msg_content = null, bool contentToTitle = false, string Flag = null)
        {
            PushPayload payload = new PushPayload();
            Notification notification = payload.getNotificationAll(content, title, contentToTitle, Flag);
            payload.audience = audience;
            payload.message = Message.content(msg_content);
            switch (platform)
            {
                case JpushApiPlatform.All:
                    payload.platform = Platform.all();
                    payload.notification = notification;
                    break;
                case JpushApiPlatform.Android:
                    payload.platform = Platform.android();
                    payload.notification.AndroidNotification = notification.AndroidNotification;
                    break;
                case JpushApiPlatform.Ios:
                    payload.platform = Platform.ios();
                    payload.notification = Notification.ios(content);
                    break;
                case JpushApiPlatform.WindowsPhone:
                    payload.platform = Platform.winphone();
                    payload.notification.WinphoneNotification = notification.WinphoneNotification;
                    break;
                case JpushApiPlatform.Android_Ios:
                    payload.platform = Platform.android_ios();
                    payload.notification.AndroidNotification = notification.AndroidNotification;
                    payload.notification.IosNotification = notification.IosNotification;
                    break;
                case JpushApiPlatform.Android_WindowsPhone:
                    payload.platform = Platform.android_winphone();
                    payload.notification.AndroidNotification = notification.AndroidNotification;
                    payload.notification.WinphoneNotification = notification.WinphoneNotification;
                    break;
                case JpushApiPlatform.Ios_WindowsPhone:
                    payload.platform = Platform.ios_winphone();
                    payload.notification.IosNotification = notification.IosNotification;
                    payload.notification.WinphoneNotification = notification.WinphoneNotification;
                    break;
                default:
                    break;
            }
            payload.notification = payload.notification.Check();
            try
            {
                if (sendTime.HasValue)
                {
                    #region 修正时间

                    var now = DateTime.Now.AddSeconds(3);//延迟3秒，否则推送时间晚于当前时间，导致不正确
                    var sendtime = sendTime.Value;
                    sendtime = sendtime < now ? now : sendtime;//发送的时间不能早于当前时间，否则没有意义

                    #endregion

                    ScheduleClient clientSched = new ScheduleClient(app_key, master_secret);
                    TriggerPayload triggerConstructor = new TriggerPayload(sendtime.ToString("yyyy-MM-dd HH:mm:ss"));//一次性发送
                    SchedulePayload schedulepayloadperiodical = new SchedulePayload(title + sendtime.ToString("yyyyMMddHHmmss"), true, triggerConstructor, payload);
                    var result = clientSched.sendSchedule(schedulepayloadperiodical);
                    return result.isResultOK();
                }
                else
                {
                    JPushClient client = new JPushClient(app_key, master_secret);
                    var result = client.SendPush(payload);
                    return result.isResultOK();

                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
