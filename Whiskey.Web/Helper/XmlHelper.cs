using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Threading.Tasks;

namespace Whiskey.Web.Helper
{
    public static class XmlHelper
    {

        /// <summary>
        /// 对象序列化成 XML String
        /// </summary>
        public static string Serialize<T>(T obj)
        {
            string xmlString = string.Empty;
            try {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (MemoryStream ms = new MemoryStream())
                {
                    xmlSerializer.Serialize(ms, obj);
                    xmlString = Encoding.UTF8.GetString(ms.ToArray());
                }
            }catch(Exception ex){
                xmlString = ex.Message;
            }
            return xmlString;
        }

        /// <summary>
        /// XML String 反序列化成对象
        /// </summary>
        public static T Deserialize<T>(string xmlString)
        {
            T t = default(T);
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                using (Stream xmlStream = new MemoryStream(Encoding.UTF8.GetBytes(xmlString)))
                {
                    using (XmlReader xmlReader = XmlReader.Create(xmlStream))
                    {
                        Object obj = xmlSerializer.Deserialize(xmlReader);
                        t = (T)obj;
                    }
                }
            }catch(Exception ex) {
                throw ex;
            }
            return t;
        }




    }
}
