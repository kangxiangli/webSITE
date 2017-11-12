using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Xml;
using Whiskey.Utility.Data;

namespace Whiskey.Utility.Helper
{
    /// <summary>
    /// 配置文件读取工具
    /// </summary>

    public class ConfigurationHelper
    {
        #region //字段
        private ArrayList bytesArray;
        private Encoding encoding = Encoding.UTF8;
        private string boundary = String.Empty;
        #endregion

        public ConfigurationHelper()
        {
            bytesArray = new ArrayList();
            encoding = Encoding.UTF8;
            string flag = DateTime.Now.Ticks.ToString("x");
            boundary = "---------------------------" + flag;
        }

        /// <summary>
        /// 读取配置文件节点AppSetting值
        /// </summary>
        /// <param name="name">key的值</param>
        public static string GetAppSetting(string name)
        {
            string value = ConfigurationManager.AppSettings[name];
            return value;
        }
        public static string GetAppSetting(string name, string defaltValue = null)
        {
            string value = ConfigurationManager.AppSettings[name];
            if (string.IsNullOrEmpty(value))
            {
                return defaltValue;
            }
            return value;
        }

        /// <summary>
        /// yxk 2016-6-23
        /// 修改config文件 ,本修改方法并不一定适用于所有情况，
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void SetAppSetting(string name, string value)
        {


            string assemblyConfigFile = Assembly.GetEntryAssembly().Location;
            string appDomainConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);


            AppSettingsSection appSettings = (AppSettingsSection)config.GetSection("appSettings");


            appSettings.Settings.Remove(name);
            appSettings.Settings.Add(name, value);


            config.Save();
        }

        public static void SetAppSetting1(string name, string value)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                string appDomainConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                doc.Load(appDomainConfigFile);
                XmlNode node;
                XmlElement element;
                node = doc.SelectSingleNode("//appSettings");
                var key = "add[@key='" + name + "']";
                element = (XmlElement)node.SelectSingleNode(key);
                element.SetAttribute("value", value);
                doc.Save(appDomainConfigFile);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 判断是否是开发环境
        /// </summary>
        /// <returns></returns>
        public static bool IsDevelopment()
        {
            var isDev = ConfigurationHelper.GetAppSetting("IsDevelopment");
            if (string.IsNullOrEmpty(isDev))
            {
                return false;
            }
            int isDevInt;
            if (!int.TryParse(isDev, out isDevInt))
            {
                return false;
            }
            return isDevInt == 1;

        }
        /// <summary>
        /// 启用测试人脸参数
        /// </summary>
        public static bool EnableFaceDev
        {
            get {
                var EnableManyDevice = ConfigurationHelper.GetAppSetting("EnableManyDevice");
                if (string.IsNullOrEmpty(EnableManyDevice))
                {
                    return false;
                }
                int isenable = 1;
                if (!int.TryParse(EnableManyDevice, out isenable))
                {
                    return false;
                }
                return isenable > 0;
            }
        }

        public static bool EnableManyDevice
        {
            get
            {
                var EnableManyDevice = ConfigurationHelper.GetAppSetting("EnableManyDevice");
                if (string.IsNullOrEmpty(EnableManyDevice))
                {
                    return true;
                }
                bool isenable;
                if (!bool.TryParse(EnableManyDevice, out isenable))
                {
                    return false;
                }
                return isenable;
            }
        }

        public static string WebUrl
        {
            get
            {
                var strWebUrl = GetAppSetting("WebUrl");
                return strWebUrl;
            }
        }

        public static string ApiUrl
        {
            get
            {
                var strApiUrl = GetAppSetting("ApiUrl");
                return strApiUrl;
            }
        }

        public static string Domain
        {
            get
            {
                var domain = GetAppSetting("Domain");
                if (string.IsNullOrEmpty(domain))
                {
                    return FormsAuthentication.CookieDomain;
                }
                return domain;
            }
        }

        /// <summary>
        /// 跨域名读取方法
        /// </summary>
        /// <param name="methodName">方法路径</param>
        /// <param name="Paras">参数</param>
        /// <param name="domain">根目录（0：weburl；1：apiurl）</param>
        /// <returns></returns>
        public static T CrossDomainPost<T>(string methodName, IDictionary<string, object> Paras, int domain, T defValue = default(T))
        {
            try
            {
                string strUrl = "";
                if (domain == 0)
                {
                    strUrl = WebUrl + "/" + methodName;
                }

                string paras = GetParas(Paras);

                System.Net.WebClient wCient = new System.Net.WebClient();
                wCient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                wCient.Encoding = Encoding.UTF8;

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

                string responseData = wCient.UploadString(strUrl, "POST", paras);

                T returnStr = JsonConvert.DeserializeObject<T>(responseData);//返回接受的数据   

                return returnStr;
            }
            catch (Exception ex)
            {
                return defValue;
            }
        }

        /////// <summary>
        /////// 跨域名读取方法
        /////// </summary>
        /////// <param name="methodName">方法路径</param>
        /////// <param name="Paras">参数</param>
        /////// <param name="domain">根目录（0：weburl；1：apiurl）</param>
        /////// <returns></returns>
        //public static T CrossDomainPost<T>(string methodName, HttpFileCollectionBase files, IDictionary<string, object> Paras, int domain, T defValue = default(T))
        //{
        //    try
        //    {
        //        string strUrl = "";
        //        if (domain == 0)
        //        {
        //            strUrl = WebUrl_Http + "/" + methodName;
        //        }

