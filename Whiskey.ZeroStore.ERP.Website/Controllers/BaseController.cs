using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading;
using Whiskey.Utility.Helper;
using System.Text;
using System.Web.Script.Serialization;
using System.Web;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{

    //[AuthValid]
    //[Authori]
    public class BaseController : Controller
    {
        /// <summary>
        /// WebUrl地址，来自web.config
        /// </summary>
        protected readonly string WebUrl = ConfigurationHelper.WebUrl;
        /// <summary>
        /// ApiUrl地址，来自web.config
        /// </summary>
        protected readonly string ApiUrl = ConfigurationHelper.ApiUrl;

        /// <summary>
        /// 发送通知Action，通知指定的人
        /// </summary>
        protected Action<List<int>> sendNotificationAction = (adminids) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendNotification(adminids.ToArray()); }); } catch { } };
        /// <summary>
        /// 发送通知Action，通知所有人
        /// </summary>
        protected Action sendNotificationAllAction = () => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendNotificationAll(); }); } catch { } };
        /// <summary>
        /// 发送消息Action，通知指定的人
        /// </summary>
        protected Action<List<int>> sendMessageAction = (adminids) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendMessage(adminids.ToArray()); }); } catch { } };
        /// <summary>
        /// 发送消息Action，通知所有人
        /// </summary>
        protected Action sendMessageAllAction = () => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendMessageAll(); }); } catch { } };
        /// <summary>
        /// 更新菜单Badge方法,参数1操作人2要变化的数值可以为负数3菜单badgeTag类名称4为true时在原来的基础上变化否则直接取当前值
        /// </summary>
        protected Action<int, int, string, bool> updateBadgeAction = (adminId, changeCount, badgeTag, autoCalc) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.UpdateBadgeCount(adminId, changeCount, badgeTag, autoCalc); }); } catch { } };

        /// <summary>
        /// 发送弹窗Loading，通知指定人,bool=true关闭弹窗,int[]为通知人Id
        /// </summary>
        protected Action<bool, string, int[]> sendPopLoadingAction = (isclose, content, adminIds) => { try { ThreadPool.QueueUserWorkItem((obj) => { NotificationHub.SendPopLoading(isclose, content, adminIds); }); } catch { } };
        /// <summary>
        /// 创建 JsonResult 对象，该对象使用指定 JSON 请求行为将指定对象序列化为 JavaScript 对象表示法 (JSON) 格式，突破maxJsonLength最大限制
        /// </summary>
        /// <param name="data">要序列化的 JavaScript 对象</param>
        /// <param name="behavior">JSON 请求行为</param>
        /// <returns>将指定对象序列化为 JSON 格式的结果对象</returns>
        protected JsonResult JsonLarge(object data, JsonRequestBehavior behavior)
        {
            return new JsonResult
            {
                MaxJsonLength = int.MaxValue,
                JsonRequestBehavior = behavior,
                Data = data,
            };
        }

        protected FileResult FileExcel(Antlr3.ST.StringTemplate st, string fileName)
        {
            Response.AppendCookie(new HttpCookie("fileDownload", "true") { Path = "/" });
            st.SetAttribute("FileTitle", fileName);
            var str = st.ToString();
            byte[] fileContents = Encoding.UTF8.GetBytes(str);
            return File(fileContents, "application/vnd.ms-excel", $"{fileName}.xls");
        }
    }
}