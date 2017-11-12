using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Authority;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;

namespace Whiskey.ZeroStore.ERP.Services.Extensions.Helper
{
    public static class AdminParamsHelper
    {
        /// <summary>
        /// 定义委托
        /// </summary>
        /// <param name="admin"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public delegate OperationResult CheckParam(Administrator admin, string param);


        #region 校验真是姓名

        /// <summary>
        /// 校验真是姓名
        /// </summary>
        /// <param name="p"></param>
        public static OperationResult CheckRealName(Administrator entity, string realName)
        {
            realName = realName.Trim();
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(realName))
            {
                oper.Message = "请填写真实姓名";
            }
            else
            {
                if (realName.Length > 20 || realName.Length < 1)
                {
                    oper.Message = "字符串范围在1~20之间";
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                    entity.Member.RealName = realName;
                }
            }
            return oper;
        }
        #endregion

        #region 校验手机号码

        /// <summary>
        /// 校验手机号码
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static OperationResult CheckMobilePhone(Administrator entity, string mobilePhone)
        {
            mobilePhone = mobilePhone.Trim();
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(mobilePhone))
            {
                oper.Message = "请填写手机号码";
            }
            else
            {
                string strReg = "(1(([3587][0-9])|(47)|[8][0126789]))\\d{8}$";
                if (Regex.IsMatch(mobilePhone, strReg))
                {
                    oper.ResultType = OperationResultType.Success;
                    entity.Member.MobilePhone = mobilePhone;
                }
                else
                {
                    oper.Message = "请填写正确的手机号码";
                }
            }
            return oper;
        }
        #endregion

        #region 校验参数

        /// <summary>
        /// 校验参数
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="paras"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public static OperationResult CheckParams(Administrator entity, string keyWord, AdminUpdateFlag flag)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            CheckParam checkParam = null;
            switch (flag)
            {
                case AdminUpdateFlag.RealName:
                    checkParam = CheckRealName;
                    break;
                case AdminUpdateFlag.MobilePhone:
                    checkParam = CheckMobilePhone;
                    break;
                case AdminUpdateFlag.Gender:
                    checkParam = CheckGender;
                    break;
                case AdminUpdateFlag.Email:
                    checkParam = CheckEmail;
                    break;
                case AdminUpdateFlag.MacAddress:
                    checkParam = CheckMacAddress;
                    break;
                default:
                    break;
            }
            if (checkParam != null)
            {
                oper = checkParam(entity, keyWord);
            }
            return oper;
        }






        #endregion

        #region 校验性别
        /// <summary>
        /// 校验性别
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="gender"></param>
        /// <returns></returns>
        public static OperationResult CheckGender(Administrator entity, string strGender)
        {
            int gender = int.Parse(strGender);
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (gender == (int)GenderFlag.Male || gender == (int)GenderFlag.Female)
            {
                oper.ResultType = OperationResultType.Success;
                entity.Member.Gender = gender;
            }
            else
            {
                oper.Message = "请选择性别";
            }
            return oper;
        }
        #endregion

        #region 校验邮箱
        /// <summary>
        /// 校验邮箱
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public static OperationResult CheckEmail(Administrator entity, string email)
        {
            email = email.Trim();
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(email))
            {
                oper.Message = "请填写邮箱";
            }
            else
            {
                if (email.Length > 32)
                {
                    oper.Message = "邮箱的最大长度不能超过32个字符";

                }
                else
                {
                    string strPattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$";
                    bool res = Regex.IsMatch(email, strPattern);
                    if (res == true)
                    {
                        entity.Member.Email = email;
                        oper.ResultType = OperationResultType.Success;
                    }
                    else
                    {
                        oper.Message = "邮箱格式不正确";
                    }
                }
            }
            return oper;
        }
        #endregion

        #region 校验MAC
        /// <summary>
        /// 校验MAC
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="macAddress"></param>
        /// <returns></returns>
        public static OperationResult CheckMacAddress(Administrator entity, string macAddress)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(macAddress))
            {
                oper.Message = "请填写Mac地址";
            }
            else
            {
                if (macAddress.Length > 12)
                {
                    oper.Message = "不能超过12个字符";
                }
                else
                {
                    oper.ResultType = OperationResultType.Success;
                    entity.MacAddress = macAddress.ToUpper();
                }
            }
            return oper;
        }
        #endregion


    }
}