        //        string paras = GetParas(Paras);

        //        System.Net.WebClient wCient = new System.Net.WebClient();
        //        wCient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
        //        foreach (string key in files.AllKeys)
        //        {
        //            wCient.Headers.Add("File_" + key, ConverToByte(files[key]));
        //        }
        //        wCient.Encoding = Encoding.UTF8;

        //        ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;

        //        string responseData = wCient.UploadData(strUrl, "POST", paras);
        //        //string responseData = wCient.UploadFile(strUrl, "POST", HttpUtility.UrlEncode(Convert.ToBase64String()));

        //        T returnStr = JsonConvert.DeserializeObject<T>(responseData);//返回接受的数据   

        //        return returnStr;
        //    }
        //    catch (Exception ex)
        //    {
        //        return defValue;
        //    }
        //}

        /// <summary>
        /// 获取参数
        /// </summary>
        /// <param name="Paras">参数</param>
        /// <returns></returns>
        private static string GetParas(IDictionary<string, object> Paras)
        {
            string paras = "";
            foreach (var item in Paras)
            {
                paras += "&" + item.Key + "=" + item.Value.ToString();
            }

            paras = paras.TrimStart('&');
            return paras;
        }

        /// <summary>
        /// 将文件转换成流
        /// </summary>
        /// <param name="Paras">参数</param>
        /// <returns></returns>
        private static byte[] ConverToByte(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return null;
            }
            byte[] byteFile = new byte[file.InputStream.Length];
            file.InputStream.Read(byteFile, 0, Convert.ToInt32(file.InputStream.Length));
            file.InputStream.Close();
            return byteFile;
            //return HttpUtility.UrlEncode(Convert.ToBase64String(byteFile));
        }

        #region //方法
        /// <summary>
        /// 合并请求数据
        /// </summary>
        /// <returns></returns>
        private byte[] MergeContent()
        {
            int length = 0;
            int readLength = 0;
            string endBoundary = "--" + boundary + "--\r\n";
            byte[] endBoundaryBytes = encoding.GetBytes(endBoundary);

            bytesArray.Add(endBoundaryBytes);

            foreach (byte[] b in bytesArray)
            {
                length += b.Length;
            }

            byte[] bytes = new byte[length];

            foreach (byte[] b in bytesArray)
            {
                b.CopyTo(bytes, readLength);
                readLength += b.Length;
            }

            return bytes;
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="requestUrl">请求url</param>
        /// <param name="responseText">响应</param>
        /// <returns></returns>
        public static T Upload<T>(String requestUrl, HttpFileCollectionBase files, IDictionary<string, string> dic, int domain)
        {
            string strUrl = "";
            if (domain == 0)
            {
                strUrl = WebUrl + "/" + requestUrl;
            }
            else
            {
                strUrl = ApiUrl + "/" + requestUrl;
            }
            ConfigurationHelper ch = new ConfigurationHelper();

            WebClient webClient = new WebClient();
            webClient.Headers.Add("Content-Type", "multipart/form-data; boundary=" + ch.boundary);

            foreach (var item in dic)
            {
                webClient.QueryString.Add(item.Key, item.Value);
            }
            if (files != null && files.Count > 0)
            {
                HttpPostedFileBase file = files[0];
                ch.SetFieldValue("upload", file.FileName, "multipart/form-data", ConverToByte(file));
            }


            byte[] responseBytes;
            byte[] bytes = ch.MergeContent();

            try
            {
                responseBytes = webClient.UploadData(strUrl, bytes);
                string responseText = System.Text.Encoding.UTF8.GetString(responseBytes);
                T returnStr = JsonConvert.DeserializeObject<T>(responseText);
                return returnStr;
            }
            catch (WebException ex)
            {
                Stream responseStream = ex.Response.GetResponseStream();
                responseBytes = new byte[ex.Response.ContentLength];
                responseStream.Read(responseBytes, 0, responseBytes.Length);
            }
            return default(T);
        }

        /// <summary>
        /// 设置表单数据字段
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="fieldValue">字段值</param>
        /// <returns></returns>
        public void SetFieldValue(String fieldName, String fieldValue)
        {
            string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}\r\n";
            string httpRowData = String.Format(httpRow, fieldName, fieldValue);

            bytesArray.Add(encoding.GetBytes(httpRowData));
        }

        /// <summary>
        /// 设置表单文件数据
        /// </summary>
        /// <param name="fieldName">字段名</param>
        /// <param name="filename">字段值</param>
        /// <param name="contentType">内容内型</param>
        /// <param name="fileBytes">文件字节流</param>
        /// <returns></returns>
        public void SetFieldValue(String fieldName, String filename, String contentType, Byte[] fileBytes)
        {
            string end = "\r\n";
            string httpRow = "--" + boundary + "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string httpRowData = String.Format(httpRow, fieldName, filename, contentType);

            byte[] headerBytes = encoding.GetBytes(httpRowData);
            byte[] endBytes = encoding.GetBytes(end);
            byte[] fileDataBytes = new byte[headerBytes.Length + fileBytes.Length + endBytes.Length];

            headerBytes.CopyTo(fileDataBytes, 0);
            fileBytes.CopyTo(fileDataBytes, headerBytes.Length);
            endBytes.CopyTo(fileDataBytes, headerBytes.Length + fileBytes.Length);

            bytesArray.Add(fileDataBytes);
        }
        #endregion
    }
}
