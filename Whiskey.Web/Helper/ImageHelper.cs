using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Web;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Text;
using BarcodeLib;
using Whiskey.Utility.Helper;
using ThoughtWorks.QRCode.Codec;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using System.Web.Hosting;

namespace Whiskey.Web.Helper
{
    /// <summary>
    /// 图片帮助类
    /// </summary>
    public static class ImageHelper
    {
        #region 获取缩略图
        /// <summary>
        /// 获取缩略图
        /// </summary>
        /// <param name="url">地址</param>
        /// <param name="width">宽</param>
        /// <param name="height">高</param>
        /// <returns></returns>
        public static string GetThumbnailWithBorder(string url, int width, int height)
        {
            var result = string.Empty;
            if (url.Length > 0)
            {
                result = "<div style='width:" + width + "px;height:" + height + "px;overflow:hidden;border:1px solid #eaeaea;margin:0 auto 0 auto;'><img src='" + url + "' style='margin:2px;max-width:" + (width - 6) + "px;' /></div>";
            }
            else
            {
                result = "无图";
            }
            return result;
        }
        #endregion

        #region 生成缩略图
       


        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="originalImagePath">源图片路径</param>
        /// <param name="thumbnailPath">缩略图保存路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="mode">压缩模式（HW：无操作；W：根据输入宽度缩放；H：根据输入高度缩放；Cut：根据输入高和宽等比缩放）</param>
        /// <param name="imageType">保存图片格式（Gif，Jpg，Bmp，Png）</param>
        /// <returns>返回缩略图路径</returns>
        public static string MakeThumbnail(string originalImagePath, string thumbnailPath, int width, int height, string mode, string imageType)
        {
            //if (!File.Exists(originalImagePath))
            //    return null;
            Image originalImage = null;
            try
            {
                originalImage = Image.FromFile(FileHelper.UrlToPath(originalImagePath));
            }
            catch (Exception)
            {
                return thumbnailPath;
            }
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case "HW":
                    break;
                case "W":
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }
            //新建一个bmp图片 
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板 
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);

