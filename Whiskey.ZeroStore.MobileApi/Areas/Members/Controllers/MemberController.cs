using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Web.Helper;
using System.Text.RegularExpressions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Security.Cryptography;
using System.Net;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.Utility.Helper;
using System.Xml;
using Whiskey.ZeroStore.MobileApi.Controllers;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{

    public class MemberController : BaseController
    {
        #region 初始化业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberController));
        //声明业务层操作对象
        protected readonly IMemberContract _memberContract;

        protected readonly IMemberFigureContract _memberFigureContract;

        protected readonly IMobileInfoContract _mobileInfoContract;
        private readonly IMemberIMProfileContract _imContract;
        protected readonly IStoreContract _storeContract;

        protected readonly IMemberConsumeContract _memberConsumeContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;
        protected readonly IOAuthRecordContract _oauthRecordContract;
        //构造函数-初始化业务层操作对象
        public MemberController(IMemberContract memberContract,
            IMobileInfoContract mobileInfoContract,
            IMemberFigureContract memberFigureContract,
            IMemberIMProfileContract imContract,
            IOAuthRecordContract _oauthRecordContract,
            IStoreContract storeContract,
            IMemberConsumeContract memberConsumeContract,
            ICollocationQuestionnaireContract collocationQuestionnaireContract)
        {
            _memberContract = memberContract;
            _mobileInfoContract = mobileInfoContract;
            _memberFigureContract = memberFigureContract;
            _imContract = imContract;
            _storeContract = storeContract;
            _memberConsumeContract = memberConsumeContract;
            _collocationQuestionnaireContract = collocationQuestionnaireContract;
            this._oauthRecordContract = _oauthRecordContract;
        }
        #endregion

        #region APP第三方登录
        /// <summary>
        /// APP第三方登录
        /// </summary>
        /// <returns></returns>

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult ThirdLogin(MobileInfo mobileInfo)
        {
            string strThirdLoginType = Request["ThirdLoginType"];
            string strThirdLoginId = Request["ThirdLoginId"];
            string strMobileType = Request["MobileType"];
            try
            {
                if (string.IsNullOrEmpty(strThirdLoginType) && string.IsNullOrEmpty(strThirdLoginId))
                {
                    return Json(new OperationResult(OperationResultType.Error, "第三方登录异常，请稍后重试"));
                }
                else
                {
                    var thirdLoginType = (ThirdLoginFlag)Enum.Parse(typeof(ThirdLoginFlag), strThirdLoginType);
                    var mobileType = (RegisterFlag)Enum.Parse(typeof(RegisterFlag), strMobileType);
                    var res = _memberContract.ThirdLogin(strThirdLoginId, thirdLoginType, mobileType);
                    return Json(res, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }

        }

        #endregion

        #region 添加会员风格
        [HttpPost]
        public JsonResult AddAttr()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                string strApparelSize = Request["ApparelSize"];
                string strFigureInfo = Request["FigureInfo"];
                string strFigureDescription = Request["FigureDescription"];
                string strPreferenceColor = Request["PreferenceColor"];
                string strDate = Request["Birthday"];
                string strGender = Request["Gender"];
                if (string.IsNullOrEmpty(strMemberId)) return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录"));
                if (string.IsNullOrEmpty(strApparelSize)) return Json(new OperationResult(OperationResultType.Error, "请选择服装尺码"));
                if (string.IsNullOrEmpty(strFigureInfo)) return Json(new OperationResult(OperationResultType.Error, "请选择身材信息"));
                if (string.IsNullOrEmpty(strFigureDescription)) return Json(new OperationResult(OperationResultType.Error, "请选择身材描述"));
                if (string.IsNullOrEmpty(strPreferenceColor)) return Json(new OperationResult(OperationResultType.Error, "请选择颜色"));
                if (string.IsNullOrEmpty(strGender)) return Json(new OperationResult(OperationResultType.Error, "请选择性别"));
                if (string.IsNullOrEmpty(strApparelSize)) return Json(new OperationResult(OperationResultType.Error, "请选择生日"));
                int memberId = 0;
                bool res = int.TryParse(strMemberId, out memberId);
                if (!res)
                {
                    return Json(new OperationResult(OperationResultType.Error, "登录异常，请重新登录"));
                }
                DateTime bir = DateTime.Parse(strDate);
                short gender = short.Parse(strGender);
                string[] figureInfo = strFigureInfo.Split(',');
                short height = short.Parse(figureInfo[0]);
                short weight = short.Parse(figureInfo[1]);
                short shoulder = short.Parse(figureInfo[2]);
                short bust = short.Parse(figureInfo[3]);
                short waistline = short.Parse(figureInfo[4]);
                short hips = short.Parse(figureInfo[5]);
                string figureType = figureInfo[6];
                MemberFigureDto dto = new MemberFigureDto()
                {
                    MemberId = memberId,
                    ApparelSize = strApparelSize,
                    Birthday = bir,
                    Gender = gender,
                    Height = height,
                    Weight = weight,
                    Shoulder = shoulder,
                    Bust = bust,
                    Waistline = waistline,
                    Hips = hips,
                    FigureType = figureType,
                    FigureDes = strFigureDescription,
                    PreferenceColor = strPreferenceColor
                };
                var model = _memberFigureContract.MemberFigures.OrderByDescending(o => o.CreatedTime).FirstOrDefault(w => w.MemberId == memberId && w.IsEnabled && !w.IsDeleted);
                if (model == null)
                {
                    var addRes = _memberFigureContract.Insert(dto);
                    return Json(addRes);
                }
                else
                {
                    dto.Id = model.Id;
                    var addRes = _memberFigureContract.Update(dto);
                    return Json(addRes);
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后访问"));
            }

        }

        #endregion

        #region 会员登录
        [HttpPost]
        public JsonResult Login(string JPushRegistrationId, MobileInfoDto dto = null)
        {
            try
            {
                string strPhoneNum = Request["PhoneNumber"];
                string strPassWord = Request["PassWord"];
                if (string.IsNullOrEmpty(strPhoneNum))
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "请输入手机号"));
                }
                if (dto.IsNull() || dto.DeviceToken.IsNullOrEmpty())
                {
                    dto.DeviceToken = Guid.NewGuid().ToString();
                }
                OperationResult oper = CheckPassWord(strPassWord);
                if (oper.ResultType != OperationResultType.Success)
                {
                    return Json(oper);
                }
                else
                {
                    strPassWord = oper.Data.ToString();
                }
                Member entity = _memberContract.Members.FirstOrDefault(x => x.MobilePhone == strPhoneNum && x.MemberPass == strPassWord);
                if (entity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "手机号码或者密码错误"));
                }
                else
                {
                    if (entity.IsEnabled == false || entity.IsDeleted == true)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "该用户已经被禁用"));
                    }
                    else
                    {
                        if (dto != null)
                        {
                            dto.MemberId = entity.Id;
                            _mobileInfoContract.Insert(dto);
                        }

                        // 缓存jpushId
                        var hashId = RedisCacheHelper.KEY_MEMBER_JPUSH;
                        if (!string.IsNullOrEmpty(JPushRegistrationId))
                        {
                            RedisCacheHelper.SetEntryInHash(hashId, dto.MemberId.ToString(), JPushRegistrationId);
                            _memberContract.RefreshJpushId(entity.Id, JPushRegistrationId);
                        }
                        var memberPushId = RedisCacheHelper.GetValueFromHash(hashId, dto.MemberId.ToString());
                        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
                        Dictionary<int, string> dic = CacheHelper.GetCache("MemberVerify") as Dictionary<int, string>;

                        //string strCode = $"{entity.MobilePhone}{entity.UniquelyIdentifies}{entity.MemberPass}".MD5Hash();
                        string strCode = $"{entity.MobilePhone}{entity.UniquelyIdentifies}{entity.MemberPass}{dto.DeviceToken}".MD5Hash();
                        if (dic == null)
                        {
                            dic = new Dictionary<int, string>();
                            dic.Add(entity.Id, strCode);
                            CacheHelper.SetCache("MemberVerify", dic);
                        }
                        else
                        {
                            if (!dic.ContainsKey(entity.Id))
                                dic.Add(entity.Id, strCode);
                            else
                                dic[entity.Id] = strCode;
                        }

                        //获取token
                        var tokenStr = _imContract.GetToken(entity.Id.ToString(), entity.MemberName, strWebUrl + entity.UserPhoto);

                        bool IsTest = false;
                        if (entity.MemberFigures != null && entity.MemberFigures.Count > 0)
                        {
                            IsTest = true;
                        }
                        var data = new
                        {
                            MemberId = entity.Id,
                            U_Num = strCode,
                            UserPhoto = strWebUrl + entity.UserPhoto,
                            IsTest,
                            Token = tokenStr,
                            entity.MemberName,
                            RegistrationId = memberPushId,
                            isDetailTest = _collocationQuestionnaireContract.Entities.Count(c => c.MemberId == entity.Id && c.IsEnabled && !c.IsDeleted) > 0 ? 1 : 0
                        };
                        return Json(new OperationResult(OperationResultType.Success, "用户登录成功", data));
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试" + ex.Message));
            }

        }

        [HttpPost]
        [License(CheckMode.Check)]
        public ActionResult ConfirmLogin(string MemberId, int? isConfirm)
        {
            if (!isConfirm.HasValue)
            {
                return Json(OperationResult.Error("参数错误"));
            }
            if (isConfirm.Value != 1 && isConfirm.Value != 0)
            {
                return Json(OperationResult.Error("参数值错误"));
            }

            if (string.IsNullOrEmpty(MemberId))
            {
                return Json(OperationResult.Error("参数错误"));
            }

            var key = RedisCacheHelper.KEY_MEMBER_LOGIN_STAT_PREFIX + MemberId;
            var res = RedisCacheHelper.Set(key, isConfirm.Value, TimeSpan.FromMinutes(5));
            if (!res)
            {
                return Json(OperationResult.Error("系统错误"));
            }
            return Json(OperationResult.OK());
        }
        #endregion

        #region 注册会员
        [HttpPost]
        public JsonResult Register(MobileInfo mobileInfo)
        {
            try
            {
                string strPhoneNum = Request["PhoneNumber"];
                string strVerifyCode = Request["VerifyCode"];
                string strPassWord = Request["PassWord"];
                //校验手机号码，密码和验证码
                OperationResult resPhoneNum = CheckPhoneNum(strPhoneNum, VerifyCodeFlag.Register);
                if (resPhoneNum.ResultType != OperationResultType.Success)
                {
                    return Json(resPhoneNum);
                }
                OperationResult resPassWord = CheckPassWord(strPassWord, false);
                if (resPassWord.ResultType == OperationResultType.Success)
                {
                    strPassWord = resPassWord.Data.ToString();
                }
                else
                {
                    return Json(resPassWord);
                }
                OperationResult resSecurityCode = CheckVerifyCode(strPhoneNum, strVerifyCode, VerifyCodeFlag.Register);
                if (resSecurityCode.ResultType != OperationResultType.Success)
                {
                    return Json(resSecurityCode);
                }
                IQueryable<Member> listMember = _memberContract.Members;
                MemberDto dto = new MemberDto()
                {
                    RegisterType = mobileInfo.MobileSystem,
                    MobilePhone = strPhoneNum,
                    MemberPass = strPassWord,
                    MemberTypeId = 1,//普通会员                    
                    UserPhoto = "/Content/Images/logo-_03.png",
                };
                dto.MobileInfos.Add(mobileInfo);
                var addRes = _memberContract.Insert(dto);
                if (addRes.ResultType == OperationResultType.Success)
                {
                    Dictionary<int, string> dic = CacheHelper.GetCache("MemberVerify") as Dictionary<int, string>;
                    if (dic == null)
                    {
                        dic = new Dictionary<int, string>();
                    }
                    string strCode = $"{dto.MobilePhone}{dto.UniquelyIdentifies}{dto.MemberPass}{mobileInfo?.DeviceToken}".MD5Hash();
                    Member member = listMember.FirstOrDefault(x => x.MobilePhone == dto.MobilePhone);
                    if (member != null)
                    {
                        dic.Add(member.Id, strCode);
                        CacheHelper.RemoveAllCache("MemberVerify");
                        CacheHelper.SetCache("MemberVerify", dic);
                    }
                }
                return Json(addRes);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }

        /// <summary>
        /// 第三方授权注册或重新绑定手机号
        /// </summary>
        /// <param name="PhoneNumber"></param>
        /// <param name="VerifyCode"></param>
        /// <param name="OpenId"></param>
        /// <param name="mobileInfo"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult RegisterThird(string PhoneNumber, string VerifyCode, string OpenId, MobileInfo mobileInfo, ThirdLoginFlag ThirdLoginFlag = ThirdLoginFlag.WeChat)
        {
            try
            {
                if (PhoneNumber.IsNullOrEmpty() || VerifyCode.IsNullOrEmpty() || OpenId.IsNullOrEmpty())
                    return Json(new OperationResult(OperationResultType.ValidError, "参数无效"));

                string strReg = "(1(([3587][0-9])|(47)|[8][0126789]))\\d{8}$";
                bool matchRes = Regex.IsMatch(PhoneNumber, strReg);
                if (!matchRes)
                {
                    return Json(new OperationResult(OperationResultType.ValidError, "手机号无效"));
                }
                var resCode = CheckVerifyCode(PhoneNumber, VerifyCode, VerifyCodeFlag.Register);
                if (resCode.ResultType != OperationResultType.Success)
                {
                    return Json(resCode);
                }
                if (_oauthRecordContract.Entities.Any(w => w.IsEnabled && !w.IsDeleted && w.OpenId == OpenId))
                {
                    var resResult = new OperationResult(OperationResultType.Error);
                    var query = _memberContract.Members.Where(w => w.IsEnabled && !w.IsDeleted);
                    var modM = query.FirstOrDefault(w => w.MobilePhone == PhoneNumber);
                    if (modM.IsNull())
                    {
                        var dtoM = new MemberDto()
                        {
                            MobilePhone = PhoneNumber,
                            MemberPass = "123456",
                            RegisterType = RegisterFlag.Web,
                            MemberTypeId = 1,//普通会员
                            UserPhoto = "/Content/Images/logo-_03.png",
                            Notes = "来自第三方授权注册",
                            ThirdLoginType = ThirdLoginFlag,
                        };
                        dtoM.MobileInfos.Add(mobileInfo);
                        var addRes = _memberContract.Insert(dtoM);
                        if (addRes.ResultType == OperationResultType.Success)
                        {
                            resResult = new OperationResult(OperationResultType.Success, "注册成功");
                        }
                        else
                        {
                            resResult.Message = "注册失败，请稍后重试";
                        }
                    }
                    else
                    {
                        if (OpenId != modM.ThirdLoginId)
                        {
                            modM.ThirdLoginId = OpenId;
                            modM.ThirdLoginType = ThirdLoginFlag;
                            var updRes = _memberContract.Update(modM);
                            if (updRes.ResultType != OperationResultType.Success)
                            {
                                resResult.Message = "绑定失败，请稍后重试";
                            }
                            else
                            {
                                resResult = new OperationResult(OperationResultType.Success, "绑定成功");
                            }
                        }
                        else
                        {
                            resResult = new OperationResult(OperationResultType.Success, "绑定成功");
                        }
                    }
                    if (resResult.ResultType == OperationResultType.Success)
                    {
                        var da = (from s in _memberContract.Members.Where(w => w.IsEnabled && !w.IsDeleted && w.MobilePhone == PhoneNumber)
                                  let mi = s.MobileInfos.OrderByDescending(o => o.Id).Select(ss => ss.DeviceToken)
                                  let hasDetailTest = _collocationQuestionnaireContract.Entities.Any(c => c.MemberId == s.Id && c.IsEnabled && !c.IsDeleted)
                                  select new
                                  {
                                      s.Id,
                                      s.MobilePhone,
                                      s.UniquelyIdentifies,
                                      s.MemberPass,
                                      s.UserPhoto,
                                      IsTest = s.MemberFigures.Any(a => a.IsEnabled && !a.IsDeleted),
                                      isDetailTest = hasDetailTest,
                                      DeviceToken = mi.FirstOrDefault(),
                                      s.MemberName,
                                  }).FirstOrDefault();
                        string num = $"{da.MobilePhone}{da.UniquelyIdentifies}{da.MemberPass}{da.DeviceToken}".MD5Hash();
                        resResult.Data = new
                        {
                            MemberId = da.Id,
                            U_Num = num,
                            UserPhoto = WebUrl + da.UserPhoto,
                            da.IsTest,
                            da.MemberName,
                            isDetailTest = da.isDetailTest,
                        };
                    }
                    return Json(resResult);
                }
                else
                {
                    return Json(new OperationResult(OperationResultType.Error, "第三方授权失效,请重试"));
                }
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }

        #endregion

        #region 添加用户手机信息
        //private MobileInfoDto AddMobileInfo(string strMobileSystem, string strSystemVersion, string AppVersion, string MobileModel,string)
        //{

        //}
        #endregion

        #region 校验手机号码
        private OperationResult CheckPhoneNum(string strPhoneNum, VerifyCodeFlag verifyCodeFlag)
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
                    if (entity != null)
                    {
                        if (entity.IsDeleted == true && entity.IsEnabled == false)
                        {
                            return new OperationResult(OperationResultType.ValidError, "该手机号码已经被禁用");
                        }
                        else
                        {
                            if (verifyCodeFlag == VerifyCodeFlag.Register)
                            {
                                return new OperationResult(OperationResultType.ValidError, "手机号码已经存在");
                            }
                            else if (verifyCodeFlag == VerifyCodeFlag.ChangePassword)
                            {
                                return new OperationResult(OperationResultType.Success);
                            }
                            else
                            {
                                return new OperationResult(OperationResultType.Error, "获取手机验证失败");
                            }
                        }
                    }
                    else
                    {
                        if (verifyCodeFlag == VerifyCodeFlag.Register)
                        {
                            return new OperationResult(OperationResultType.Success, "手机号码正确");
                        }
                        else if (verifyCodeFlag == VerifyCodeFlag.ChangePassword)
                        {
                            return new OperationResult(OperationResultType.Error, "该手机号码未注册");
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "获取手机验证失败");
                        }
                    }
                }
                else
                {
                    return new OperationResult(OperationResultType.ValidError, "请输入正确格式的手机号");
                }
            }
        }
        #endregion

        #region 校验密码
        private OperationResult CheckPassWord(string strPassWord, bool md5 = true)
        {
            strPassWord = strPassWord.Trim();
            if (string.IsNullOrEmpty(strPassWord))
            {
                return new OperationResult(OperationResultType.ValidError, "请输入密码");
            }
            else
            {
                if (strPassWord.Length < 6 || strPassWord.Length > 20)
                {
                    return new OperationResult(OperationResultType.ValidError, "密码长度6-20个字符");
                }
                else
                {
                    strPassWord = md5 ? strPassWord.MD5Hash() : strPassWord;
                    return new OperationResult(OperationResultType.Success, "密码可以使用", strPassWord);
                }
            }
        }
        #endregion

        #region 校验验证码
        private OperationResult CheckVerifyCode(string strPhoneNum, string strSecurityCode, VerifyCodeFlag verifyCodeFlag)
        {

            if (string.IsNullOrEmpty(strSecurityCode))
            {
                return new OperationResult(OperationResultType.ValidError, "请输入验证码");
            }
            else
            {
                int verifyCodeType = (int)verifyCodeFlag;
                string _key = string.Format("_verifycode_{0}_{1}_", verifyCodeType.ToString(), strPhoneNum);
                string obj = CacheHelper.GetCache(_key) as string;
                if (obj == null)
                {
                    return new OperationResult(OperationResultType.Error, "验证码失效，请重新获取");
                }
                else if (obj != strSecurityCode)
                {
                    return new OperationResult(OperationResultType.Error, "验证码不正确");
                }
                else
                {
                    CacheHelper.RemoveCache(_key);
                    return new OperationResult(OperationResultType.Success, "验证码正常");
                }
            }
        }
        #endregion

        #region 修改密码
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <returns></returns>
        public JsonResult UpdatePassWord()
        {
            try
            {
                string strPhoneNum = Request["PhoneNumber"];
                string strVerifyCode = Request["VerifyCode"];
                string strPassWord = Request["PassWord"];
                OperationResult resPhoneNum = CheckPhoneNum(strPhoneNum, VerifyCodeFlag.ChangePassword);
                if (resPhoneNum.ResultType != OperationResultType.Success)
                {
                    return Json(resPhoneNum, JsonRequestBehavior.AllowGet);
                }
                OperationResult resPassWord = CheckPassWord(strPassWord);
                if (resPassWord.ResultType == OperationResultType.Success)
                {
                    strPassWord = resPassWord.Data.ToString();
                }
                else
                {
                    return Json(resPassWord, JsonRequestBehavior.AllowGet);
                }
                OperationResult resSecurityCode = CheckVerifyCode(strPhoneNum, strVerifyCode, VerifyCodeFlag.ChangePassword);
                if (resSecurityCode.ResultType != OperationResultType.Success)
                {
                    return Json(resSecurityCode, JsonRequestBehavior.AllowGet);
                }
                var res = _memberContract.UpdatePassWord(strPhoneNum, strPassWord);
                return Json(res);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试"));
            }
        }
        #endregion

        public void ReadRole(int MemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            if (member == null)
            {
                oper.Message = "";
            }
            string strPath = @"\xml\Role.xml";
            XmlDocument xmlDoc = new XmlDocument();
            //xmlDoc.Load();
        }

        #region 获取
        //private 
        #endregion

        [HttpPost]
        public ActionResult SwitchAttachStore(int memberId, int newStoreId)
        {
            var res = _memberContract.SwitchMemberStore(memberId, newStoreId);
            return Json(res);
        }

        [HttpPost]
        public ActionResult Logout(int memberId, string JPushRegistrationId)
        {
            try
            {
                var key = RedisCacheHelper.KEY_MEMBER_JPUSH;
                RedisCacheHelper.RemoveEntryFromHash(key, memberId.ToString());

                _memberContract.RemoveJpushId(memberId, JPushRegistrationId);

                Dictionary<int, string> dic = CacheHelper.GetCache("MemberVerify") as Dictionary<int, string>;
                if (dic != null)
                {
                    if (dic.ContainsKey(memberId))
                        dic.Remove(memberId);
                }
                return Json(new OperationResult(OperationResultType.Success, "退出登录成功"));
            }
            catch (Exception e)
            {
                RedisCacheHelper.LogExcepition("api:logout", e.Message + e.StackTrace);
                return Json(new OperationResult(OperationResultType.Success, "退出登录成功"));
            }

        }

    }
}