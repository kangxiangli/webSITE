using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.IO;
using System.Text;
using System.Drawing;
 

namespace Whiskey.Web.Helper
{
    public static class FileHelper
    {
        #region 删除文件
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="fileName">绝对路径</param>
        /// <returns></returns>
        public static bool Delete(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return false;
            try
            {
                var path = HostingEnvironment.MapPath(fileName);
                if (System.IO.File.Exists(path))
                {
                    System.IO.File.Delete(path);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// 删除文件夹下指定时间之前的文件
        /// </summary>
        /// <param name="folderPath">虚拟文件夹路径</param>
        /// <param name="timeDay">天数</param>
        /// <param name="timeHour">小时</param>
        /// <param name="timeMinute">分钟</param>
        /// <returns></returns>
        public static bool Delete(string folderPath, DateTime timeOut)
        {
            try
            {
                folderPath = HostingEnvironment.MapPath(folderPath);
                DirectoryInfo di = new DirectoryInfo(folderPath);
                if (di.Exists)
                {
                    var delFiles = di.EnumerateFiles().Where(s => s.CreationTime < timeOut);
                    foreach (var file in delFiles)
                    {
                        file.Delete();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region 保存文件
        /// <summary>
        /// 保存文件
        /// </summary>
        /// <param name="path">全路径</param>
        /// <param name="content">保存的内容</param>
        /// <returns></returns>
        public static bool SavePath(string path,string content)
        {
            path = UrlToPath(path);    
            //FileStream fs = new FileStream(path,FileMode.Create);                        
            try
            {
                File.WriteAllText(path, content, Encoding.UTF8);　            
                //byte[] buffer = Encoding.Default.GetBytes(content);
                //fs.Write(buffer, 0, buffer.Length);
                return true;
            }
            catch (Exception)
            {               
                return false;
            }
            finally
            {
                //fs.Close();
                //fs.Dispose();                 
            }
         }
        #endregion

        #region 校验路径
        /// <summary>
        /// 校验路径（文件路径存在则返回全路径；否则，创建并返回全路径）
        /// </summary>
        /// <param name="url">绝对路径</param>
        /// <returns></returns>
        public static string UrlToPath(string url)
        {
            var path = HostingEnvironment.MapPath(url);
            var directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return path;
        }
        #endregion

        #region 校验主题路径下必要文件是否存在
        /// <summary>
        /// 校验主题路径下必要文件是否存在
        /// </summary>
        /// <param name="themePath"></param>
        /// <returns></returns>
        public static bool ThemeIsExist(string themePath)
        {
            var path = HostingEnvironment.MapPath(themePath);
            var directory = Path.GetDirectoryName(path);
            var indexpath = Path.Combine(directory, "_Index.cshtml");
            var menupath = Path.Combine(directory, "_Menu.cshtml");
            var navpath = Path.Combine(directory, "_Nav.cshtml");
            var headpath = Path.Combine(directory, "_Head.cshtml");
            //var footpath = Path.Combine(directory, "_Foot.cshtml");
            if (Directory.Exists(directory) && File.Exists(menupath) && File.Exists(navpath) && File.Exists(headpath) && File.Exists(indexpath))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 校验商城主题路径下必要文件是否存在
        /// </summary>
        /// <param name="themePath"></param>
        /// <returns></returns>
        public static bool ThemeMallIsExist(string themePath)
        {
            var path = HostingEnvironment.MapPath(themePath);
            var directory = Path.GetDirectoryName(path);
            var indexpath = Path.Combine(directory, "_Index.cshtml");
            var menupath = Path.Combine(directory, "_Menu.cshtml");
            var navpath = Path.Combine(directory, "_Nav.cshtml");
            var path2 = Path.Combine(directory, "_RegisterLogin.cshtml");
            if (Directory.Exists(directory) && File.Exists(indexpath) && File.Exists(path2) && File.Exists(menupath) && File.Exists(navpath))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region 校验某一个文件是否存在
        /// <summary>
        /// 校验某一个文件是否存在
        /// </summary>
        /// <param name="themePath"></param>
        /// <returns></returns>
        public static bool FileIsExist(string filePath)
        {
            var filepath = HostingEnvironment.MapPath(filePath);
            var directory = Path.GetDirectoryName(filepath);
            if (Directory.Exists(directory) && File.Exists(filepath))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region 清空文件夹下的内容
        /// <summary>
        /// 清空文件夹下的文件和文件夹
        /// </summary>
        /// <param name="dir">文件夹路径</param>
        public static void DeleteFolder(string path)
        {
            foreach (string file in Directory.GetFileSystemEntries(path))
            {
                if (System.IO.File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fileInfo.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file);//直接删除其中的文件  
                }
                else
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(file);
                    if (dirInfo.GetFiles().Length != 0)
                    {
                        DeleteFolder(dirInfo.FullName);////递归删除子文件夹
                    }
                    Directory.Delete(file);
                }
            }
        }
        #endregion

        #region 删除文件夹及其内容
        /// <summary>
        /// 删除文件夹及其内容
        /// </summary>
        /// <param name="dir">全路径</param>
        public static void DeleteFiles(string path)
        {
            foreach (string file in Directory.GetFileSystemEntries(path))
            {
                if (System.IO.File.Exists(file))
                {
                    FileInfo fileInfo = new FileInfo(file);
                    if (fileInfo.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fileInfo.Attributes = FileAttributes.Normal;
                    System.IO.File.Delete(file);//直接删除其中的文件  
                }
                else
                {
                    DeleteFolder(file);////递归删除子文件夹
                    Directory.Delete(file);
                }
            }
            if (Directory.Exists(path))
            {
                Directory.Delete(path);
            }
        }
        #endregion

        #region 上传文件
        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="stream">文件流</param>
        /// <param name="savePath">保存文件路径全路径</param>
        /// <returns></returns>                
        public static bool SaveUpload(Stream stream,string savePath)
        {
            savePath = UrlToPath(savePath);
            FileStream fs = new FileStream(savePath, FileMode.Create);
            try
            {
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                fs.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                //fs.Close();
                fs.Dispose();
            }
             
        }
        #endregion

        public static string Rename(string strPath)
        {
            
            string strFileName = string.Empty;
            string strBaseFile = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(strPath))
            {
                return strPath;
            }
            else
            {
                string strFilePath = strBaseFile + strPath;
                FileInfo fileInfo = new FileInfo(strFilePath);
                bool exist = fileInfo.Exists;
                if (exist)
                {
                    Image image = Image.FromFile(strFilePath);                    
                    string strExtension = fileInfo.Extension;
                    string strName = fileInfo.Name;
                    string[] arrStr = strName.Split('.');
                    string strTemp = arrStr[0];
                    string strImage="_" + image.Width.ToString() + "_" + image.Height.ToString();
                    if (strTemp.Contains(strImage))
                    {
                       
                        return strPath;
                    }
                    image.Dispose();
                    strTemp = strTemp + strImage + strExtension;
                    string strNewPath = strFilePath.Replace(strName, strTemp);
                    if (File.Exists(strNewPath))
                    {                        
                        return strPath;
                    }
                    else
                    {                        
                        strPath = strPath.Replace(strName, strTemp);
                        fileInfo.CopyTo(strNewPath);                        
                        fileInfo.Delete();
                        return strPath;
                    }                                       
                }
                else
                {
                    return strPath;
                }
            }
        }

        /// <summary>
        /// 虚拟路径转物理路径
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string Map2PhysicalPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(virtualPath))
                return virtualPath;
            return HostingEnvironment.MapPath(virtualPath);
        }
        /// <summary>
        /// 物理路径转虚拟路径
        /// </summary>
        /// <param name="physicalPath"></param>
        /// <returns></returns>
        public static string Map2VirtualPath(string physicalPath)
        {
            if (string.IsNullOrEmpty(physicalPath))
                return physicalPath;
            string tmpRootDir = HostingEnvironment.MapPath(HttpContext.Current.Request.ApplicationPath);//获取程序根目录
            physicalPath = physicalPath.Replace(tmpRootDir, "/"); //转换成相对路径
            physicalPath = physicalPath.Replace(@"\", @"/");
            return physicalPath;
        }
    }
}