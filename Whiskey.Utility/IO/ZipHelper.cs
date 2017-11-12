using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Whiskey.Utility.IO
{
    public static class ZipHelper
    {
        #region 上传压缩文件
        /// <summary>
        /// 上传压缩文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="strSavePath">保存路径</param>
        public static bool UploadZip(Stream stream, string strSavePath,string name,out List<string> listPath)
        {
            try
            {
                //当目录不存在时，创建目录
                if (!Directory.Exists(strSavePath))
                {
                    Directory.CreateDirectory(strSavePath);
                }
                FileStream fsWrite = new FileStream(strSavePath+name, FileMode.OpenOrCreate, FileAccess.Write);
                int size = 2048;
                byte[] buffer = new byte[size];
                while (true)
                {
                    int count = stream.Read(buffer, 0, size);
                    if (count==0)
                    {
                        break;
                    }
                    fsWrite.Write(buffer, 0, count);
                }
                //fsWrite.Close();
                fsWrite.Dispose();
                listPath=GetPathList(strSavePath + name);
                Decompress(strSavePath + name, strSavePath);
                DeleteZip(strSavePath + name);
                return true;
            }
            catch (Exception)
            {
                listPath = null;
                return false;
            }
        }
        #endregion

        #region 解压文件
        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="readPath">压缩文件路径</param>
        /// <param name="savePath">解压文件路径</param>
        private static void Decompress(string readPath, string savePath)
        {
            try
            {
                FastZip fz = new FastZip();
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                fz.ExtractZip(readPath, savePath, "");
            }
            catch (Exception)
            {                
                throw;
            }                 
        }
        #endregion

        #region 获取压缩文件里静态页面的路径
        /// <summary>
        /// 获取静态页面路径
        /// </summary>
        /// <param name="path">压缩文件路径</param>
        private static List<string> GetPathList(string path)
        {
            
             List<string> listPath = new List<string>();
             using (ZipFile zip = new ZipFile(path))
             {                 
                 string list = string.Empty;
                 foreach (ZipEntry entry in zip)
                 {                        
                     string name = entry.Name;
                     int index = name.LastIndexOf('.');                       
                     if (index>0)
                     {
                         list = name.Substring(index+1);
                         if (list.Equals("html", StringComparison.CurrentCultureIgnoreCase) || list.Equals("htm", StringComparison.CurrentCultureIgnoreCase))
                         {
                             listPath.Add(name);
                         }
                     }
                 }
             }
             return listPath;
        }
        #endregion

        #region 删除压缩文件
        /// <summary>
        /// 删除压缩文件
        /// </summary>
        /// <param name="path">压缩文件路径</param>
        private static void DeleteZip(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                File.Delete(path);
            }
        }
        #endregion
    }
}
