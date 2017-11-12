
//  <copyright file="LinqToXmlExtensions.cs" company="优维拉软件设计工作室">



//  <last-date>2014-07-18 17:43</last-date>


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;


namespace Whiskey.Utility.Extensions
{
    /// <summary>
    /// Xml 扩展操作类
    /// </summary>
    public static class LinqToXmlExtensions
    {
        #region 公共方法

        /// <summary>
        /// 将XmlNode转换为XElement
        /// </summary>
        /// <returns> XElment对象 </returns>
        public static XElement ToXElement(this XmlNode node)
        {
            XDocument xdoc = new XDocument();
            using (XmlWriter xmlWriter = xdoc.CreateWriter())
            {
                node.WriteTo(xmlWriter);
            }
            return xdoc.Root;
        }

        /// <summary>
        /// 将XElement转换为XmlNode
        /// </summary>
        /// <returns> 转换后的XmlNode </returns>
        public static XmlNode ToXmlNode(this XElement element)
        {
            using (XmlReader xmlReader = element.CreateReader())
            {
                XmlDocument xml = new XmlDocument();
                xml.Load(xmlReader);
                return xml;
            }
        }

        #endregion
    }
}