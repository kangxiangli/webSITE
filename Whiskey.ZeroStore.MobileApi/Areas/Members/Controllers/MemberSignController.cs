using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.MobileApi.Extensions.Attribute;
using Whiskey.Utility.Class;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.MobileApi.Controllers;

namespace Whiskey.ZeroStore.MobileApi.Areas.Members.Controllers
{
    [License(CheckMode.Verify)]
    public class MemberSignController : BaseController
    {
        #region 声明业务层操作对象

        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberInfoController));
        //声明业务层操作对象
        protected readonly IMemberSignContract _memberSignContract;
        protected readonly ISignRuleContract _signRuleContract;
        protected readonly IPrizeContract _prizeContract;

        //构造函数-初始化业务层操作对象
        public MemberSignController(IMemberSignContract memberSignContract
            , ISignRuleContract _signRuleContract
            , IPrizeContract _prizeContract
            )
        {
            _memberSignContract = memberSignContract;
            this._signRuleContract = _signRuleContract;
            this._prizeContract = _prizeContract;
        }
        #endregion

        #region 签到
                
        [HttpPost]
        public JsonResult Sign()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                MemberSignDto dto = new MemberSignDto() { 
                  MemberId=memberId,
                  SignTime=DateTime.Now,
                };
                var result =  _memberSignContract.Insert(dto);
                return Json(result);
            }
            catch (Exception)
            {
                OperationResult oper = new OperationResult(OperationResultType.Error, "服务器忙，请稍后");
                return Json(oper);
            }
        }
        #endregion

        #region 获取签到列表
        /// <summary>
        /// 获取签到列表
        /// </summary>
        /// <returns></returns>
        public JsonResult Get()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);                
                DateTime date = DateTime.Now.Date;

                DateTime startWeek = date.AddDays(1 - Convert.ToInt32(date.DayOfWeek.ToString("d")));  //本周周一
                //DateTime endWeek = startWeek.AddDays(6);  //本周周日

                var listMemberSign = _memberSignContract.MemberSigns.Where(x => x.SignTime >= startWeek && x.MemberId == memberId).Select(x => x.SignTime).ToList().Select(s => s.Date).ToList();
                List<SignRule> listSignRule = _signRuleContract.SignRules.Where(x => x.IsDeleted == false && x.IsEnabled == true).DistinctBy(b => b.Week).OrderBy(o => o.Week).ToList();
                if (listSignRule.Count < 7)
                {
                    return Json(new OperationResult(OperationResultType.Error, "签到规则不完整"));
                }

                List<dynamic> list = new List<dynamic>();
                foreach (var item in listSignRule)
                {
                    var curday = startWeek.AddDays(item.Week - 1);

                    var Prize = item.PrizeType == (int)PrizeFlag.Score ? (item.Prize?.Score ?? 0) + "" :
                                item.PrizeType == (int)PrizeFlag.Coupon ? (item.Coupon?.CouponName ?? "") + "(券)" : "";

                    var ImgPath = item.PrizeType == (int)PrizeFlag.Score ? item.Prize?.RewardImagePath :
                                  item.PrizeType == (int)PrizeFlag.Coupon ? (item.Coupon?.CouponImagePath ?? "") : "";

                    list.Add(new
                    {
                        //Week = curday.ToString("dddd"),
                        Week = $"第{item.Week}天",
                        IsSigned = listMemberSign.Exists(e => e == curday),
                        item.PrizeType,
                        Prize = Prize,
                        ImgPath = ImgPath.IsNotNullAndEmpty() ? WebUrl + ImgPath : string.Empty
                    });
                }

                var rdata = new
                {
                    IsSigned = listMemberSign.Exists(e => e == date),
                    list = list
                };

                return Json(new OperationResult(OperationResultType.Success, "获取成功", rdata));
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "获取失败，请稍后重试"));
            }
        }
        #endregion

        #region 获取详情
        public JsonResult GetDetail()
        {
            OperationResult oper = new OperationResult(OperationResultType.Success, "没有签到");
            try
            {
                string strMemberId = Request["MemberId"];
                int memberId = int.Parse(strMemberId);
                string strCount = Request["Count"];
                int count= int.Parse(strCount);
                DateTime nowTime = DateTime.Now;
                IQueryable<MemberSign> memberSigns = _memberSignContract.MemberSigns.Where(x => x.MemberId == memberId && x.SignTime.Year == nowTime.Year && x.SignTime.Month == nowTime.Month);
                MemberSign memberSign = memberSigns.OrderBy(x => x.CreatedTime).Skip((count-1)*1).Take(1).FirstOrDefault();
                if (memberSign == null)
                {
                    oper.Message = "没有签到";
                }
                else
                {
                    
                    if(memberSign.CouponId==null)
                    {
                        oper.Message = "什么也没有";                        
                    }
                    else
                    {
                        var data = new
                        {
                            Name=memberSign.Coupon.CouponName,
                            ImagePath=memberSign.Coupon.CouponImagePath,
                        };
                        oper.Message = "获得" + memberSign.Coupon.CouponName+"一张";
                        oper.Data = data;                       
                    }
                    if (memberSign.PrizeId==null)
                    {
                        oper.Message = "什么也没有";  
                    }
                    else
                    {
                        Prize prize = memberSign.Prize;
                        var data = new
                        {
                            Name = prize.PrizeName,
                            ImagePath = prize.RewardImagePath,
                        };
                        oper.Message = "获得" + prize.PrizeName;
                        oper.Data = data;                         
                    }
                }
                return Json(oper);
            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new OperationResult(OperationResultType.Error, "获取失败，请稍后重试"));
            }
        }
        #endregion

    }
}