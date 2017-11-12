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
using Whiskey.ZeroStore.MobileApi.Areas.Members.Models;
using Whiskey.Utility.Helper;
using System.Data.Entity;
using System.Threading.Tasks;
using AutoMapper;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberInfoController : Controller
    {
        #region 声明业务层操作对象

        string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberInfoController));
        //声明业务层操作对象
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IMemberLevelContract _memberLevelContract;
        protected readonly IMemberTypeContract _memberTypeContract;
        protected readonly IMemberFigureContract _memberFigureContract;
        protected readonly IProductAttributeContract _productAttributeContract;
        protected readonly IMemberBehaviorContract _memberBehaviorContract;

        //构造函数-初始化业务层操作对象
        public MemberInfoController(IMemberContract memberContract, IStoreContract storeContract, IAdministratorContract adminContract
            , IMemberDepositContract memberDepositContract,
            IMemberLevelContract memberLevelContract,
            IMemberFigureContract memberFigureContract,
            IMemberBehaviorContract memberBehaviorContract,
            IProductAttributeContract productAttributeContract,
            IMemberTypeContract memberTypeContract)
        {
            _memberContract = memberContract;
            _storeContract = storeContract;
            _adminContract = adminContract;
            _memberDepositContract = memberDepositContract;
            _memberLevelContract = memberLevelContract;
            _memberTypeContract = memberTypeContract;
            this._memberFigureContract = memberFigureContract;
            this._productAttributeContract = productAttributeContract;
            this._memberBehaviorContract = memberBehaviorContract;
        }
        #endregion

        #region 添加会员风格测试
        /// <summary>
        /// 添加会员测试
        /// </summary>
        /// <returns></returns>
        //public OperationResult Get(string str1)
        //{
        //    return null;
        //}
        //public OperationResult Post([FromBody]MemberInfo dto)
        //{
        //    return null;
        //}
        #endregion

        #region 获取会员要修改的数据
        [HttpPost]
        public JsonResult GetEdit(int MemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "用户不存在");
            Member member = _memberContract.View(MemberId);
            if (member == null)
            {
                return Json(oper);
            }
            var entity = new
            {
                MemberId = member.Id,
                member.UserPhoto,
                member.MemberName,
                member.MobilePhone,
                member.RealName,
                member.Gender,
                member.Email,
                member.IDCard,
                DateofBirth = member.DateofBirth == null ? string.Empty : ((DateTime)member.DateofBirth).ToString("yyyyMMdd"),
            };
            oper.ResultType = OperationResultType.Success;
            oper.Data = entity;
            return Json(oper);
        }
        #endregion

        #region 修改会员基本资料
        [HttpPost]
        public JsonResult Update(M_Member m_Member)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(m_Member.MemberId);
            member.UpdatedTime = DateTime.Now;
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                oper = CheckPara(m_Member, ref member);
                if (oper.ResultType == OperationResultType.Error)
                {
                    return Json(oper);
                }
                else
                {
                    oper = _memberContract.Update(member);
                    return Json(oper);
                }
            }

        }
        #endregion

        #region 校验参数
        private OperationResult CheckPara(M_Member m_Member, ref Member member)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            string strRealName = m_Member.RealName;
            string strEmail = m_Member.Email;
            string strIDCard = m_Member.IDCard;
            member.DateofBirth = m_Member.DateofBirth;
            member.Gender = m_Member.Gender;
            if (!string.IsNullOrEmpty(strRealName))
            {
                if (strRealName.Length > 10)
                {
                    oper.Message = "长度在10个字符以内";
                    return oper;
                }
                else
                {
                    member.RealName = strRealName;
                }
            }
            if (!string.IsNullOrEmpty(strEmail))
            {
                if (strEmail.Length > 50)
                {
                    oper.Message = "邮箱长度不能超过50个字符";
                    return oper;
                }
                else
                {
                    string strPattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$";
                    bool res = Regex.IsMatch(strEmail, strPattern);
                    if (res == true)
                    {
                        member.Email = strEmail;
                    }
                    else
                    {
                        oper.Message = "请输入正确的邮箱格式";
                        return oper;
                    }
                }
            }
            if (!string.IsNullOrEmpty(strIDCard))
            {
                if (strIDCard.Length == 18)
                {
                    string strPattern = @"^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$";
                    bool res = Regex.IsMatch(strIDCard, strPattern);
                    if (res)
                    {
                        member.IDCard = strIDCard;
                    }
                    else
                    {
                        oper.Message = "请输入正确的身份证号码";
                        return oper;
                    }
                }
                else
                {
                    if (strIDCard.Length == 15)
                    {
                        string strPattern = @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$";
                        bool res = Regex.IsMatch(strIDCard, strPattern);
                        if (res)
                        {
                            member.IDCard = strIDCard;
                        }
                        else
                        {
                            oper.Message = "请输入正确的身份证号码";
                            return oper;
                        }
                    }
                    else
                    {
                        oper.Message = "请输入正确的身份证号码";
                        return oper;
                    }
                }
            }
            oper.ResultType = OperationResultType.Success;
            return oper;
        }
        #endregion

        #region 修改昵称
        [HttpPost]
        public JsonResult UpdateName(int MemberId, string MemberName)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            member.UpdatedTime = DateTime.Now;
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                if (string.IsNullOrEmpty(MemberName))
                {
                    oper.Message = "请填写昵称";
                }
                else
                {
                    MemberName = MemberName.Trim();
                    if (MemberName.Length > 12)
                    {
                        oper.Message = "昵称最大长度不能超过12个字符";
                        return Json(oper);
                    }
                    else
                    {
                        member.MemberName = MemberName;
                    }
                }
            }
            oper = _memberContract.Update(member);
            return Json(oper);
        }
        #endregion

        #region 修改手机号码
        /// <summary>
        /// 修改手机号码
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="MobilePhone"></param>
        /// <returns></returns>
        public JsonResult UpdatePhone(int MemberId, string MobilePhone)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            member.UpdatedTime = DateTime.Now;
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                if (string.IsNullOrEmpty(MobilePhone))
                {
                    oper.Message = "请填写手机号码";
                }
                else
                {
                    MobilePhone = MobilePhone.Trim();
                    if (MobilePhone.Length != 11)
                    {
                        oper.Message = "手机号码最大长度不能超过11个字符";
                        return Json(oper);
                    }
                    else
                    {
                        string strPattern = "^[1][3578][0-9]{9}$";
                        if (Regex.IsMatch(MobilePhone, strPattern))
                        {
                            member.MobilePhone = MobilePhone;
                        }
                        else
                        {
                            oper.Message = "请输入正确的手机号码";
                            return Json(oper);
                        }
                    }
                }
            }
            oper = _memberContract.Update(member);
            if (oper.ResultType == OperationResultType.Success)
            {
                Dictionary<int, string> dic = CacheHelper.GetCache("MemberVerify") as Dictionary<int, string>;
                var strcode = member.MobileInfos.OrderByDescending(o => o.Id).Select(s => s.DeviceToken).FirstOrDefault();
                string strCode = $"{member.MobilePhone}{member.UniquelyIdentifies}{member.MemberPass}{strcode}".MD5Hash();
                if (dic == null)
                {
                    dic = new Dictionary<int, string>();
                    dic.Add(member.Id, strCode);
                    CacheHelper.SetCache("MemberVerify", dic);
                }
                else
                {
                    if (!dic.ContainsKey(member.Id))
                    {
                        dic.Add(member.Id, strCode);
                    }
                    else
                    {
                        dic[member.Id] = strCode;
                    }
                    CacheHelper.RemoveAllCache("MemberVerify");
                    CacheHelper.SetCache("MemberVerify", dic);
                }
            }
            return Json(oper);
        }
        #endregion

        #region 修改密码
        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Password"></param>
        /// <returns></returns>
        public JsonResult UpdatePassword(int MemberId, string Password)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            member.UpdatedTime = DateTime.Now;
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                if (string.IsNullOrEmpty(Password))
                {
                    oper.Message = "请填写密码";
                }
                else
                {
                    Password = Password.Trim();
                    if (Password.Length > 16 || Password.Length < 6)
                    {
                        oper.Message = "密码长度限制6-16个字符";
                        return Json(oper);
                    }
                    else
                    {
                        member.MemberPass = Password;
                    }
                }
            }
            oper = _memberContract.Update(member);
            return Json(oper);
        }
        #endregion

        #region 获取会员账号信息
        public JsonResult GetAccount(int MemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                var data = new
                {
                    member.MemberName,
                    member.MobilePhone

                };
                oper.ResultType = OperationResultType.Success;
                oper.Data = data;
                return Json(oper);
            }


        }
        #endregion

        #region 获取会员信息
        public JsonResult GetMemberInfo(int MemberId)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.View(MemberId);
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                var data = new
                {
                    member.RealName,
                    member.Gender,
                    member.Email,
                    member.IDCard,
                    StoreName= member.Store?.StoreName ?? "无",
                    DateofBirth = member.DateofBirth == null ? string.Empty : ((DateTime)member.DateofBirth).ToString("yyyy/MM/dd"),
                };
                oper.ResultType = OperationResultType.Success;
                oper.Data = data;
                return Json(oper);
            }
        }
        #endregion

        #region 获取会员昵称和头像、储值
        public JsonResult GetMemberName(int MemberId, string U_Num)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            Member member = _memberContract.Members.Where(x => x.Id == MemberId).FirstOrDefault();
            string strWebUrl = ConfigurationHelper.GetAppSetting("WebUrl");
            if (member == null)
            {
                oper.Message = "会员不存在";
                return Json(oper);
            }
            else
            {
                //是否是员工
                var isStaff = _adminContract.Administrators.Count(p => p.IsDeleted == false && p.IsEnabled == true && p.MemberId == member.Id) > 0;
                decimal historyPrice = 0;
                var memberDepList = _memberDepositContract.MemberDeposits.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == member.Id);
                if (memberDepList.Count() > 0)
                {
                    foreach (var memberDep in memberDepList)
                    {
                        historyPrice = historyPrice + memberDep.Price;
                    }
                }
                decimal currentLevelPrice = 0;
                decimal nextlevelPrice = 0;
                string jindutiao = "0";
                var memberLevelList = _memberLevelContract.MemberLevels.Where(x =>
                x.IsDeleted == false && x.IsEnabled == true);
                if (member.LevelId != null && member.LevelId != 0)
                {
                    currentLevelPrice = memberLevelList.Where(x => x.Id == member.LevelId).Select(x => x.UpgradeCondition).FirstOrDefault();
                    var nextLevel = memberLevelList.Where(x => x.UpgradeCondition >= historyPrice).ToList();
                    if (nextLevel.Count() > 0)
                    {
                        nextlevelPrice = _memberLevelContract.MemberLevels.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.UpgradeCondition >= historyPrice)
                                                                          .Select(x => x.UpgradeCondition).FirstOrDefault();
                        if (nextlevelPrice - currentLevelPrice > 0)
                        {
                            jindutiao = ((historyPrice - currentLevelPrice) / (nextlevelPrice - currentLevelPrice)).ToString("p3");
                        }
                        else
                        {
                            jindutiao = "100%";
                        }
                    }
                    else
                    {
                        //当前会员等级达到最高等级
                        nextlevelPrice = currentLevelPrice;
                        jindutiao = "100%";
                    }
                }
                else
                {
                    //会员没有等级 默认 无等级
                    currentLevelPrice = 0;
                    nextlevelPrice = 0;
                    jindutiao = "0.00%";

                }
                var LevelName = memberLevelList.Where(x => x.Id == member.LevelId).Select(x => x.LevelName).FirstOrDefault();
                var MemberTypeName = _memberTypeContract.MemberTypes.Where(x => x.Id == member.MemberTypeId).Select(x => x.MemberTypeName).FirstOrDefault();
                var StoreName = "无店铺";
                if (member.StoreId != null)
                {
                    StoreName = _storeContract.Stores.Where(x => x.Id == member.StoreId).Select(x => x.StoreName).FirstOrDefault();
                }
                var data = new
                {
                    member.MemberName,
                    UserPhoto = strWebUrl + member.UserPhoto,
                    member.Balance,
                    member.Score,
                    IsStaff = isStaff ? 1 : 0,
                    StoreName,
                    MemberTypeName,
                    LevelName,
                    currentLevelPrice,
                    nextlevelPrice,
                    historyPrice,
                    jindutiao = jindutiao

                };
                oper.ResultType = OperationResultType.Success;
                oper.Data = data;
                return Json(oper, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 获取会员所属门店信息及总店信息
        [HttpPost]
        public JsonResult GetMemberStore(int MemberId)
        {
            var memberEntity = _memberContract.View(MemberId);
            if (memberEntity == null || memberEntity.IsDeleted || !memberEntity.IsEnabled)
            {
                return Json(new OperationResult(OperationResultType.Error, "会员不存在"), JsonRequestBehavior.AllowGet);
            }
            List<StoreDto> list = new List<StoreDto>();

            // 获取总店store
            var mainStoreId = int.Parse(ConfigurationHelper.GetAppSetting("OnlineStorage"));
            var mainStoreEntity = _storeContract.Stores.FirstOrDefault(store => store.Id == mainStoreId);
            if (mainStoreEntity != null)
            {
                list.Add(AutoMapper.Mapper.Map<StoreDto>(mainStoreEntity));
            }

            // 获取会员所属店铺
            var storeEntity = memberEntity.Store;
            if (storeEntity != null)
            {
                var dto = AutoMapper.Mapper.Map<StoreDto>(storeEntity);
                //判断归属店铺ismain属性是否为true
                if (list.Count > 0 && storeEntity.IsMainStore == true)
                {
                    dto.IsMainStore = false;
                }
                list.Add(dto);
            }

            var data = list.Select(s => new { s.Id, s.StoreName, s.IsMainStore }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);

        }
        #endregion

        #region 修改会员基本资料
        [HttpPost]
        public JsonResult UpdateInfo(int MemberId, string KeyWord, int Flag)
        {
            OperationResult oper = UpdateData(MemberId, KeyWord, Flag);
            return Json(oper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="KeyWord"></param>
        /// <param name="Flag"></param>
        /// <returns></returns>
        private OperationResult UpdateData(int MemberId, string KeyWord, int Flag)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后操作");
            Member member = _memberContract.View(MemberId);
            if (member == null)
            {
                oper.Message = "会员不存在";
                return oper;
            }
            else
            {
                Func<Member, string, OperationResult> func = null;
                switch (Flag)
                {
                    case (int)MemberUpdateFlag.MemberName:
                        func = UpdateMemberName;
                        break;
                    case (int)MemberUpdateFlag.MobilePhone:
                        func = UpdateMobilePhone;
                        break;
                    case (int)MemberUpdateFlag.Password:
                        func = UpdatePass;
                        break;
                    case (int)MemberUpdateFlag.Email:
                        func = UpdateEmail;
                        break;
                    case (int)MemberUpdateFlag.Gender:
                        func = UpdateGender;
                        break;
                    case (int)MemberUpdateFlag.IDCard:
                        func = UpdateIDCard;
                        break;
                    case (int)MemberUpdateFlag.Birthday:
                        func = UpdateBirthday;
                        break;
                    case (int)MemberUpdateFlag.RealName:
                        func = UpdateRealName;
                        break;
                    default:

                        break;
                }
                if (func != null)
                {
                    oper = func(member, KeyWord);
                }

            }
            return oper;
        }






        #endregion

        #region 更新会员昵称
        /// <summary>
        /// 更新会员昵称
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="memberName"></param>
        /// <returns></returns>
        private OperationResult UpdateMemberName(Member member, string memberName)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试");
            if (string.IsNullOrEmpty(memberName))
            {
                oper.Message = "请输入会员昵称";
                return oper;
            }
            else
            {
                memberName = memberName.Trim();
                if (memberName.Length >= 2 && memberName.Length <= 12)
                {
                    member.MemberName = memberName;
                    member.UpdatedTime = DateTime.Now;
                    oper = _memberContract.Update(member);
                    return oper;
                }
                else
                {
                    oper.Message = "会员昵称长度在2~12之间";
                    return oper;
                }
            }
        }
        #endregion

        #region 更新手机号码
        /// <summary>
        /// 更新手机号码
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        private OperationResult UpdateMobilePhone(Member member, string mobilePhone)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍候访问");
            if (string.IsNullOrEmpty(mobilePhone))
            {
                oper.Message = "请填写手机号码";
                return oper;
            }
            else
            {
                mobilePhone = mobilePhone.Trim();
                if (mobilePhone.Length != 11)
                {
                    oper.Message = "手机号码最大长度不能超过11个字符";
                    return oper;
                }
                else
                {

                    string strPattern = "^[1][3578][0-9]{9}$";
                    if (Regex.IsMatch(mobilePhone, strPattern))
                    {
                        member.UpdatedTime = DateTime.Now;
                        member.MobilePhone = mobilePhone;
                        oper = _memberContract.Update(member);
                        if (oper.ResultType == OperationResultType.Success)
                        {
                            Dictionary<int, string> dic = CacheHelper.GetCache("MemberVerify") as Dictionary<int, string>;
                            var strcode = member.MobileInfos.OrderByDescending(o => o.Id).Select(s => s.DeviceToken).FirstOrDefault();
                            string strCode = $"{member.MobilePhone}{member.UniquelyIdentifies}{member.MemberPass}{strcode}".MD5Hash();
                            if (dic == null)
                            {
                                dic = new Dictionary<int, string>();
                                dic.Add(member.Id, strCode);
                                CacheHelper.SetCache("MemberVerify", dic);
                            }
                            else
                            {
                                if (!dic.ContainsKey(member.Id))
                                {
                                    dic.Add(member.Id, strCode);
                                }
                                else
                                {
                                    dic[member.Id] = strCode;
                                }
                                CacheHelper.RemoveAllCache("MemberVerify");
                                CacheHelper.SetCache("MemberVerify", dic);
                            }
                        }
                        return oper;
                    }
                    else
                    {
                        oper.Message = "请输入正确的手机号码";
                        return oper;
                    }
                }
            }
        }
        #endregion

        #region 更新密码
        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="KeyWord"></param>
        /// <returns></returns>
        private OperationResult UpdatePass(Member member, string password)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(password))
            {
                oper.Message = "请填写密码";
                return oper;
            }
            else
            {

                if (password.Length > 16 || password.Length < 6)
                {
                    oper.Message = "密码长度6-16个字符";
                    return oper;
                }
                else
                {
                    member.UpdatedTime = DateTime.Now;
                    member.MemberPass = password.MD5Hash();
                    oper = _memberContract.Update(member);
                    return oper;
                }
            }
        }
        #endregion

        #region 更新邮件
        /// <summary>
        /// 更新邮件
        /// </summary>
        /// <param name="member"></param>
        /// <param name="KeyWord"></param>
        /// <returns></returns>
        private OperationResult UpdateEmail(Member member, string email)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(email))
            {
                if (email.Length > 50)
                {
                    oper.Message = "邮箱长度不能超过50个字符";
                    return oper;
                }
                else
                {
                    string strPattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$";
                    bool res = Regex.IsMatch(email, strPattern);
                    if (res == true)
                    {
                        member.Email = email;
                        member.UpdatedTime = DateTime.Now;
                        oper = _memberContract.Update(member);
                        return oper;
                    }
                    else
                    {
                        oper.Message = "请输入正确的邮箱格式";
                        return oper;
                    }
                }
            }
            else
            {
                oper.Message = "邮箱不能为空";
                return oper;
            }
        }
        #endregion

        #region 更新性别
        /// <summary>
        /// 更新性别
        /// </summary>
        /// <param name="member"></param>
        /// <param name="Keyword"></param>
        /// <returns></returns>
        private OperationResult UpdateGender(Member member, string strGender)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(strGender))
            {
                oper.Message = "请选择性别";
                return oper;
            }
            else
            {
                int gender = 0;
                bool res = int.TryParse(strGender, out gender);
                if (res)
                {
                    if (gender == (int)GenderFlag.Female || gender == (int)GenderFlag.Male)
                    {
                        member.Gender = gender;
                        member.UpdatedTime = DateTime.Now;
                        oper = _memberContract.Update(member);
                        return oper;
                    }
                    else
                    {
                        oper.Message = "服务器忙，请稍候访问";
                        return oper;
                    }
                }
                else
                {
                    oper.Message = "操作异常，请稍后访问";
                    return oper;
                }
            }
        }
        #endregion

        #region 更新身份证
        /// <summary>
        /// 更新身份证
        /// </summary>
        /// <param name="member"></param>
        /// <param name="KeyWord"></param>
        /// <returns></returns>
        private OperationResult UpdateIDCard(Member member, string IDCard)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(IDCard))
            {
                IDCard = IDCard.Trim();
                if (IDCard.Length == 18)
                {
                    string strPattern = @"^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}([0-9]|X)$";
                    bool res = Regex.IsMatch(IDCard, strPattern);
                    if (res)
                    {
                        member.IDCard = IDCard;
                        member.UpdatedTime = DateTime.Now;
                        oper = _memberContract.Update(member);
                        return oper;
                    }
                    else
                    {
                        oper.Message = "请输入正确的身份证号码";
                        return oper;
                    }
                }
                else
                {
                    if (IDCard.Length == 15)
                    {
                        string strPattern = @"^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$";
                        bool res = Regex.IsMatch(IDCard, strPattern);
                        if (res)
                        {
                            member.IDCard = IDCard;
                            member.UpdatedTime = DateTime.Now;
                            oper = _memberContract.Update(member);
                            return oper;
                        }
                        else
                        {
                            oper.Message = "请输入正确的身份证号码";
                            return oper;
                        }
                    }
                    else
                    {
                        oper.Message = "请输入正确的身份证号码";
                        return oper;
                    }
                }
            }
            else
            {
                oper.Message = "身份证号码不能为空";
                return oper;
            }
        }
        #endregion

        #region 更新出生日期
        /// <summary>
        /// 更新出生日期
        /// </summary>
        /// <param name="member"></param>
        /// <param name="Keyword"></param>
        /// <returns></returns>
        private OperationResult UpdateBirthday(Member member, string strBirthday)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (string.IsNullOrEmpty(strBirthday))
            {
                oper.Message = "请选择出生日期";
            }
            else
            {
                DateTime birthday = DateTime.Now;
                bool res = DateTime.TryParse(strBirthday, out birthday);
                if (res)
                {
                    member.DateofBirth = birthday;
                    member.UpdatedTime = DateTime.Now;
                    oper = _memberContract.Update(member);
                }
                else
                {
                    oper.Message = "请填写正确的出生日期";
                }
            }
            return oper;
        }
        #endregion

        #region 更新真实姓名
        /// <summary>
        /// 更新真实姓名
        /// </summary>
        /// <param name="member"></param>
        /// <param name="realName"></param>
        /// <returns></returns>
        private OperationResult UpdateRealName(Member member, string realName)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(realName))
            {
                realName = realName.Trim();
                if (realName.Length <= 10 && realName.Length >= 1)
                {
                    member.RealName = realName;
                    member.UpdatedTime = DateTime.Now;
                    oper = _memberContract.Update(member);
                }
                else
                {
                    oper.Message = "长度在10个字符以内";
                }
            }
            else
            {
                oper.Message = "请填写真实姓名";
            }
            return oper;
        }
        #endregion

        #region 获取会员风格

        public async Task<JsonResult> GetMemberAttr(int MemberId)
        {
            var data = await OperationHelper.TryAsync(() =>
               {
                   var modFig = _memberFigureContract.MemberFigures.OrderByDescending(o => o.CreatedTime).FirstOrDefault(w => w.MemberId == MemberId && w.IsEnabled && !w.IsDeleted);
                   if (modFig.IsNull())
                   {
                       return new OperationResult(OperationResultType.QueryNull, "会员不存在");
                   }
                   var queryAttr = _productAttributeContract.ProductAttributes.Where(w => w.AttributeName == "体型" || w.AttributeName == "特型")
                   .Select(s => new
                   {
                       s.AttributeName,
                       Children = s.Children.Select(ss => new MemberInfoAttribute { AttributeName = ss.AttributeName, AttributeImgs = ss.ProductAttributeImage.Select(sss => strWebUrl + sss.OriginalPath) })
                   }).ToList();

                   List<MemberInfoAttribute> texing = new List<MemberInfoAttribute>();
                   List<MemberInfoAttribute> tixing = new List<MemberInfoAttribute>();

                   if (modFig.FigureDes.IsNotNullAndEmpty())
                   {
                       var strTexing = modFig.FigureDes.Split(",", true);
                       texing = queryAttr.Where(w => w.AttributeName == "特型").SelectMany(s => s.Children).Where(w => strTexing.Contains(w.AttributeName))
                                .ToList();
                   }
                   if (modFig.FigureType.IsNotNullAndEmpty())
                   {
                       var strTixing = modFig.FigureType.Split(",", true).Select(s => s + "型");//库中缺少 （型字）
                       tixing = queryAttr.Where(w => w.AttributeName == "体型").SelectMany(s => s.Children).Where(w => strTixing.Contains(w.AttributeName))
                                .ToList();
                   }

                   var rdata = new
                   {
                       MemberFigure = new
                       {
                           modFig.FigureType,
                           modFig.Height,
                           modFig.Weight,
                           modFig.Shoulder,
                           modFig.Bust,
                           modFig.Waistline,
                           modFig.Hips,
                           FigureDes = modFig.FigureDes?.Split(",", true)
                       },
                       Attr_TeXing = texing,
                       Attr_TiXing = tixing
                   };

                   return OperationHelper.ReturnOperationResult(true, "", rdata);
               }, ex =>
               {
                   return OperationHelper.ReturnOperationExceptionResult(ex, "服务器忙，请稍后访问", true);
               });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        public class MemberInfoAttribute
        {
            public MemberInfoAttribute()
            {
                AttributeImgs = new List<string>();
            }
            public string AttributeName { get; set; }
            public IEnumerable<string> AttributeImgs { get; set; }
        }

        #region 会员浏览商品记录

        public JsonResult AddBehaviorRecord(int MemberId, string BigProNum)
        {
            var data = _memberBehaviorContract.AddBehaviorRecord(MemberId, BigProNum);
            return Json(data);
        }

        public JsonResult RelatedRecommend(int MemberId, int StoreId, int Count = 10)
        {
            var data = _memberBehaviorContract.RelatedRecommend(MemberId, StoreId, Count);
            return Json(data);
        }
        public JsonResult GetBehaviourRecord(int MemberId, int PageIndex = 1, int PageSize = 10)
        {
            var data = _memberBehaviorContract.GetBehaviourRecord(MemberId, PageIndex, PageSize);
            return Json(data);
        }

        #endregion
    }
}
