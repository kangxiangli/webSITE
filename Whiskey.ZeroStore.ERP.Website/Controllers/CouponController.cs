using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;

namespace Whiskey.ZeroStore.ERP.Website.Controllers
{
    public class CouponController : Controller
    {
         
        #region 初始化数据层操作对象
        protected  readonly ICouponContract _couponContract;
        protected  readonly ILogger _Logger = LogManager.GetLogger(typeof(CouponController));
        public CouponController(
             ICouponContract couponContract)
        {            
            _couponContract = couponContract;            
        }
        #endregion

        #region 扫描优惠卷二维码接口
        /// <summary>
        /// 扫描优惠卷二维码接口
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public JsonResult Get()
        {             
            try
            {
                string strNum = Request["num"];
                string strMemberId = Request["MemberId"];
                if (string.IsNullOrEmpty(strNum)) return Json(new OperationResult(OperationResultType.Error, "请扫描二维码"), JsonRequestBehavior.AllowGet);
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "请先登录零时尚APP"), JsonRequestBehavior.AllowGet);
                int memberId = int.Parse(strMemberId);
                var result = _couponContract.Get(strNum,memberId);
                return Json(result,JsonRequestBehavior.AllowGet);                
            }
            catch (Exception ex) 
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"), JsonRequestBehavior.AllowGet);
            }
            
        }
        #endregion

        #region 测试
               
        public ActionResult Test()
        {
            string path= System.IO.File.ReadAllText(FileHelper.UrlToPath(@"/App_Data/ImagePath.xml"));
            Uri mUri = new Uri(@"http://ww3.sinaimg.cn/thumbnail/673c0421jw1e9a6au7h5kj218g0rsn23.jpg");
            HttpWebRequest mRequest = (HttpWebRequest)WebRequest.Create(mUri);
            mRequest.Method = "GET";
            mRequest.Timeout = 200;
            mRequest.ContentType = "text/html;charset=utf-8";
            
            HttpWebResponse mResponse = (HttpWebResponse)mRequest.GetResponse();
            Stream mStream = mResponse.GetResponseStream();
            string aSize;
            aSize = (mResponse.ContentLength / 1024).ToString() + "KB";
            Image mImage = Image.FromStream(mStream);
            string aLength = mImage.Width.ToString() + "x" + mImage.Height.ToString();
            mStream.Close();
            StringBuilder sb = new StringBuilder("123789746418");
            sb=sb.Replace("123", "sss");
            string str1 = sb.ToString();
             
            return View();
        }
        #endregion

        #region 使用优惠券

        /// <summary>
        /// 使用优惠券
        /// </summary>
        /// <param name="Number"></param>
        /// <returns></returns>
        public JsonResult Use(string Number,int MemberId)
        {
            OperationResult oper = _couponContract.Use(Number, MemberId);
            return Json(oper);
        }
        #endregion
    }
}