using System.IO;
using System.Web.Hosting;
using System.Web.Mvc;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.IO;
namespace System
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// 读取Css文件生成Css片段
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static MvcHtmlString CssBlock(this HtmlHelper helper, string filePath)
        {
            if (filePath.IsNullOrEmpty()) return new MvcHtmlString("");
            var path = HostingEnvironment.MapPath(filePath);

            FileInfo info = new FileInfo(path);
            if (info.Extension != ".css") throw new Exception("不是Css文件");

            var strinfo = File.ReadAllText(path);

            var result = $"<style type='text/css'>{ strinfo }</style>";

            return MvcHtmlString.Create(result);
        }
        /// <summary>
        /// 文件追加MD5值,防止文件修改后缓存,css和js会自动追加标签，如~/css/jquery.js?v=xxxx
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="filePath">文件路径</param>
        /// <returns></returns>
        public static MvcHtmlString FilePathAppendMd5(this HtmlHelper helper, string filePath)
        {
            try
            {
                if (filePath.IsNullOrEmpty()) return MvcHtmlString.Empty;
                var result = string.Empty;

                var hasPar = filePath.IndexOf('?');
                var strPar = string.Empty;
                var strPrefix = string.Empty;
                var strSuffix = string.Empty;
                if (hasPar > -1)
                {
                    strPar = filePath.Substring(hasPar);
                    filePath = filePath.Substring(0, hasPar);
                }

                var path = HostingEnvironment.MapPath(filePath);

                FileInfo fileinfo = new FileInfo(path);
                if (fileinfo.Extension == ".css")
                {
                    strPrefix = "<link rel='stylesheet' href='";
                    strSuffix = "' />";
                }
                else if (fileinfo.Extension == ".js")
                {
                    strPrefix = "<script type='text/javascript' src='";
                    strSuffix = "' ></script>";
                }

                var strmd5 = FileHelper.GetFileMd5(path);

                var flag = strPar.IsNullOrEmpty() ? "?" : "&";

                result = $"{strPrefix}{filePath}{strPar}{flag}_v={strmd5}{strSuffix}";

                return MvcHtmlString.Create(result);
            }
            catch
            {
                return MvcHtmlString.Create(filePath);
            }
        }
    }
}
