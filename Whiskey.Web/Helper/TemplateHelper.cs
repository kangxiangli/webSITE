using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Whiskey.Utility.Helper;

namespace Whiskey.Web.Helper
{
    public static class TemplateHelper
    {
        /// <summary>
        /// 保存模版
        /// </summary>
        /// <param name="templatePath">模板路径</param>
        /// <param name="fileName">文件名</param>
        /// <param name="suffix">后缀名</param>
        /// <param name="templateHtml">html</param>
        /// <returns></returns>
        public static bool SaveTemplate(string templatePath, string fileName, string suffix,string templateHtml)
        {

           
            bool result = true;
            string fullPath = templatePath + fileName + suffix;
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }  
            FileStream  fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs, Encoding.UTF8); ;
            try
            {
                          
                sw.Write(templateHtml);
            }
            catch (Exception)
            {
                result = false;
                return false;
            }
            finally
            {
                sw.Close();
                fs.Close();
                sw.Dispose();
                fs.Dispose();
            }
            return result;
        }

        /// <summary>
        /// 删除模版文件
        /// </summary>
        /// <param name="fileName">文件路径</param>
        /// <returns></returns>
        public static bool DelTemplate(string path)
        {
            //获取当前程序路径
            StringBuilder sbPath = new StringBuilder(AppDomain.CurrentDomain.BaseDirectory);
            sbPath.Append(path);
            if (string.IsNullOrEmpty(path)) return false;

            try
            {
                File.Delete(sbPath.ToString());
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        /// <summary>
        /// 校验后缀名
        /// </summary>
        /// <param name="fileName">文件名（包含后缀名）</param>
        /// <param name="suffix">后缀名(.*)</param>
        /// <returns></returns>
        public static bool CheckSuffix(string fileName,string suffix)
        {
            int lastIndex = fileName.LastIndexOf('.');
            string fileNameSuffix = fileName.Substring(lastIndex+1);
            if (fileNameSuffix==suffix)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 保存上传文件
        /// </summary>
        /// <param name="stream">内容</param>
        /// <param name="path">路径</param>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static bool SaveUploadFile(Stream stream, string savePath, string fileName)
        {
            FileStream fs=null;
            BinaryWriter bw = null;
            if (string.IsNullOrEmpty(savePath)) return false;
            if (string.IsNullOrEmpty(fileName)) return false;
            try
            {
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                fs = new FileStream(savePath + fileName, FileMode.OpenOrCreate, FileAccess.Write);
                bw = new BinaryWriter(fs, Encoding.UTF8);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);

                bw.Write(bytes);
                return true;
            }
            catch (Exception)
            {
                fs.Close();
                fs.Dispose();
                bw.Close();
                bw.Dispose();
                return false;
            }
            finally 
            {
                fs.Close();
                fs.Dispose();
                bw.Close();
                bw.Dispose();
            }
        }

        #region 保存上传文件
        /// <summary>
        /// 保存上传文件
        /// </summary>
        /// <param name="stream">内容</param>
        /// <param name="path">绝对路径</param>
        /// <returns></returns>
        public static bool SaveUploadFile(Stream stream, string savePath)
        {
            FileStream fs=null;
            BinaryWriter bw = null;
            if (string.IsNullOrEmpty(savePath)) return false;            
            savePath = FileHelper.UrlToPath(savePath);
            try
            {                
                fs = new FileStream(savePath, FileMode.OpenOrCreate, FileAccess.Write);
                bw = new BinaryWriter(fs, Encoding.UTF8);
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                bw.Write(bytes);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally 
            {
                fs.Close();
                fs.Dispose();
                bw.Close();
                bw.Dispose();
            }
        }
        #endregion
        /// <summary>
        /// 获取文件名
        /// </summary>
        /// <param name="fileName">文件名称</param>
        /// <returns></returns>
        public static string GetFile(string fileName)
        {
            int lastIndex = fileName.LastIndexOf('.');
            string name=fileName.Substring(0, lastIndex);
            return name;
        }
        public static bool SaveHtml(string url, string path, int articleId, int templateId)
        {

            try
            {
                WebRequest request = WebRequest.Create(url + "?articleId=" + articleId + "&templateId=" + templateId);
                Stream stream = request.GetResponse().GetResponseStream();
                StreamReader sr = new StreamReader(stream, Encoding.UTF8);
                string htmlcontent = sr.ReadToEnd();
                stream.Close();
                sr.Close();
                FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
                StreamWriter sw = new StreamWriter(fs, Encoding.UTF8);
                sw.Write(htmlcontent);
                sw.Close();
                fs.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }
        public static void ReadJS(string path) 
        {
            try
            {
                StreamReader sr = new StreamReader(path);
                string str1=sr.ToString();
            }
            catch (Exception)
            {
                
                throw;
            }
        }
    }
}
