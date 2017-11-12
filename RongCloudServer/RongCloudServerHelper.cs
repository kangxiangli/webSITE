using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Web;
using Newtonsoft.Json;

namespace Whiskey.RongCloudServer
{
    public class RongCloudServerHelper
    {
        private const string APP_KEY = "6tnym1brn6aw7";
        private const string APP_SECRET = "SFoX1v04P4UTo";
        /**
         * 构建请求参数
         */
        private static String buildQueryStr(Dictionary<String, String> dicList)
        {
            String postStr = "";

            foreach (var item in dicList)
            {
                postStr += item.Key + "=" + HttpUtility.UrlEncode(item.Value, Encoding.UTF8) + "&";
            }
            if (!string.IsNullOrEmpty(postStr))
            {
                postStr = postStr.Substring(0, postStr.LastIndexOf('&'));
            }
            return postStr;
        }

        private static String buildParamStr(String[] arrParams)
        {
            String postStr = "";

            for (int i = 0; i < arrParams.Length; i++)
            {
                if (0 == i)
                {
                    postStr = "chatroomId=" + HttpUtility.UrlDecode(arrParams[0], Encoding.UTF8);
                }
                else
                {
                    postStr = postStr + "&" + "chatroomId=" + HttpUtility.UrlEncode(arrParams[i], Encoding.UTF8);
                }
            }
            return postStr;
        }

        /**
         * 获取 token
         */
        public static GetTokenRes GetToken(String userId, String name, String portraitUri)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("name", name);
            dicList.Add("portraitUri", portraitUri);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.getTokenUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<GetTokenRes>(res);

        }

