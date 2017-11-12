using System;
using System.Web;
using System.Web.Security;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Helper;

namespace Whiskey.Web.Helper
{

    public static class AuthorityHelper
    {

        public static ILogger _Logger = LogManager.GetLogger(typeof(AuthorityHelper));
        public enum TicketType
        {
            Id = 0,
            AdminName = 1,
            RealName=2
        }

        public static bool IsVerified {
            get {
                return HttpContext.Current.User.Identity.IsAuthenticated? true : false;
            }
        }

        public static int? OperatorId
        {
            get
            {
                return GetId();
            }
        }

        public static string AdminName
        {
            get
            {
                var name = GetInfo(TicketType.AdminName);
                return name.Length > 0 ? name : "系统用户";
            }
        }

        public static string RealName
        {
            get
            {
                var name = GetInfo(TicketType.RealName);
                return name.Length > 0 ? name : "未知";
            }
        }

        public static bool IsAdministrator
        {
            get
            {
                var isAdmin = GetInfo(TicketType.AdminName);
                return isAdmin.ToLower() == "admin".ToLower() ? true : false;
            }
        }


        public static int? GetId() {
            var id = new Nullable<int>();
            
            if (HttpContext.Current != null) {
                var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket != null && !string.IsNullOrEmpty(ticket.UserData) && !string.IsNullOrEmpty(ticket.Name))
                    {
                        JObject userinfo = (JObject)JsonConvert.DeserializeObject(ticket.UserData);
                        id = int.Parse(userinfo["Id"].ToString());
                    }
                }
            }
            
            return id;
        }
 
        public static string GetInfo(TicketType info)
        {
            string result = "";
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null && !string.IsNullOrEmpty(ticket.UserData) && !string.IsNullOrEmpty(ticket.Name))
                {
                    JObject userinfo = (JObject)JsonConvert.DeserializeObject(ticket.UserData);
                    switch (info)
                    {
                        case TicketType.Id:
                            result = userinfo["Id"].ToString();
                            break;
                        case TicketType.AdminName:
                            result = userinfo["AdminName"].ToString();
                            break;
                        case TicketType.RealName:
                            result = userinfo["RealName"].ToString();
                            break;
                    }

                }
            }
            return result;
        }

    }

    public static class AuthorityMemberHelper
    {
        public static string CookieName { get { return FormsAuthentication.FormsCookieName + "_MALL"; } }
        public static string Domain { get { return ConfigurationHelper.Domain; } }

        public enum TicketType
        {
            Id = 0,
            MemberName = 1,
            RealName = 2
        }

        public static bool IsVerified
        {
            get
            {
                return OperatorId != null ? true : false;
            }
        }

        public static int? OperatorId
        {
            get
            {
                return GetId();
            }
        }

        public static string MemberName
        {
            get
            {
                var name = GetInfo(TicketType.MemberName);
                return name.Length > 0 ? name : "系统用户";
            }
        }

        public static string RealName
        {
            get
            {
                var name = GetInfo(TicketType.RealName);
                return name.Length > 0 ? name : "未知";
            }
        }

        public static int? GetId()
        {
            var id = new Nullable<int>();

            if (HttpContext.Current != null)
            {
                var cookie = HttpContext.Current.Request.Cookies[CookieName];
                if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
                {
                    var ticket = FormsAuthentication.Decrypt(cookie.Value);
                    if (ticket != null && !string.IsNullOrEmpty(ticket.UserData) && !string.IsNullOrEmpty(ticket.Name))
                    {
                        JObject userinfo = (JObject)JsonConvert.DeserializeObject(ticket.UserData);
                        id = int.Parse(userinfo["Id"].ToString());
                    }
                }
            }

            return id;
        }

        private static string GetInfo(TicketType info)
        {
            string result = "";
            var cookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                var ticket = FormsAuthentication.Decrypt(cookie.Value);
                if (ticket != null && !string.IsNullOrEmpty(ticket.UserData) && !string.IsNullOrEmpty(ticket.Name))
                {
                    JObject userinfo = (JObject)JsonConvert.DeserializeObject(ticket.UserData);
                    switch (info)
                    {
                        case TicketType.Id:
                            result = userinfo["Id"].ToString();
                            break;
                        case TicketType.MemberName:
                            result = userinfo["MemberName"].ToString();
                            break;
                        case TicketType.RealName:
                            result = userinfo["RealName"].ToString();
                            break;
                    }

                }
            }
            return result;
        }
    }
}