using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.WebMember.Extensions.Attribute;
using Whiskey.Web.Helper;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.ERP.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.WebMember.Areas.Members.Controllers
{
    // GET: Members/Member
    [License(CheckMode.Verify)]
    public class MemberController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberController));
        protected readonly IMemberContract _memberContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberFaceContract _MemberFaceContract;
        protected readonly IMemberBehaviorContract _MemberBehaviorContract;

        public MemberController(
            IMemberContract _memberContract,
            IStoreContract _storeContract,
            IMemberFaceContract _MemberFaceContract,
            IMemberBehaviorContract _MemberBehaviorContract
            )
        {
            this._memberContract = _memberContract;
            this._storeContract = _storeContract;
            this._MemberFaceContract = _MemberFaceContract;
            this._MemberBehaviorContract = _MemberBehaviorContract;
        }

        [Layout]
        [NavInfo]
        public ActionResult Index()
        {
            return View();
        }

        #region 获取会员信息

        #region 基本信息

        public JsonResult GetMemberInfo()
        {
            var data = OperationHelper.Try((opera) =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                var dto = _memberContract.View(AuthorityMemberHelper.OperatorId.Value);
                var rdata = new
                {
                    MemberId = dto.Id,
                    dto.RealName,
                    dto.MemberName,
                    dto.MobilePhone,
                    Gender = dto.Gender,
                    dto.Email,
                    dto.IDCard,
                    DateofBirth = dto.DateofBirth?.ToString("yyyy/MM/dd") ?? string.Empty,
                    StoreName = dto.Store?.StoreName ?? "无",
                    dto.MemberType.MemberTypeName,
                };

                return OperationHelper.ReturnOperationResult(true, "", rdata);

            }, "获取个人信息");

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 扩展信息

        public JsonResult GetMemberInfoExt()
        {
            var data = OperationHelper.Try((opera) =>
            {
                OperationResult oper = new OperationResult(OperationResultType.Error);
                var dto = _memberContract.View(AuthorityMemberHelper.OperatorId.Value);
                var rdata = new
                {
                    dto.Balance,
                    dto.Score,
                    BuyCount = 0,//购买记录数，现在还没有
                    CartCount = 0,//购物车数，现在没有
                };

                return OperationHelper.ReturnOperationResult(true, "", rdata);

            }, "获取个人信息");

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

        #region 获取归属店铺列表

        public async Task<ActionResult> GetStoreList(int PageIndex = 1, int PageSize = 10)
        {
            var query = _storeContract.Stores.Where(w => w.IsEnabled && !w.IsDeleted && w.StoreType.TypeName.Contains("直营店"));
            var data = await OperationHelper.TryAsync(() =>
            {
                var count = 0;

                count = query.Count();
                double page = (double)count / PageSize;
                int totalPage = (int)Math.Ceiling(page);
                var list = query.OrderByDescending(x => x.CreatedTime).Skip((PageIndex - 1) * PageSize).Take(PageSize).Select(s => new
                {
                    StoreId = s.Id,
                    s.StoreName,
                    s.StoreType.TypeName,
                    s.Address,
                }).ToList();

                var rdata = new { total = count, totaPage = totalPage, result = list };
                OperationResult oResult = new OperationResult(OperationResultType.Success, "", rdata);
                return oResult;
            }, ex =>
            {
                return OperationHelper.ReturnOperationExceptionResult(ex, "获取店铺列表失败", true);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion
        
        #region 修改归属店铺

        public ActionResult UpdateStore(int StoreId)
        {
            var data = OperationHelper.Try((oper) =>
             {
                 var opera = new OperationResult(OperationResultType.Error);

                 var modMember = _memberContract.View(AuthorityMemberHelper.OperatorId.Value);
                 if (modMember.StoreId == StoreId)
                 {
                     opera.Message = "归属店铺没有变化";
                 }
                 else
                 {
                     if (_storeContract.Stores.Any(w => w.IsEnabled && !w.IsDeleted && w.Id == StoreId))
                     {
                         if (modMember.StoreId.HasValue)
                         {

                             MemberConsume modCon = new MemberConsume();
                             modCon.ConsumeType = Models.Enums.MemberActivityFlag.Score;
                             modCon.ConsumeContext = Models.Enums.MemberConsumeContextEnum.线下消费;
                             modCon.BalanceConsume = 0;
                             modCon.ScoreConsume = modMember.Score;
                             modCon.StoreId = modMember.StoreId.Value;
                             modCon.ConsumeContext = Models.Enums.MemberConsumeContextEnum.系统调整;

                             modMember.MemberConsumes.Add(modCon);

                             modMember.Score = 0;//积分清零
                             var res = _MemberFaceContract.MoveMemberToNewFaceSet(modMember.Id, StoreId);
                             if (res.ResultType != OperationResultType.Success)
                             {
                                 opera.Message = "更新归属店铺失败,人脸信息更新失败";
                                 return opera;
                             }
                         }

                         modMember.StoreId = StoreId;

                         opera = _memberContract.Update(modMember);
                     }
                     else
                     {
                         opera.Message = "归属店铺不存在";
                     }
                 }

                 return opera;
             }, "更新归属店铺");

            return Json(data);
        }

        #endregion

        #region 会员信息修改

        #region 修改会员基本资料

        [HttpPost]
        public JsonResult UpdateInfo(MemberDto memberInfo)
        {
            OperationResult oper = UpdateData(memberInfo);
            return Json(oper);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="KeyWord"></param>
        /// <param name="Flag"></param>
        /// <returns></returns>
        private OperationResult UpdateData(MemberDto memberInfo)
        {
            OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后操作");
            Member member = _memberContract.View(AuthorityMemberHelper.OperatorId.Value);
            if (member == null|| memberInfo.IsNull())
            {
                oper.Message = "会员信息有误";
                return oper;
            }
            else
            {
                #region 昵称校验

                if (memberInfo.MemberName.IsNullOrWhiteSpace())
                {
                    oper.Message = "请输入会员昵称";
                    return oper;
                }
                else
                {
                    var memberName = memberInfo.MemberName.Trim();
                    if (memberName.Length >= 2 && memberName.Length <= 12)
                    {
                        member.MemberName = memberName;
                    }
                    else
                    {
                        oper.Message = "会员昵称长度在2~12之间";
                        return oper;
                    }
                }

                #endregion

                #region 真实名称校验

                if (memberInfo.RealName.IsNotNullAndEmpty())
                {
                    var realName = memberInfo.RealName.Trim();
                    if (realName.Length <= 10 && realName.Length >= 1)
                    {
                        member.RealName = realName;
                    }
                    else
                    {
                        oper.Message = "长度需要在10个字符以内";
                        return oper;
                    }
                }
                else
                {
                    oper.Message = "请填写真实姓名";
                    return oper;
                }

                #endregion

                #region 性别校验

                var gender = memberInfo.Gender;
                if (gender == (int)GenderFlag.Female || gender == (int)GenderFlag.Male)
                {
                    member.Gender = gender;
                }
                else
                {
                    oper.Message = "性别无效";
                    return oper;
                }

                #endregion

                #region 邮箱检验

                if (memberInfo.Email.IsNotNullAndEmpty())
                {
                    if (memberInfo.Email.Length > 50)
                    {
                        oper.Message = "邮箱长度不能超过50个字符";
                        return oper;
                    }
                    else
                    {
                        string strPattern = @"^([a-zA-Z0-9_\.\-])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$";
                        bool res = Regex.IsMatch(memberInfo.Email, strPattern);
                        if (res == true)
                        {
                            member.Email = memberInfo.Email;
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

                #endregion

                #region 密码校验

                if (memberInfo.ResetPass == memberInfo.SecondReset)
                {
                    if (memberInfo.ResetPass.IsNotNullAndEmpty())
                    {
                        if (memberInfo.MemberPass.IsNullOrWhiteSpace())
                        {
                            oper.Message = "原始密码不能为空";
                            return oper;
                        }
                        if (memberInfo.MemberPass.MD5Hash() != member.MemberPass)
                        {
                            oper.Message = "原始密码不正确";
                            return oper;
                        }
                        if (memberInfo.ResetPass.Length > 16 || memberInfo.ResetPass.Length < 6)
                        {
                            oper.Message = "密码长度6-16个字符";
                            return oper;
                        }
                        var strpass = memberInfo.ResetPass.MD5Hash();
                        if (strpass != member.MemberPass)
                        {
                            member.MemberPass = strpass;
                        }
                    }
                }
                else
                {
                    oper.Message = "再次输入的密码不一致";
                    return oper;
                }

                #endregion

                #region 手机号校验

                //if (memberInfo.MobilePhone.IsNullOrWhiteSpace())
                //{
                //    oper.Message = "请填写手机号码";
                //    return oper;
                //}
                //else
                //{
                //    var mobilePhone = memberInfo.MobilePhone.Trim();
                //    if (mobilePhone.Length != 11)
                //    {
                //        oper.Message = "手机号码最大长度不能超过11个字符";
                //        return oper;
                //    }
                //    else
                //    {
                //        string strPattern = "^[1][3578][0-9]{9}$";
                //        if (Regex.IsMatch(mobilePhone, strPattern))
                //        {
                //            member.MobilePhone = mobilePhone;
                //        }
                //        else
                //        {
                //            oper.Message = "请输入正确的手机号码";
                //            return oper;
                //        }
                //    }
                //}

                #endregion
            }

            return  _memberContract.Update(member);
        }

        #endregion

        #endregion

        #region 足迹

        [Layout]
        public ActionResult MyTrack()
        {
            return View();
        }

        public JsonResult GetBehaviourRecord(int PageIndex = 1, int PageSize = 10)
        {
            var data = _MemberBehaviorContract.GetBehaviourRecord(AuthorityMemberHelper.OperatorId.Value, PageIndex, PageSize);
            return Json(data);
        }

        #endregion

        #region 添加商品浏览记录

        public JsonResult AddBehaviorRecord(string BigProNum)
        {
            var data = _MemberBehaviorContract.AddBehaviorRecord(AuthorityMemberHelper.OperatorId.Value, BigProNum);
            return Json(data);
        }

        #endregion

    }
}