        /**
         * 刷新用户信息
         */
        public static OptRes RefreshUser(String userId, String name, String portraitUri)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("name", name);
            dicList.Add("portraitUri", portraitUri);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.refreshUserUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<OptRes>(res);

        }

        /**
          * 检查用户在线状态
          */
        public static CheckOnlineRes CheckOnline(String userId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.checkOnlineUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<CheckOnlineRes>(res);


        }

        /**
          * 用户封号
          */
        public static OptRes Block(String userId, string minute)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("minute", minute);
            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.blockUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<OptRes>(res);
        }

        /**
          * 用户解封
          */
        public static OptRes Unblock(String userId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.unblockUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<OptRes>(res);
        }


        /**
         *查询被禁用的号 
         */
        public static QueryBlockRes QueryBlock()
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();


            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.queryBlockUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<QueryBlockRes>(res);
        }


        /**
          * 拉黑用户
          */
        public static OptRes Black(String userId, string[] blackUserIds)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            for (int i = 0; i < blackUserIds.Length; i++)
            {
                dicList.Add("blackUserId", blackUserIds[i]);
            }
            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.blackUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<OptRes>(res);
        }

        /**
         * 解除拉黑用户
         */
        public static OptRes UnBlack(String userId, string blackUserId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);

            dicList.Add("blackUserId", blackUserId);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.unBlackUrl, postStr);

            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<OptRes>(res);
        }

        /**
         * 查询已拉黑用户
         */
        public static QueryBlackRes QueryBlack(String userId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            String postStr = buildQueryStr(dicList);
            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.queryBlackUrl, postStr);
            var res = client.ExecutePost();
            return JsonConvert.DeserializeObject<QueryBlackRes>(res);
        }



        /**
         * 加入 群组
         */
        public static String JoinGroup(String userId, String groupId, String groupName)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("groupId", groupId);
            dicList.Add("groupName", groupName);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.joinGroupUrl, postStr);

            return client.ExecutePost();
        }

        /**
         * 退出 群组
         */
        public static String QuitGroup(String userId, String groupId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("groupId", groupId);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.quitGroupUrl, postStr);

            return client.ExecutePost();
        }

        /**
         * 解散 群组
         */
        public static String DismissGroup(String userId, String groupId)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("userId", userId);
            dicList.Add("groupId", groupId);

            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.dismissUrl, postStr);

            return client.ExecutePost();

        }

        /**
         * 同步群组
         */
        public static String syncGroup(String userId, String[] groupId, String[] groupName)
        {

            String postStr = "userId=" + userId + "&";
            String id, name;

            for (int i = 0; i < groupId.Length; i++)
            {
                id = HttpUtility.UrlEncode(groupId[i], Encoding.UTF8);
                name = HttpUtility.UrlEncode(groupName[i], Encoding.UTF8);
                postStr += "group[" + id + "]=" + name + "&";
            }

            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.syncGroupUrl, postStr);

            return client.ExecutePost();
        }


        /// <summary>
        /// 发送二人消息
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appSecret"></param>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        /// <param name="objectName"></param>
        /// <param name="content">RC:TxtMsg消息格式{"content":"hello"}  RC:ImgMsg消息格式{"content":"ergaqreg", "imageKey":"http://www.demo.com/1.jpg"}  RC:VcMsg消息格式{"content":"ergaqreg","duration":3}</param>
        /// <returns></returns>
        public static String PublishMessage(String fromUserId, String toUserId, String objectName, String content, string user = null)
        {
            //此数据结构不适用多个toUserId情况,请注意
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("fromUserId", fromUserId);
            dicList.Add("toUserId", toUserId);
            dicList.Add("objectName", objectName);
            dicList.Add("content", content);
            if (!string.IsNullOrEmpty(user))
            {
                dicList.Add("user", user);
            }
            String postStr = buildQueryStr(dicList);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.sendMsgUrl, postStr);

            return client.ExecutePost();
        }

        public static OptRes publishPrivate(String fromUserId, String[] toUserId, string message, string objectName, String pushContent, String pushData, String count, int verifyBlacklist, int isPersisted, int isCounted)
        {

            if (fromUserId == null)
            {
                throw new ArgumentNullException("Paramer 'fromUserId' is required");
            }

            if (toUserId == null)
            {
                throw new ArgumentNullException("Paramer 'toUserId' is required");
            }



            String postStr = "";
            postStr += "fromUserId=" + HttpUtility.UrlEncode(fromUserId == null ? "" : fromUserId, Encoding.UTF8) + "&";
            for (int i = 0; i < toUserId.Length; i++)
            {
                String child = toUserId[i];
                postStr += "toUserId=" + HttpUtility.UrlEncode(child, Encoding.UTF8) + "&";
            }

            postStr += "objectName=" + HttpUtility.UrlEncode(objectName, Encoding.UTF8) + "&";
            postStr += "content=" + HttpUtility.UrlEncode(message, Encoding.UTF8) + "&";
            postStr += "pushContent=" + HttpUtility.UrlEncode(pushContent == null ? "" : pushContent, Encoding.UTF8) + "&";
            postStr += "pushData=" + HttpUtility.UrlEncode(pushData == null ? "" : pushData, Encoding.UTF8) + "&";
            postStr += "count=" + HttpUtility.UrlEncode(count == null ? "" : count, Encoding.UTF8) + "&";
            postStr += "verifyBlacklist=" + HttpUtility.UrlEncode(Convert.ToString(verifyBlacklist) == null ? "" : Convert.ToString(verifyBlacklist), Encoding.UTF8) + "&";
            postStr += "isPersisted=" + HttpUtility.UrlEncode(Convert.ToString(isPersisted) == null ? "" : Convert.ToString(isPersisted), Encoding.UTF8) + "&";
            postStr += "isCounted=" + HttpUtility.UrlEncode(Convert.ToString(isCounted) == null ? "" : Convert.ToString(isCounted), Encoding.UTF8) + "&";
            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.sendMsgUrl, postStr);
            return JsonConvert.DeserializeObject<OptRes>(client.ExecutePost());

        }


        public static OptRes publishSystem(String fromUserId, String[] toUserId, string message, string objectName, String pushContent, String pushData, String count, int verifyBlacklist, int isPersisted, int isCounted)
        {

            if (fromUserId == null)
            {
                throw new ArgumentNullException("Paramer 'fromUserId' is required");
            }

            if (toUserId == null)
            {
                throw new ArgumentNullException("Paramer 'toUserId' is required");
            }



            String postStr = "";
            postStr += "fromUserId=" + HttpUtility.UrlEncode(fromUserId == null ? "" : fromUserId, Encoding.UTF8) + "&";
            for (int i = 0; i < toUserId.Length; i++)
            {
                String child = toUserId[i];
                postStr += "toUserId=" + HttpUtility.UrlEncode(child, Encoding.UTF8) + "&";
            }

            postStr += "objectName=" + HttpUtility.UrlEncode(objectName, Encoding.UTF8) + "&";
            postStr += "content=" + HttpUtility.UrlEncode(message, Encoding.UTF8) + "&";
            postStr += "pushContent=" + HttpUtility.UrlEncode(pushContent == null ? "" : pushContent, Encoding.UTF8) + "&";
            postStr += "pushData=" + HttpUtility.UrlEncode(pushData == null ? "" : pushData, Encoding.UTF8) + "&";
            //postStr += "count=" + HttpUtility.UrlEncode(count == null ? "" : count, Encoding.UTF8) + "&";
            //postStr += "verifyBlacklist=" + HttpUtility.UrlEncode(Convert.ToString(verifyBlacklist) == null ? "" : Convert.ToString(verifyBlacklist), Encoding.UTF8) + "&";
            postStr += "isPersisted=" + HttpUtility.UrlEncode(Convert.ToString(isPersisted) == null ? "" : Convert.ToString(isPersisted), Encoding.UTF8) + "&";
            postStr += "isCounted=" + HttpUtility.UrlEncode(Convert.ToString(isCounted) == null ? "" : Convert.ToString(isCounted), Encoding.UTF8) + "&";
            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.sendSysMsgUrl, postStr);
            return JsonConvert.DeserializeObject<OptRes>(client.ExecutePost());

        }

        /// <summary>
        /// 广播消息暂时未开放
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appSecret"></param>
        /// <param name="fromUserId"></param>
        /// <param name="objectName"></param>
        /// <param name="content">RC:TxtMsg消息格式{"content":"hello"}  RC:ImgMsg消息格式{"content":"ergaqreg", "imageKey":"http://www.demo.com/1.jpg"}  RC:VcMsg消息格式{"content":"ergaqreg","duration":3}</param>
        /// <returns></returns>
        public static String BroadcastMessage(String fromUserId, String objectName, String content)
        {
            Dictionary<String, String> dicList = new Dictionary<String, String>();
            dicList.Add("content", content);
            dicList.Add("fromUserId", fromUserId);
            dicList.Add("objectName", objectName);
            dicList.Add("pushContent", "");
            dicList.Add("pushData", "");

            String postStr = buildQueryStr(dicList);
            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.broadcastUrl, postStr);

            return client.ExecutePost();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appSecret"></param>
        /// <param name="chatroomInfo">chatroom[id10001]=name1001</param>
        /// <returns></returns>
        public static String CreateChatroom(String[] chatroomId, String[] chatroomName)
        {
            String postStr = null;

            String id, name;

            for (int i = 0; i < chatroomId.Length; i++)
            {
                id = HttpUtility.UrlEncode(chatroomId[i], Encoding.UTF8);
                name = HttpUtility.UrlEncode(chatroomName[i], Encoding.UTF8);
                postStr += "chatroom[" + id + "]=" + name + "&";
            }

            postStr = postStr.Substring(0, postStr.LastIndexOf('&'));

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.createChatroomUrl, postStr);

            return client.ExecutePost();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appkey"></param>
        /// <param name="appSecret"></param>
        /// <param name="chatroomIdInfo">chatroomId=id1001</param>
        /// <returns></returns>
        public static String DestroyChatroom(String[] chatroomIdInfo)
        {
            String postStr = null;

            postStr = buildParamStr(chatroomIdInfo);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.destroyChatroomUrl, postStr);

            return client.ExecutePost();
        }
        public static String queryChatroom(String[] chatroomId)
        {
            String postStr = null;

            postStr = buildParamStr(chatroomId);

            RongHttpClient client = new RongHttpClient(APP_KEY, APP_SECRET, InterfaceUrl.queryChatroomUrl, postStr);

            return client.ExecutePost();
        }
    }
}
