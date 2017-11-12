using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Secutiry;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.WebMember.Controllers
{
    public class VerifyCodeController : BaseController
    {

        #region 初始化业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(VerifyCodeController));
        //声明业务层操作对象
        protected readonly IMemberContract _memberContract;
         
        //构造函数-初始化业务层操作对象
        public VerifyCodeController(IMemberContract memberContract)
        {
            _memberContract = memberContract;             
        }
        #endregion

        #region 发送验证码
        [HttpPost]
        public JsonResult Get()
        {
            try
            {
                string strVerifyCodeType = Request["VerifyCodeType"];
                string strPhoneNum = Request["MobilePhone"];
                if (string.IsNullOrEmpty(strVerifyCodeType))
                {
                    return Json(new OperationResult(OperationResultType.Error, "访问服务器异常，请稍后重试")); 
                }
                else
                {
                    int verifyCodeType = int.Parse(strVerifyCodeType);
                    OperationResult operPhoneNum = CheckPhoneNum(strPhoneNum, verifyCodeType);
                    if (operPhoneNum.ResultType == OperationResultType.Success)
                    {
                        string strNum = RandomHelper.GetRandomNum(6);
                        OperationResult operVerifyCode = CheckVerifyCode(strNum, strPhoneNum, verifyCodeType);
                        if (operVerifyCode.ResultType==OperationResultType.Success)
                        {
                            SendVerifyCode(strPhoneNum, strNum);
                            return Json(new OperationResult(OperationResultType.Success, "发送成功"));   
                        }
                        else
                        {
                            return Json(operVerifyCode);
                        }
                    }
                    else
                    {
                        return Json(operPhoneNum);
                    }                                        
                }                                
            }
            catch (Exception ex)
            {
                _Logger.Error(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙,请稍后重试!"));
            }
        }

        #endregion

        #region 校验验证码
        /// <summary>
        /// 校验验证码
        /// </summary>
        /// <param name="strNum">验证码</param>
        /// <param name="strPhoneNum">手机号码</param>
        /// <param name="verifyCodeType">验证码类型</param>
        /// <returns></returns>
        private OperationResult CheckVerifyCode(string strNum, string strPhoneNum, int verifyCodeType)
        {
            string _key = string.Format("_verifycode_{0}_{1}_", verifyCodeType.ToString(), strPhoneNum);

            //设置校验码的有效时间为2分钟
            TimeSpan Timeout = new TimeSpan(0, 2, 0);

            switch (verifyCodeType)
            {
                case (int)VerifyCodeFlag.Register:
                case (int)VerifyCodeFlag.ChangePassword:
                    {
                        object objCode = CacheHelper.GetCache(_key);
                        if (objCode == null)
                        {
                            CacheHelper.SetCache(_key, strNum, Timeout);
                            return new OperationResult(OperationResultType.Success);
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "请求过于频繁，请稍后重试");
                        }
                    }
                default:
                    {
                        return new OperationResult(OperationResultType.Error, "访问服务器异常，请稍后重试");
                    }
            }
        }
        #endregion

        #region 发送短信校验
        /// <summary>
        /// 发送短信校验
        /// </summary>
        /// <param name="strPhoneNum">手机号码</param>
        /// <param name="strNum">验证码</param>
        private void SendVerifyCode(string strPhoneNum, string strNum)
        {
            try
            {
                //短信接口账号
                string strAdminNum = "xiaoruis";
                //短信接口密码32位小写加密
                string strPassWord = HashHelper.GetMd5("0fashioncom");
                string url = "http://sms.dtcms.net/httpapi/?cmd=tx&pass=1&uid=" + strAdminNum + "&pwd=" + strPassWord + "&mobile=" + strPhoneNum + "&content=验证码：" + strNum;
                Uri mUri = new Uri(url);
                HttpWebRequest mRequest = (HttpWebRequest)WebRequest.Create(mUri);
                mRequest.Method = "GET";
                mRequest.ContentType = "application/x-www-form-urlencoded"; ;
                HttpWebResponse response = (HttpWebResponse)mRequest.GetResponse();
                if (response.StatusCode!=HttpStatusCode.OK)
                {
                    _Logger.Error<string>("访问短信接口失败！");
                    _Logger.Error<string>(((int)response.StatusCode).ToString());

                }               
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());                
            }
           
            
        }
        #endregion

        #region 校验手机号码
        /// <summary>
        /// 校验手机号码
        /// </summary>
        /// <param name="strPhoneNum">手机号码</param>
        /// <param name="verifyCodeType">验证码类型</param>
        /// <returns></returns>
        private OperationResult CheckPhoneNum(string strPhoneNum,int verifyCodeType)
        {
            if (string.IsNullOrEmpty(strPhoneNum))
            {
                return new OperationResult(OperationResultType.ValidError, "手机号码不能为空");
            }
            else
            {

                string strReg = "(1(([3587][0-9])|(47)|[8][0126789]))\\d{8}$";
                bool matchRes = Regex.IsMatch(strPhoneNum, strReg);
                if (matchRes)
                {
                    var entity = _memberContract.Members.Where(x => x.MobilePhone == strPhoneNum).FirstOrDefault();
                    if (verifyCodeType == (int)VerifyCodeFlag.Register)
                    {
                        if (entity!=null)
                        {
                            if (entity.IsDeleted==true || entity.IsEnabled==false)
                            {
                                return new OperationResult(OperationResultType.ValidError, "该手机号码已经被禁用");    
                            }
                            else
                            {
                                return new OperationResult(OperationResultType.ValidError, "手机号码已经存在");
                            }
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Success, "可以注册");
                        }
                    }
                    else if(verifyCodeType == (int)VerifyCodeFlag.ChangePassword)
                    {                        
                        if (entity!=null)
                        {
                            if (entity.IsDeleted == true || entity.IsEnabled == false)
                            {
                                return new OperationResult(OperationResultType.ValidError, "该手机号码已经被禁用");
                            }
                            else
                            {
                                return new OperationResult(OperationResultType.Success);
                            }                            
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "该手机号还没有注册");
                        }
                    }
                    else
                    {
                        return new OperationResult(OperationResultType.Error, "获取验证码失败");
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.ValidError, "请输入正确格式的手机号");
                }
            }
        }
        #endregion

    }
}