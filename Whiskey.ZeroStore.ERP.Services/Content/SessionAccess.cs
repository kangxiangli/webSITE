using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using System.Xml.Serialization;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Content
{
    //yxk 2016-1 session access
    public class SessionAccess
    {
       
        /// <summary>
        /// 保存数据到session
        /// </summary>
        /// <param name="sessionName">标识名，如果为空将会抛出异常</param>
        /// <param name="data">object数据</param>
        /// <param name="isover">如果存在同名的session值是否覆盖，如果不覆盖将会抛出异常</param>
        /// <returns></returns>
        public static bool Set(string sessionName, object data, bool isover = true)
        {
            HttpContext hc = HttpContext.Current ?? CacheAccess.GetHttpContext();
            if (string.IsNullOrEmpty(sessionName))
            {
                throw new NullReferenceException("sessionName不为空");
            }
            if (hc.Session[sessionName] != null && !isover)
            {
                return false;
                //throw new Exception("同名的session值已经存在且不允许覆盖");
            }
            else
            {
                hc.Session.Remove(sessionName);
                hc.Session.Add(sessionName, data);
                return true;
            }

        }
        /// <summary>
        /// 获取session值
        /// </summary>
        /// <param name="sessionName"></param>
        /// <returns></returns>
        public static object Get(string sessionName)
        {

            if (string.IsNullOrEmpty(sessionName))
            {
                throw new NullReferenceException("sessionName参数不为空");
            }
            else
            {
                HttpContext hc = HttpContext.Current ?? CacheAccess.GetHttpContext();
                if (hc != null)
                {
                    return hc.Session[sessionName];
                }
                return null;
            }
        }

        public static bool Remove(string sessionName)
        {

            if (string.IsNullOrEmpty(sessionName))
            {
                throw new NullReferenceException("sessionName参数不为空");
            }
            else
            {
                try
                {
                    HttpContext hc = HttpContext.Current ?? CacheAccess.GetHttpContext();
                    object obj = hc.Session[sessionName];
                    if (obj != null)
                    {
                        hc.Session.Remove(sessionName);
                    }
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }
        public static T Clone<T>(T RealObject)
        {
            T t = default(T);
            if (!typeof(T).IsValueType && RealObject == null)
            {
                return t;
            }

            using (Stream stream = new MemoryStream())
            {

                try
                {
                    XmlSerializer xmlser = new XmlSerializer(typeof(T));
                    xmlser.Serialize(stream, RealObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (T)xmlser.Deserialize(stream);
                }
                catch (Exception)
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    BinaryFormatter binary = new BinaryFormatter();

                    binary.Serialize(stream, RealObject);
                    stream.Seek(0, SeekOrigin.Begin);
                    t = (T)binary.Deserialize(stream);
                }
                return t;
            }
        }
    }

}
