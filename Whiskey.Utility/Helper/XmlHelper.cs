using System.Data;
using System.IO;
using System.Web;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using Whiskey.Utility.Helper;

namespace Whiskey.Utility
{
    public class XmlHelper
    {
        private string strXmlFile;
        public XElement XmlRoot;
        private bool autoSave = false;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="DirName">文件夹名</param>
        /// <param name="FileName">文件名（不含扩展名）</param>
        /// <param name="autoSave">操作自动保存【false 时需要手动调用Save() 】</param>
        public XmlHelper(string DirName, string FileName, bool autoSave = false)
        {
            DirName.CheckNotNullOrEmpty("DirName");
            FileName.CheckNotNullOrEmpty("FileName");
            this.autoSave = autoSave;

            var basePath = Path.Combine(HttpRuntime.AppDomainAppPath, "Content/Config/", DirName);
            strXmlFile = Path.Combine(basePath, FileName + ".xml");
            try
            {
                if (!Directory.Exists(basePath))
                {
                    Directory.CreateDirectory(basePath);
                }
                if (!File.Exists(strXmlFile))
                {
                    using (var stream = File.Create(strXmlFile))
                    {

                    }
                }
                XmlRoot = XDocument.Load(strXmlFile).Element("Root") ?? new XElement("Root");
            }
            catch
            {
                XmlRoot = new XElement("Root");
            }
        }

        #region  保存XML文件  
        /// <summary>  
        /// 保存文件  
        /// </summary>  
        /// <param name="XmlFile">XML保存的路径</param>  
        public void Save()
        {
            XmlRoot.Save(strXmlFile);
        }
        #endregion

        #region 添加元素
        /// <summary>  
        /// 添加元素  
        /// </summary>  
        /// <param name="parentElement"></param>  
        /// <param name="childElement">new XElement("节点名称", new XAttribute("节点属性", 节点属性),  new XElement("子节点", new XAttribute("节点属性", 节点属性))，无限添加子节点   );</param>  
        public bool AddElement(params XElement[] childElement)
        {
            try
            {
                XmlRoot.Add(childElement);
                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 根据元素名称删除元素  
        /// <summary>  
        /// 根据元素名称删除元素  
        /// </summary>  
        /// <param name="element"></param>  
        /// <param name="RemoveElementID"></param>  
        public bool RemoveElement(string RemoveElementID)
        {
            try
            {
                XElement xe = XmlRoot.Element(RemoveElementID);
                xe.Remove();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (autoSave) Save();
            }
        }

        #endregion

        #region 修改某元素的值  
        /// <summary>  
        /// 修改某元素的值  
        /// </summary>  
        /// <param name="element"></param>  
        /// <param name="elementName"></param>  
        /// <param name="setValue"></param>  
        public bool ModifyElement(string elementName, string setValue)
        {
            try
            {
                var cur = GetElement(elementName);
                if (cur != null)
                {
                    cur.SetValue(setValue);
                    return true;
                }
                else
                {
                    return AddElement(new XElement(elementName, setValue));
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                if (autoSave) Save();
            }
        }
        #endregion

        #region 根据元素名称查询元素  
        /// <summary>  
        /// 根据元素名称查询元素  
        /// </summary>  
        /// <param name="element"></param>  
        /// <param name="RemoveElementID"></param>  
        public XElement GetElement(string QueryElementID)
        {
            try
            {
                return XmlRoot.Element(QueryElementID);
            }
            catch { return null; }
        }
        /// <summary>  
        /// 根据元素名称批量查询元素  
        /// </summary>  
        /// <param name="element"></param>  
        /// <param name="RemoveElementID"></param>  
        public IEnumerable<XElement> GetElements(string QueryElementID)
        {
            try
            {
                return XmlRoot.Elements(QueryElementID);
            }
            catch { return null; }
        }
        /// <summary>  
        /// 根据元素名和   属性名称批量查询元素  
        /// </summary>  
        /// <param name="element">源</param>  
        /// <param name="QueryElementID"></param>  
        /// <param name="AttributeName">属性名</param>  
        /// <param name="AttributeValue">属性值</param>  
        /// <returns></returns>  
        public IEnumerable<XElement> GetElements(string QueryElementID, string AttributeName, string AttributeValue)
        {
            try
            {
                return XmlRoot.Elements(QueryElementID).Where(i => i.Attribute(AttributeName).Value == AttributeValue).ToList<XElement>();
            }
            catch { return null; }
        }
        #endregion
    }
    public static class XmlStaticHelper
    {
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="DirName">文件夹名称</param>
        /// <param name="xmlFileName">文件名称</param>
        /// <param name="QueryElementID">节点名称</param>
        /// <param name="defValue">默认返回值（不传默认返回“0”）</param>
        /// <returns></returns>
        public static string GetXmlNodeByXpath(string DirName, string xmlFileName, string QueryElementID, string defValue = "0")
        {
            Utility.XmlHelper xml = new Utility.XmlHelper(DirName, xmlFileName, true);
            XElement xel = xml.GetElement(QueryElementID);
            return xel == null || xel.Value == null ? defValue : xel.Value;
        }

        ///<summary>
        /// 修改节点
        ///</summary>
        /// <param name="DirName">文件夹名称</param>
        /// <param name="xmlFileName">文件名称</param>
        /// <param name="QueryElementID">节点名称</param>
        /// <param name="innerText">配置的值</param>
        /// <returns></returns>
        public static bool UpdateNode(string DirName, string xmlFileName, string QueryElementID, string innerText)
        {
            Utility.XmlHelper xml = new Utility.XmlHelper(DirName, xmlFileName, true);
            return xml.ModifyElement(QueryElementID, innerText);
        }

        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <param name="url">要读取的项目地址</param>
        /// <param name="DirName">文件夹名称</param>
        /// <param name="xmlFileName">文件名称</param>
        /// <param name="QueryElementID">节点名称</param>
        /// <param name="defValue">默认返回值（不传默认返回“0”）</param>
        /// <returns></returns>
        public static string GetXmlNodeByXpath_Url(string url, string DirName, string xmlFileName, string QueryElementID, string defValue = "0")
        {
            IDictionary<string, object> dic = new Dictionary<string, object>();
            dic.Add("DirName", DirName);
            dic.Add("xmlFileName", xmlFileName);
            dic.Add("QueryElementID", QueryElementID);
            dic.Add("defValue", defValue);

            return ConfigurationHelper.CrossDomainPost<string>(url, dic, 0, defValue);
        }
    }
}
