using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Routing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Net;
using System.Management;
using System.Xml;
using Whiskey.Utility.Helper;
using System.Web.Caching;
  
namespace Whiskey.Web.Helper
{
    public static class EnvironmentHelper
    {
        public static string ProductPath
        {
            get
            {
                var productPath = "/Content/UploadFiles/Products/" + DateTime.Now.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                if (!Directory.Exists(HttpContext.Current.Server.MapPath(productPath)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(productPath));
                }
                return productPath;
            }
        }

        public static string GalleryPath
        {
            get
            {
                var productPath = "/Content/UploadFiles/Galleries/" + DateTime.Now.ToString("yyyyMMdd", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                if (!Directory.Exists(HttpContext.Current.Server.MapPath(productPath)))
                {
                    Directory.CreateDirectory(HttpContext.Current.Server.MapPath(productPath));
                }
                return productPath;
            }
        }

        public static string TemplatePath(RouteData route)
        {
            var area = route.DataTokens.ContainsKey("area") ? route.DataTokens["area"].ToString().ToLower() : string.Empty;
            var controller = route.Values["controller"].ToString().ToLower();
            var action = route.Values["action"].ToString().ToLower();
            var path = "Areas\\" + area + "\\Views\\" + controller+"\\Templates";
            return path;
        }


        /// <summary>
        /// 获取本机Excel版本
        /// </summary>
        /// <returns></returns>
        public static int ExcelVersion()
        {
            
            Type objExcelType = Type.GetTypeFromProgID("Excel.Application");
            if (objExcelType == null)
            {
                return 2003;
            }
            object objApp = Activator.CreateInstance(objExcelType);
            if (objApp == null)
            {
                return 2003;
            }
            object objVer = objApp.GetType().InvokeMember("Version", BindingFlags.GetProperty, null, objApp, null);
            double iVer = Convert.ToDouble(objVer.ToString());
            objVer = null;
            objApp = null;
            objExcelType = null;
            GC.Collect();

            var result = 0;
            if (iVer >= 12) {
                result = 2007;
            }else if (iVer >= 7 && iVer<=11){
                result = 2003;
            }
            return result;
        }


        /// <summary>
        /// 获取IP地址
        /// </summary>
        /// <param name="IsLocal">是否获取本地IP</param>
        /// <returns></returns>
        public static string GetIP()
        {
            string AddressIP = string.Empty;
            if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null){
                AddressIP = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"].ToString();
            }else{
                AddressIP = HttpContext.Current.Request.UserHostAddress.ToString();
            }
            if (string.IsNullOrEmpty(AddressIP)) {
                AddressIP = "127.0.0.1"; 
            }
            return AddressIP;
        }


        /// <summary>
        /// 获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMAC()
        {
            string MacAddress = String.Empty;
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"] == true)
                {
                    MacAddress = mo["MacAddress"].ToString();
                    break;
                }
            }
            moc = null;
            mc = null;
            MacAddress = MacAddress.Replace(":", "-");
            return MacAddress;
        }

        /// <summary>
        /// yky 2015-11-13
        /// </summary>
        public static bool IsStore
        {
            get
            {
                var host = HttpContext.Current.Request.Url.Host.ToString().ToLower();
                List<string> listUrl= ReadUrlConfiguration();
                return listUrl.Contains(host);
                //return (host.Contains(".ovisa.cn") || host.Contains(".0kufang.com") || host.Contains("localhost") || host.Contains("10.1.1.207") || host.Contains(".0fashion.com")) || host.Contains("192.168.119.1") || host.Contains("192.168.111.1") ? true : false;
            }
        }
        /// <summary>
        /// 使用依赖缓存读取Url配置 yky 2015-11-13
        /// </summary>
        private static List<string> ReadUrlConfiguration()
        {

            if (HttpRuntime.Cache["UrlConfig"] == null)
            {
                string urlPath = ConfigurationHelper.GetAppSetting("UrlConfigurationPath");
                urlPath = FileHelper.UrlToPath(urlPath);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(urlPath);
                XmlElement xmlEle = xmlDoc.DocumentElement;
                XmlNodeList list = xmlEle.ChildNodes;
                List<string> listUrl = new List<string>();
                for (int i = 0; i < list.Count; i++)
                {
                    string url = list[i].SelectSingleNode("path").InnerText;
                    listUrl.Add(url);
                }
                CacheDependency cacheDep = new CacheDependency(urlPath);
                HttpRuntime.Cache.Insert("UrlConfig", listUrl);                
                return listUrl;            
            }
            else
            {
                List<string> list = new List<string>();
                list = (List<string>)HttpRuntime.Cache["UrlConfig"];
                return list;
            }
        }
    }


}