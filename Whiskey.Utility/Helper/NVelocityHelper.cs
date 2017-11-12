using NVelocity.App;
using NVelocity;
using System.IO;
using System.Collections.Generic;
using Whiskey.Utility.Extensions;
using System.Collections;

namespace Whiskey.Utility.Helper
{
    public class NVelocityHelper
    {
        /// <summary>
        /// 生成模板字符串
        /// </summary>
        /// <param name="templdateString">待解析的模板字符串</param>
        /// <param name="keyvalues">数据字典</param>
        /// <param name="logTag">报错时记录日志标识</param>
        /// <returns></returns>
        public static string Generate(string templdateString, Dictionary<string,object> keyvalues, string logTag)
        {
            VelocityEngine templateEngine = new VelocityEngine();
            templateEngine.Init();

            VelocityContext vltContext = new VelocityContext();
            foreach (var item in keyvalues)
            {
                vltContext.Put(item.Key, item.Value);
            }
            vltContext.Put("Helper", new Helper());

            StringWriter writer = new StringWriter();
            templateEngine.Evaluate(vltContext, writer, logTag, templdateString.Html2String());

            return writer.GetStringBuilder().ToString();
        }
        /// <summary>
        /// 生成模板字符串
        /// </summary>
        /// <param name="templdateString">待解析的模板字符串</param>
        /// <param name="keyvalues">数据字典</param>
        /// <returns></returns>
        public static string Generate(string templdateString, Dictionary<string, object> keyvalues)
        {
            return Generate(templdateString, keyvalues, "_template_nvelocity_log_");
        }
        /// <summary>
        /// 生成模板字符串，调用$data.
        /// </summary>
        /// <param name="templdateString">待解析的模板字符串</param>
        /// <param name="data">数据</param>
        /// <param name="logTag">报错时记录日志标识</param>
        /// <returns></returns>
        public static string Generate(string templdateString, object data, string logTag)
        {
            VelocityEngine templateEngine = new VelocityEngine();
            templateEngine.Init();

            VelocityContext vltContext = new VelocityContext();
            vltContext.Put("data", data);
            vltContext.Put("Helper", new Helper());

            StringWriter writer = new StringWriter();
            templateEngine.Evaluate(vltContext, writer, logTag, templdateString.Html2String());

            return writer.GetStringBuilder().ToString();
        }
    }

    public class Helper
    {
        public string TrimEnd(string str, string trim)
        {
            return str.TrimEnd(char.Parse(trim));
        }
        public string TrimStart(string str, string trim)
        {
            return str.TrimStart(char.Parse(trim));
        }
        public string Trim(string str)
        {
            return str.Trim();
        }

        public string Contact(params string[] str)
        {
            return string.Concat(str);
        }

    }
}