            try
            {
                thumbnailPath = FileHelper.UrlToPath(thumbnailPath);
                switch (imageType.ToLower())
                {
                    case "gif":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case "jpg":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case "bmp":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case "png":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
            }
            catch (System.Exception e)
            {
                thumbnailPath = e.Message;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            return thumbnailPath;
        }

        /// <summary>
        /// 生成缩略图,文件名自动追加宽高
        /// </summary>
        /// <param name="originalImagePath"></param>
        /// <param name="thumbnailPath"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="mode"></param>
        /// <param name="imageType"></param>
        /// <returns>文件虚拟路径</returns>
        public static string MakeThumbnailAndSizeName(string originalImagePath, string thumbnailPath, int width, int height, string mode, string imageType)
        {
            var extname = Path.GetExtension(thumbnailPath);
            var filename = thumbnailPath.Remove(thumbnailPath.Length - extname.Length);//去除扩展名
            filename = string.Format("{0}_{1}_{2}{3}", filename, width, height, extname);

            string fileVpath = FileHelper.Map2VirtualPath(MakeThumbnail(originalImagePath, filename, width, height, mode, imageType));
            return fileVpath;
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="stream">图片流</param>
        /// <param name="thumbnailPath">缩略图保存路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="mode">压缩模式（HW：无操作；W：根据输入宽度缩放；H：根据输入高度缩放；Cut：根据输入高和宽等比缩放）</param>
        /// <param name="imageType">保存图片格式（Gif，Jpg，Bmp，Png）</param>
        /// <returns>返回缩略图路径</returns>
        public static string MakeThumbnail(Stream stream, string thumbnailPath, int width, int height,int x, int y , string imageType)
        {
            Image originalImage = Image.FromStream(stream);
            int towidth = width;
            int toheight = height;
            //int x = 0;
            //int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;             
            //新建一个bmp图片 
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板 
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充 
            g.Clear(Color.White);
            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, towidth, toheight), GraphicsUnit.Pixel);

            try
            {
                thumbnailPath = FileHelper.UrlToPath(thumbnailPath);
                switch (imageType.ToLower())
                {
                    case "Gif":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case "Jpg":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case "Bmp":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case "Png":
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        bitmap.Save(thumbnailPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
            }
            catch (System.Exception )
            {
                thumbnailPath = string.Empty;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            return thumbnailPath;
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="stream">图片流</param>
        /// <param name="thumbnailPath">缩略图保存路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <param name="mode">压缩模式（HW：无操作；W：根据输入宽度缩放；H：根据输入高度缩放；Cut：根据输入高和宽等比缩放）</param>
        /// <param name="imageType">保存图片格式（Gif，Jpg，Bmp，Png）</param>
        /// <returns>返回缩略图路径</returns>
        public static string MakeThumbnail(Stream stream, string thumbnailPath, int width, int height, string mode, string imageType)
        {
            Image originalImage = Image.FromStream(stream);
            int towidth = width;
            int toheight = height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            switch (mode)
            {
                case "HW":
                    break;
                case "W":
                    toheight = originalImage.Height * width / originalImage.Width;
                    break;
                case "H":
                    towidth = originalImage.Width * height / originalImage.Height;
                    break;
                case "Cut":
                    if ((double)originalImage.Width / (double)originalImage.Height > (double)towidth / (double)toheight)
                    {
                        oh = originalImage.Height;
                        ow = originalImage.Height * towidth / toheight;
                        y = 0;
                        x = (originalImage.Width - ow) / 2;
                    }
                    else
                    {
                        ow = originalImage.Width;
                        oh = originalImage.Width * height / towidth;
                        x = 0;
                        y = (originalImage.Height - oh) / 2;
                    }
                    break;
                default:
                    break;
            }
            //新建一个bmp图片 
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板 
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);

            try
            {
                string strSize = "_" + towidth.ToString() + "_" + toheight.ToString();
                thumbnailPath = thumbnailPath + strSize;
                string strSavePath = FileHelper.UrlToPath(thumbnailPath);
                switch (imageType.ToLower())
                {
                    case "gif":
                        thumbnailPath += ".gif";
                        strSavePath += ".gif";
                        bitmap.Save(strSavePath, System.Drawing.Imaging.ImageFormat.Gif);
                        break;
                    case "jpg":
                        thumbnailPath += ".jpg";
                        strSavePath += ".jpg";
                        bitmap.Save(strSavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    case "bmp":
                        thumbnailPath += ".bmp";
                        strSavePath += ".bmp";
                        bitmap.Save(strSavePath, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case "png":
                        thumbnailPath += ".png";
                        strSavePath += ".png";
                        bitmap.Save(strSavePath, System.Drawing.Imaging.ImageFormat.Png);
                        break;
                    default:
                        thumbnailPath += ".jpg";
                        strSavePath += ".jpg";
                        bitmap.Save(strSavePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                }
            }
            catch (Exception)
            {
                thumbnailPath = string.Empty;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }
            return thumbnailPath;
        }

        public static bool SaveOriginImg(Stream stream,string savePath)
        {
          
            var image = Image.FromStream(stream);
            try
            {
                savePath = FileHelper.UrlToPath(savePath);
                image.Save(savePath);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                stream.Close();
                image.Dispose();
            }

           
        }

        #endregion

        #region 压缩图片
        /// <summary>
        /// 压缩图片后保存
        /// </summary>
        /// <param name="stream">图片流</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="flag">设置压缩的比例1-100（越大越好）</param>
        /// <returns></returns>
        public static bool CompressImage(Stream stream,string savePath,int flag)
        {
            savePath = FileHelper.UrlToPath(savePath);
            Image originalImage = Image.FromStream(stream);
            int towidth = originalImage.Width ;
            int toheight = originalImage.Height;
            int x = 0;
            int y = 0;
            int ow = originalImage.Width;
            int oh = originalImage.Height;
            
            //新建一个bmp图片 
            Image bitmap = new System.Drawing.Bitmap(towidth, toheight);
            //新建一个画板 
            Graphics g = System.Drawing.Graphics.FromImage(bitmap);
            //设置高质量插值法 
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //设置高质量,低速度呈现平滑程度 
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //清空画布并以透明背景色填充 
            g.Clear(Color.Transparent);
            //在指定位置并且按指定大小绘制原图片的指定部分 
            g.DrawImage(originalImage, new Rectangle(0, 0, towidth, toheight), new Rectangle(x, y, ow, oh), GraphicsUnit.Pixel);

            try
            {
                bitmap.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
                return true;
            }
            catch (System.Exception  )
            {
                return false;
            }
            finally
            {
                originalImage.Dispose();
                bitmap.Dispose();
                g.Dispose();
            }


            
        }

         
        /// <summary> 
        /// 按照比例缩小图片 
        /// </summary> 
        /// <param name="srcImage">要缩小的图片</param> 
        /// <param name="percent">缩小比例</param> 
        /// <returns>缩小后的结果</returns> 
        public static bool PercentImage(Stream stream, double percent,string savePath) 
        {
             Image srcImage = Image.FromStream(stream);
             savePath = FileHelper.UrlToPath(savePath);
             // 缩小后的高度 
             int newH = int.Parse(Math.Round(srcImage.Height * percent).ToString()); 
             // 缩小后的宽度 
             int newW = int.Parse(Math.Round(srcImage.Width * percent).ToString()); 
             try 
             { 
             // 要保存到的图片 
             Bitmap b = new Bitmap(newW, newH); 
             Graphics g = Graphics.FromImage(b); 
             // 插值算法的质量 
             g.InterpolationMode = InterpolationMode.Default; 
             g.DrawImage(srcImage, new Rectangle(0, 0, newW, newH), new Rectangle(0, 0, srcImage.Width, srcImage.Height), GraphicsUnit.Pixel); 
             g.Dispose();
             b.Save(savePath, System.Drawing.Imaging.ImageFormat.Png);
             return true; 
             } 
             catch (Exception) 
             { 
               return false; 
             } 
         }
        /// <summary> 
        /// jpeg图片压缩 
        /// </summary> 
        /// <param name="sFile"></param> 
        /// <param name="outPath"></param> 
        /// <param name="flag"></param> 
        /// <returns></returns> 
        public static bool GetPicThumbnail(Stream stream,  string savePath, int flag)
        {
            System.Drawing.Image iSource = System.Drawing.Image.FromStream(stream);
            savePath = FileHelper.UrlToPath(savePath);
            ImageFormat tFormat = iSource.RawFormat;
            //以下代码为保存图片时，设置压缩质量 
            EncoderParameters ep = new EncoderParameters();
            long[] qy = new long[1];
            qy[0] = flag;//设置压缩的比例1-100 
            EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
            ep.Param[0] = eParam;
            try
            {
                ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                ImageCodecInfo jpegICIinfo = null;
                for (int x = 0; x < arrayICI.Length; x++)
                {
                    if (arrayICI[x].FormatDescription.Equals("JPEG"))
                    {
                        jpegICIinfo = arrayICI[x];
                        break;
                    }
                }
                if (jpegICIinfo != null)
                {
                    iSource.Save(savePath, jpegICIinfo, ep);//dFile是压缩后的新路径 
                }
                else
                {
                    iSource.Save(savePath, tFormat);
                }
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                iSource.Dispose();
                iSource.Dispose();
            }
        }

         

        #endregion

        #region 上传Base64String格式图片
        /// <summary>
        /// Base64String
        /// </summary>
        /// <param name="strImage">Base64String格式图片</param>
        /// <param name="savePath">保存路径</param>
        /// <param name="format">图片格式</param>
        /// <returns></returns>
       
        public static bool SaveBase64Image(string strImage, string savePath)
        {
            if (strImage.IndexOf("base64,")>0)
            {
                strImage = strImage.Substring(strImage.IndexOf(',') + 1);
            }
            byte[] buffer = Convert.FromBase64String(strImage);
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            savePath = FileHelper.UrlToPath(savePath);
            try
            {                                                
                image.Save(savePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                ms.Dispose();
                image.Dispose();
            }
            
        }
        #endregion

        #region 生成条形码
        /// <summary>
        /// 生成条形码
        /// </summary>
        /// <param name="savePath"></param>
        /// <param name="strNum"></param>
        /// <returns></returns>
        public static bool CreateBarcode(string savePath, string strNum, int width,int height)
        {
            Barcode barcode = new Barcode();
            //对条码对象的初始化
            barcode.BackColor = Color.White;
            barcode.ForeColor = Color.Black;
            barcode.IncludeLabel = true;
            barcode.Alignment = AlignmentPositions.CENTER;
            barcode.LabelPosition = LabelPositions.BOTTOMCENTER;
            barcode.ImageFormat = ImageFormat.Jpeg;
            Font font = new Font("vardana", 10f);
            barcode.LabelFont = font;
            barcode.Height = height;
            barcode.Width = width;
            Image image = barcode.Encode(TYPE.CODE128, strNum);  
            try
            {                              
                image.Save(FileHelper.UrlToPath(savePath), ImageFormat.Jpeg);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                image.Dispose();
            }
        }
        #endregion

        #region 生成二维码
                
        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="link"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string CreateQRCode(string link,string fileName)
        {
            string strDomain = ConfigurationHelper.GetAppSetting("Domain");
            string strQrCodePath = ConfigurationHelper.GetAppSetting("CouponQRCodePath");
            string strDate = DateTime.Now.Year + "/" + DateTime.Now.Month + "/" + DateTime.Now.Day + "/";
            string strUrl = strDomain + link;
            string savePath = strQrCodePath + strDate + fileName + ".jpg";
            try
            {
                QRCodeEncoder qr = new QRCodeEncoder();
                //编码方式
                qr.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qr.QRCodeScale = 4;//大小(值越大生成的二维码图片像素越高)
                qr.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
                qr.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
                Bitmap bmp = qr.Encode(strUrl);
                bmp.Save(FileHelper.UrlToPath(savePath));
                return savePath;
            }
            catch (Exception)
            {                 
                return string.Empty;
            }
        }

        /// <summary>
        /// 生成二维码
        /// </summary>
        /// <param name="imgName">要生成的二维码文件名(不含后缀)</param>
        /// <param name="imgPath">二维码图片要保存的根路径</param>
        /// <param name="link">二维码要跳转的链接，如：/Home/Index</param>
        /// <param name="QRCodeScale">值越大生成的二维码图片像素越高,默认为4</param>
        /// <returns>成功返回二维码文件路径，失败返回null</returns>
        public static string CreateQRCode(string imgName,string imgPath, string link, int QRCodeScale = 4)
        {
            if (imgName.IsNullOrEmpty() || imgPath.IsNullOrEmpty() || link.IsNullOrEmpty())
                return null;
            //string strDomain = ConfigurationHelper.GetAppSetting("WebUrl");
            //string strUrl = strDomain + link;
            string strUrl = link;
            string savePath = imgPath + imgName + ".jpg";
            try
            {
                QRCodeEncoder qr = new QRCodeEncoder();
                //编码方式
                qr.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qr.QRCodeScale = QRCodeScale;//大小(值越大生成的二维码图片像素越高)
                qr.QRCodeVersion = 0;//版本(注意：设置为0主要是防止编码的字符串太长时发生错误)
                qr.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;//错误效验、错误更正(有4个等级)
                Bitmap img = qr.Encode(strUrl);
                img = QrCodeRemoveBorder(img);
                img = QrCodeImageAppendLogo(img);
                img.Save(FileHelper.UrlToPath(savePath), ImageFormat.Jpeg);
                return savePath;
            }
            catch (Exception)
            {
                return null;
            }
        }
        /// <summary>
        /// 二维码添加Logo
        /// </summary>
        /// <param name="image"></param>
        /// <param name="logoScale">logo所占比例</param>
        /// <returns></returns>
        public static Bitmap QrCodeImageAppendLogo(Bitmap qrImage, double logoScale = 0.125)
        {
            try
            {
                Bitmap logoImage = new Bitmap(HostingEnvironment.MapPath("/Content/Images/logo-_03.png"));
                
                ImageUtility imageUti = new ImageUtility();
                return imageUti.MergeQrImg(qrImage, logoImage, logoScale);
            }
            catch (Exception)
            {
                throw new Exception("二维码添加Logo失败");
            }
        }
        /// <summary>
        /// 去除生成的二维码右边和下边的白边
        /// </summary>
        /// <returns></returns>
        public static Bitmap QrCodeRemoveBorder(Bitmap qrImage,int pixel=2)
        {
            var newWidth = qrImage.Width - pixel;
            var newHeight = qrImage.Height - pixel;
            Rectangle rectDesc = new Rectangle(0, 0, newWidth, newWidth);
            Bitmap outImg = new Bitmap(newWidth, newHeight);
            using (Graphics g = Graphics.FromImage(outImg))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.DrawImage(qrImage, rectDesc, rectDesc, GraphicsUnit.Pixel);
            }
            return outImg;
        }

        #endregion

        public static string UploadImage(string strImage, string savePath,string fileName,string suffix)
        {
            byte[] buffer = Convert.FromBase64String(strImage);
            MemoryStream ms = new MemoryStream(buffer);
            Image image = Image.FromStream(ms);
            savePath = savePath + fileName + "_" + image.Width.ToString() + "_" + image.Height.ToString() + suffix;            
            try
            {
                image.Save(FileHelper.UrlToPath(savePath));
                return savePath;
            }
            catch (Exception)
            {
                return string.Empty;
            }
            finally
            {
                ms.Dispose();
                image.Dispose();
            }
        }
    }

    public class ImageUtility
    {
        #region 合并用户QR图片和用户头像
        /// <summary>
        /// 合并用户QR图片和用户头像
        /// </summary>
        /// <param name="qrImg">QR图片</param>
        /// <param name="headerImg">用户头像</param>
        /// <param name="Scale">头像所占比例</param>
        /// <returns></returns>
        public Bitmap MergeQrImg(Bitmap qrImg, Bitmap headerImg, double Scale = 0.125)
        {
            int margin = 10;
            float dpix = qrImg.HorizontalResolution;
            float dpiy = qrImg.VerticalResolution;
            var _newWidth = (10 * qrImg.Width - 46 * margin) * 1.0f / 46;
            var _headerImg = ZoomPic(headerImg, Scale);//_newWidth / headerImg.Width
            //处理头像
            int newImgWidth = _headerImg.Width + margin;
            Bitmap headerBgImg = new Bitmap(newImgWidth, newImgWidth);
            headerBgImg.MakeTransparent();
            Graphics g = Graphics.FromImage(headerBgImg);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            Pen p = new Pen(new SolidBrush(Color.White));
            Rectangle rect = new Rectangle(0, 0, newImgWidth - 1, newImgWidth - 1);
            using (GraphicsPath path = CreateRoundedRectanglePath(rect, 7))
            {
                g.DrawPath(p, path);
                g.FillPath(new SolidBrush(Color.White), path);
            }
            //画头像
            Bitmap img1 = new Bitmap(_headerImg.Width, _headerImg.Width);
            Graphics g1 = Graphics.FromImage(img1);
            g1.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g1.SmoothingMode = SmoothingMode.HighQuality;
            g1.Clear(Color.Transparent);
            Pen p1 = new Pen(new SolidBrush(Color.Gray));
            Rectangle rect1 = new Rectangle(0, 0, _headerImg.Width - 1, _headerImg.Width - 1);
            using (GraphicsPath path1 = CreateRoundedRectanglePath(rect1, 7))
            {
                g1.DrawPath(p1, path1);
                TextureBrush brush = new TextureBrush(_headerImg);
                g1.FillPath(brush, path1);
            }
            g1.Dispose();
            PointF center = new PointF((newImgWidth - _headerImg.Width) / 2, (newImgWidth - _headerImg.Height) / 2);
            g.DrawImage(img1, center.X, center.Y, _headerImg.Width, _headerImg.Height);
            g.Dispose();
            Bitmap backgroudImg = new Bitmap(qrImg.Width, qrImg.Height);
            backgroudImg.MakeTransparent();
            backgroudImg.SetResolution(dpix, dpiy);
            headerBgImg.SetResolution(dpix, dpiy);
            Graphics g2 = Graphics.FromImage(backgroudImg);
            g2.Clear(Color.Transparent);
            g2.DrawImage(qrImg, 0, 0);
            PointF center2 = new PointF((qrImg.Width - headerBgImg.Width) / 2, (qrImg.Height - headerBgImg.Height) / 2);
            g2.DrawImage(headerBgImg, center2);
            g2.Dispose();
            return backgroudImg;
        }
        #endregion

        #region 图形处理
        /// <summary>
        /// 创建圆角矩形
        /// </summary>
        /// <param name="rect">区域</param>
        /// <param name="cornerRadius">圆角角度</param>
        /// <returns></returns>
        private GraphicsPath CreateRoundedRectanglePath(Rectangle rect, int cornerRadius)
        {
            //下午重新整理下，圆角矩形
            GraphicsPath roundedRect = new GraphicsPath();
            roundedRect.AddArc(rect.X, rect.Y, cornerRadius * 2, cornerRadius * 2, 180, 90);
            roundedRect.AddLine(rect.X + cornerRadius, rect.Y, rect.Right - cornerRadius * 2, rect.Y);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y, cornerRadius * 2, cornerRadius * 2, 270, 90);
            roundedRect.AddLine(rect.Right, rect.Y + cornerRadius * 2, rect.Right, rect.Y + rect.Height - cornerRadius * 2);
            roundedRect.AddArc(rect.X + rect.Width - cornerRadius * 2, rect.Y + rect.Height - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 0, 90);
            roundedRect.AddLine(rect.Right - cornerRadius * 2, rect.Bottom, rect.X + cornerRadius * 2, rect.Bottom);
            roundedRect.AddArc(rect.X, rect.Bottom - cornerRadius * 2, cornerRadius * 2, cornerRadius * 2, 90, 90);
            roundedRect.AddLine(rect.X, rect.Bottom - cornerRadius * 2, rect.X, rect.Y + cornerRadius * 2);
            roundedRect.CloseFigure();
            return roundedRect;
        }
        /// <summary>
        /// 图片按比例缩放
        /// </summary>
        private Image ZoomPic(Image initImage, double Scale)
        {
            //缩略图宽、高计算
            double newWidth = initImage.Width;
            double newHeight = initImage.Height;
            newWidth = Scale * initImage.Width;
            newHeight = Scale * initImage.Height;
            //生成新图
            //新建一个bmp图片
            System.Drawing.Image newImage = new System.Drawing.Bitmap((int)newWidth, (int)newHeight);
            //新建一个画板
            System.Drawing.Graphics newG = System.Drawing.Graphics.FromImage(newImage);
            //设置质量
            newG.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            newG.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //置背景色
            newG.Clear(Color.Transparent);
            //画图
            newG.DrawImage(initImage, new System.Drawing.Rectangle(0, 0, newImage.Width, newImage.Height), new System.Drawing.Rectangle(0, 0, initImage.Width, initImage.Height), System.Drawing.GraphicsUnit.Pixel);
            newG.Dispose();
            return newImage;
        }
        #endregion
    }

}
