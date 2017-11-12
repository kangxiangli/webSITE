using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.RongCloudServer
{
    class InterfaceUrl
    {
        public static String server_addr = "http://api.cn.ronghub.com";

        public static String getTokenUrl = server_addr + "/user/getToken.json";
        public static String refreshUserUrl = server_addr + "/user/refresh.json";
        public static String checkOnlineUrl = server_addr + "/user/checkOnline.json";
        public static String blockUrl = server_addr + "/user/block.json";
        public static String unblockUrl = server_addr + "/user/unblock.json";
        public static String queryBlockUrl = server_addr + "/user/block/query.json";
        public static String joinGroupUrl = server_addr + "/group/join.json";
        public static String quitGroupUrl = server_addr + "/group/quit.json";
        public static String dismissUrl = server_addr + "/group/dismiss.json";
        public static String syncGroupUrl = server_addr + "/group/sync.json";
        public static String sendMsgUrl = server_addr + "/message/private/publish.json";
        public static String sendSysMsgUrl = server_addr + "/message/system/publish.json";
        public static String broadcastUrl = server_addr + "/message/broadcast.json";
        public static String createChatroomUrl = server_addr + "/chatroom/create.json";
        public static String destroyChatroomUrl = server_addr + "/chatroom/destroy.json";
        public static String queryChatroomUrl = server_addr + "/chatroom/query.json";
        /// <summary>
        /// 拉黑
        /// </summary>
        public static String blackUrl = server_addr + "/user/blacklist/add.json";

        /// <summary>
        /// 查询被拉黑的好友
        /// </summary>
        public static String queryBlackUrl = server_addr + "/user/blacklist/query.json";

        /// <summary>
        /// 解除拉黑
        /// </summary>
        public static String unBlackUrl = server_addr + "/user/blacklist/remove.json";

        //public static String getTokenUrl = server_addr + "/user/getToken.xml";
        //public static String joinGroupUrl = server_addr + "/group/join.xml";
        //public static String quitGroupUrl = server_addr + "/group/quit.xml";
        //public static String dismissUrl = server_addr + "/group/dismiss.xml";
        //public static String syncGroupUrl = server_addr + "/group/sync.xml";
        //public static String SendMsgUrl = server_addr + "/message/publish.xml";
        //public static String sendSysMsgUrl = server_addr + "/message/system/publish.xml";
        //public static String broadcastUrl = server_addr + "/message/broadcast.xml";
        //public static String createChatroomUrl = server_addr + "/chatroom/create.xml";
        //public static String destroyChatroomUrl = server_addr + "/chatroom/destroy.xml";
        //public static String queryChatroomUrl = server_addr + "/chatroom/query.xml";

    }
    

}
