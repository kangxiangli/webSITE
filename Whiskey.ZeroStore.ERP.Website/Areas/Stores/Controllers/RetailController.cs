using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Whiskey.jpush.api;

//using Microsoft.Ajax.Utilities;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Extensions.License;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Entities;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using XKMath36;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [CheckStoreIsClosed]
    [CheckStoreIsChecking]
    [CheckCookieAttrbute]
    public class RetailController : BaseController
    {
        protected readonly IProductContract _productContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IInventoryContract _inventoryContract;
        protected readonly IStorageContract _storageContract;
        protected readonly ISalesCampaignContract _salesCampaignContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IRetailItemContract _retailItemContract;
        protected readonly IScoreRuleContract _scoreRuleContract;
        protected readonly ICouponContract _couponContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly ICheckerContract _checkerContract;
        protected readonly IStoreActivityContract _storeActivityContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IProductTrackContract _productTrackContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;
        protected readonly ISmsContract _smsContract;

        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(RetailController));
        private readonly static object _objlock = new object();
        public RetailController(IProductContract productContract,
            IInventoryContract inventoryContract,
            IBrandContract brandContract,
            IStorageContract storageContract,
            ISalesCampaignContract salesCampaignContract,
            IMemberContract memberContract,
            ICollocationContract collocationContract,
            IRetailContract retailContract,
            IRetailItemContract retailItemContract,
            IScoreRuleContract scoreRuleContract,
            ICouponContract couponContract,
            IAdministratorContract administratorContract,
            IMemberDepositContract memberDepositContract,
            ICheckerContract checkerContract,
            IStoreActivityContract storeActivityContract,
            IStoreContract storeContract,
            IProductTrackContract productTrackContract,
            IPermissionContract permissionContract,
            IMemberConsumeContract memberConsumeContract,
            ISmsContract smsContract)
        {
            _productContract = productContract;
            _inventoryContract = inventoryContract;
            _brandContract = brandContract;
            _storageContract = storageContract;
            _salesCampaignContract = salesCampaignContract;
            _memberContract = memberContract;
            _retailContract = retailContract;
            _retailItemContract = retailItemContract;
            _scoreRuleContract = scoreRuleContract;
            _couponContract = couponContract;
            _administratorContract = administratorContract;
            _memberDepositContract = memberDepositContract;
            _checkerContract = checkerContract;
            _storeActivityContract = storeActivityContract;
            _storeContract = storeContract;
            _productTrackContract = productTrackContract;
            _permissionContract = permissionContract;
            _memberConsumeContract = memberConsumeContract;
            _smsContract = smsContract;
        }

        [Layout]
        public ActionResult Index()
        {
            //判断是否有查看会员的权限
            var viewMemberPermission = _permissionContract.Permissions.FirstOrDefault(p => !p.IsDeleted && p.IsEnabled && p.PermissionName == "选择会员" && p.Identifier == "ViewMember");
            if (viewMemberPermission == null)
            {
                ViewBag.CanSelectMember = false;
            }
            else
            {
                ViewBag.CanSelectMember = PermissionHelper.HasPermission(AuthorityHelper.OperatorId.Value, viewMemberPermission.Id, _administratorContract, _permissionContract);
            }
            return View();
        }

        public ActionResult Step2(decimal totalPrice, int storeId, string campIds, int? memberId)
        {
            var canUseCoupon = 1; //是否允许使用优惠券
            var canUseStoreActivity = 1;//是否允许使用店铺活动


            // 如果参与了商品活动,判断商品活动中是否包含禁用优惠券和其他活动的活动
            if (!string.IsNullOrEmpty(campIds))
            {
                var campIdList = campIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id)).ToList();
                var campList = _salesCampaignContract.SalesCampaigns.Where(camp => campIdList.Contains(camp.Id)).ToList();
                if (campList.Any(c => !c.OtherCampaign))
                {
                    canUseStoreActivity = 0;
                }
                if (campList.Any(c => !c.OtherCashCoupon))
                {
                    canUseCoupon = 0;
                }
            }

            if (memberId.HasValue && memberId > 0)
            {

                var memberProfile = _retailContract.GetMemberInfo(memberId.Value);
                if (memberProfile == null)
                {
                    return Json(OperationResult.Error("会员登录信息未找到,请重新登陆"));
                }
                ViewBag.LevelDiscount = memberProfile.LevelDiscount ?? 1.0F;
            }
            else
            {

                ViewBag.LevelDiscount = 1.0F;
            }

            ViewBag.StoreId = storeId;
            ViewBag.CanUseCoupon = canUseCoupon;
            ViewBag.CanUseStoreActivity = canUseStoreActivity;

            return PartialView(totalPrice);
        }

        public ActionResult Step3(FormCollection form)
        {
            var state = form["state"];
            var tid = AuthorityHelper.OperatorId;

            var scor = _scoreRuleContract.ScoreRules.Where(c => !c.IsDeleted && c.IsEnabled)
                                                    .OrderByDescending(c => c.UpdatedTime)
                                                    .FirstOrDefault();
            var scoreRuleDto = Mapper.Map<ScoreRuleDto>(scor);
            ViewBag.ConsumScore = scor == null ? "1:0" : (scor.ConsumeUnit + ":" + scor.ScoreUnit);

            ViewBag.isConsumeCardMoneyGetScore = scoreRuleDto.IsConsumeCardMoneyGetScore ? 1 : 0;
            ViewBag.isConsumeScoreGetScore = scoreRuleDto.IsConsumeScoreGetScore ? 1 : 0;
            ViewBag.CanUseScoreWhenNotBelongToStore = scoreRuleDto.CanUseScoreWhenNotBelongToStore ? 1 : 0;
            ViewBag.CanGetScoreWhenNotBelongToStore = scoreRuleDto.CanGetScoreWhenNotBelongToStore ? 1 : 0;

            ViewBag.adminis = new List<SelectListItem>();

            var operat = _administratorContract.Administrators.FirstOrDefault(c => c.Id == tid);
            if (operat != null)
            {
                int? depentId = operat.DepartmentId;
                if (depentId != null)
                {
                    var admli = _administratorContract.Administrators.Where(c => c.IsEnabled && !c.IsDeleted).Where(c => c.DepartmentId == depentId).Select(c => new SelectListItem()
                    {
                        Value = c.Id.ToString(),
                        Text = c.Member.RealName
                    }).ToList();
                    admli.Insert(0, new SelectListItem()
                    {
                        Text = "-选择经办人-",
                        Value = ""
                    });
                    ViewBag.adminis = admli;
                }
            }
            ViewBag.state = state;
            return PartialView((object)state);
        }

        /// <summary>
        /// 获取商品的零售信息
        /// </summary>
        /// <returns></returns>
        public ActionResult GetProductList(string storeid)
        {
            Response.Cache.SetOmitVaryStar(true);
            ViewBag.Storeid = storeid;
            ViewBag.Brand = CacheAccess.GetBrand(_brandContract, true);
            return PartialView();
        }

        /// <summary>
        /// 活动店铺下的优惠活动
        /// 根据店铺和会员/非会员类型进行筛选
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="memberCard"></param>
        /// <returns></returns>
        public ActionResult GetStoreActivity(int? storeId, string memberCard)
        {
            if (!storeId.HasValue)
            {
                return Json(new OperationResult(OperationResultType.Success, string.Empty, new { }), JsonRequestBehavior.AllowGet);
            }
            var activities = _storeActivityContract.StoreActivities.Where(s => !s.IsDeleted && s.IsEnabled && s.StartDate <= DateTime.Now && s.EndDate > DateTime.Now && s.StoreIds.Contains(storeId.Value.ToString())).ToList();
            if (string.IsNullOrEmpty(memberCard))
            {
                //查询面向非会员的活动
                activities = activities.Where(a => a.MemberTypes.Contains("-1")).ToList();
            }
            else //查询面向会员的活动
            {
                var memberEntity = _memberContract.Members.FirstOrDefault(m => !m.IsDeleted && m.IsEnabled && m.UniquelyIdentifies == memberCard);
                if (memberEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "会员不存在", new { }), JsonRequestBehavior.AllowGet);
                }

                // 店铺筛选,会员类型筛选
                activities = activities.Where(a => a.MemberTypes.Contains(memberEntity.MemberTypeId.ToString())).ToList();

                // 对有参与次数限制的活动校验会员的历史订单
                var restrictedActivityId = activities.Where(a => a.OnlyOncePerMember.HasValue && a.OnlyOncePerMember.Value).Select(a => a.Id).ToList();

                // 获取历史已参与的有限制次数的活动id
                var historyActivitiesFromOrder = _retailContract.Retails.Where(r => r.StoreActivityId.HasValue
                                                                                    && restrictedActivityId.Contains(r.StoreActivityId.Value)
                                                                                    && r.ConsumerId.Value == memberEntity.Id)
                                                                        .Select(r => r.StoreActivityId.Value)
                                                                        .ToList();
                // 过滤掉已参与的有限制次数的活动
                activities.RemoveAll(a => historyActivitiesFromOrder.Contains(a.Id));
            }

            var res = activities.Select(s => new
            {
                s.Id,
                s.ActivityName,
                s.MinConsume,
                s.DiscountMoney
            }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, res), JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 获取商品列表
        /// </summary>
        /// <returns></returns>
        public ActionResult List()
        {
            string numOrName = "";
            string Brand = "";
            GridRequest gr = new GridRequest(Request);
            Expression<Func<Product, bool>> predict = FilterHelper.GetExpression<Product>(gr.FilterGroup);

            int daCou = 0;
            var fg = gr.FilterGroup;
            if (fg.Rules.Count > 0)
            {
                for (int i = 0; i < fg.Rules.Count; i++)
                {
                    if (fg.Rules.ToList()[i].Field == "AttributeNameOrNum")
                    {
                        var te = fg.Rules.ToList()[i].Value;
                        if (te != null && te.ToString() != "")
                        {
                            // pageind = "1";
                            numOrName = te.ToString();
                        }
                    }
                    if (fg.Rules.ToList()[i].Field == "BrandId")
                    {
                        var te = fg.Rules.ToList()[i].Value;
                        if (te != null)
                            Brand = te.ToString();
                    }
                }
            }

            var resul =
                _productContract.Products.Where(c => !c.IsDeleted && c.IsEnabled && c.ProductOriginNumber.IsVerified == CheckStatusFlag.通过)
                    .Select(c => new
                    {
                        Id = c.Id,
                        Name = c.ProductName,
                        ProNum = c.ProductNumber,
                        BrandId = c.ProductOriginNumber.BrandId,
                        Brand = c.ProductOriginNumber.Brand.BrandName,
                        Size = c.Size.SizeName,
                        Seaso = c.ProductOriginNumber.Season.SeasonName,
                        Colo = c.Color.ColorName,
                        Thumbnail = c.ThumbnailPath ?? c.ProductOriginNumber.ThumbnailPath
                    });

            if (!string.IsNullOrEmpty(numOrName))
            {
                resul = resul.Where(c => c.Name == numOrName || c.ProNum == numOrName);
            }
            if (!string.IsNullOrEmpty(Brand) && Brand != "-1")
            {
                int brandid = Convert.ToInt32(Brand);
                resul = resul.Where(c => c.BrandId == brandid);
            }
            daCou = resul.Count();

            resul = resul.OrderBy(c => c.Id).Skip(gr.PageCondition.PageIndex).Take(gr.PageCondition.PageSize);
            var da = new GridData<object>(resul.ToList(), daCou, Request);
            return Json(da, JsonRequestBehavior.AllowGet);
        }

        ///yxk 2016-1
        /// <summary>
        /// 添加商品零售
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create(string orderInfo)
        {
            try
            {
                var retailInfoDTO = JsonHelper.FromJson<RetaiInfoDTO>(orderInfo);

                if (retailInfoDTO.ConsumeInfo.OutStoreId <= 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "没有选择出货店铺"));
                }

                if (retailInfoDTO.IsMember)
                {
                    var res = ProcessMemberRetail(retailInfoDTO);
                    return res;
                }
                else
                {
                    var res = ProcessNonMemberRetail(retailInfoDTO);
                    return res;
                }
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统繁忙,请稍候再试"));
            }
        }





        /// <summary>
        /// 处理会员零售
        /// </summary>
        /// <param name="retailInfoDTO">订单信息</param>
        /// <param name="isCheck">校验模式,不需要生成订单</param>
        /// <returns></returns>
        private JsonResult ProcessMemberRetail(RetaiInfoDTO retailInfoDTO, bool isCheck = false)
        {
            const bool isMember = true;
            try
            {
                var retail = new Retail()
                {
                    IsDeleted = false,
                    IsEnabled = true,
                    SwipeCardType = retailInfoDTO.ConsumeInfo.SwipeCardType,
                    OutStorageDatetime = retailInfoDTO.ConsumeInfo.OutStoragTime,
                    RetailNumber = GetOnlyNumb(),
                    OperatorId = retailInfoDTO.ConsumeInfo.Operat,
                    CollocationNumber = retailInfoDTO.MemberInfo == null ? "" : retailInfoDTO.MemberInfo.CollNum,
                    StoreId = retailInfoDTO.ConsumeInfo.OutStoreId,
                    TradeCredential = retailInfoDTO.TradeCredential,
                    TradeReferNumber = retailInfoDTO.TradeReferNumber
                };

                // 校验登录状态
                var memberDTO = _retailContract.GetMemberInfo(retailInfoDTO.MemberInfo.MemberId);
                if (memberDTO == null || memberDTO.UniquelyIdentifies != retailInfoDTO.MemberInfo.MemberNum)
                {
                    return Json(new OperationResult(OperationResultType.Error, "会员登陆已超时,请重新登录"));
                }

                // 获取会员信息
                var memberEntity = _memberContract.Members.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.UniquelyIdentifies == memberDTO.UniquelyIdentifies);

                if (memberEntity == null && (retail.ScoreConsume > 0 || retail.StoredValueConsume > 0))
                {
                    return Json(new OperationResult(OperationResultType.Error, "会员登录过期或异常，请重新登录"));
                }
                retail.ConsumerId = memberEntity.Id;


                // 积分规则校验
                var scoreRuleEntity = _scoreRuleContract.ScoreRules.Where(c => c.IsEnabled && !c.IsDeleted)
                                                             .OrderByDescending(c => c.UpdatedTime)
                                                             .FirstOrDefault();
                if (scoreRuleEntity == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "未找到有效的积分规则"));
                }
                var isBelongToCurrentStore = memberEntity.StoreId == retailInfoDTO.ConsumeInfo.OutStoreId;

                if (!isBelongToCurrentStore && !scoreRuleEntity.CanUseScoreWhenNotBelongToStore && retailInfoDTO.ConsumeInfo.Score > 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "会员在非归属店铺下无法使用积分消费"));
                }

                // 判断是否能获取积分
                var canGetScore = IsGetScore(scoreRuleEntity, retailInfoDTO.ConsumeInfo.ConsumeCoun, isMember, retailInfoDTO.ConsumeInfo.Score, retailInfoDTO.ConsumeInfo.CardMoney, isBelongToCurrentStore);

                retail.ScoreRuleId = canGetScore ? scoreRuleEntity.Id as int? : null;

                #region 商品校验

                // 数量校验
                if (!retailInfoDTO.Products.Any())
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品不为空"));
                }

                // 库存校验
                var retailItems = new List<RetailItem>();
                var barcodes = retailInfoDTO.Products.SelectMany(p => p.Barcodes).ToList();
                var productNumbers = retailInfoDTO.Products.Select(p => p.ProdNum).ToList();
                var productList = _productContract.Products.Where(c => !c.IsDeleted && c.IsEnabled && productNumbers.Contains(c.ProductNumber)).ToList();
                var inventoryList = _inventoryContract.Inventorys.Where(c => barcodes.Contains(c.ProductBarcode)).ToList();
                if (barcodes.Count != inventoryList.Count)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品流水号有误"));
                }
                if (productList.Count != productNumbers.Count)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品信息有误"));
                }
                if (!canGetScore)
                {
                    scoreRuleEntity = null;
                }

                var res = GenerateRetailItem(retail, retailInfoDTO.Products, productList, inventoryList, isMember, retailInfoDTO.ConsumeInfo, scoreRuleEntity);
                if (!res.Item1)
                {
                    return Json(new OperationResult(OperationResultType.Error, res.Item2));
                }
                if (res.Item3 == null)
                {
                    throw new Exception("生成商品明细异常");
                }
                retail.RetailItems = res.Item3;

                #endregion 商品校验

                #region 消费金额校验

                var consumeInfo = retailInfoDTO.ConsumeInfo;
                if (consumeInfo == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "消费信息不为空"));
                }

                //等级折扣校验
                if (!consumeInfo.LevelDiscount.HasValue)
                {
                    consumeInfo.LevelDiscount = 1;
                }

                if (consumeInfo.LevelDiscount > 1 || consumeInfo.LevelDiscount <= 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "等级折扣信息异常"));
                }

                if (memberEntity.MemberLevel == null)
                {
                    if (consumeInfo.LevelDiscount != 1)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "等级折扣信息异常"));
                    }
                }
                else
                {
                    if (consumeInfo.LevelDiscount.Value != (decimal)memberEntity.MemberLevel.Discount)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "等级折扣信息不一致"));
                    }
                }


                // 校验优惠券
                if (string.IsNullOrEmpty(consumeInfo.CouponNum))
                {
                    retail.CouponNumber = null;
                    retail.CouponConsume = 0;
                    retail.CouponItemId = null;
                }
                else
                {
                    var coupresul = CouponValid(consumeInfo.CouponNum);
                    if (coupresul.Item1 != null)
                    {
                        return Json(new OperationResult(OperationResultType.Error, coupresul.Item1));
                    }
                    // 优惠券超过实际金额
                    if (consumeInfo.CouponMoney > coupresul.Item2.Coupon.CouponPrice)
                    {
                        return Json(new OperationResult(OperationResultType.Error, "优惠券优惠金额不一致"));
                    }
                    // 优惠券金额<=总消费金额
                    retail.CouponItemId = coupresul.Item2.Id;
                    retail.CouponNumber = consumeInfo.CouponNum;
                    retail.CouponConsume = Math.Min(consumeInfo.ConsumeCoun, consumeInfo.CouponMoney);
                }

                //校验店铺活动
                if (consumeInfo.StoreActivityId.HasValue)
                {
                    var checkRes = CheckStoreActivity(consumeInfo, memberEntity);
                    if (!checkRes.Item1)
                    {
                        return Json(new OperationResult(OperationResultType.Error, checkRes.Item2));
                    }
                    //记录店铺活动优惠
                    retail.StoreActivityId = consumeInfo.StoreActivityId;
                    retail.StoreActivityDiscount = consumeInfo.storeActivityDiscountMoney;
                }
                else
                {
                    retail.StoreActivityDiscount = 0; //没有参加店铺活动
                }


                // 抹去校验
                if (consumeInfo.Erase > consumeInfo.ConsumeCoun)
                {
                    return Json(new OperationResult(OperationResultType.Error, "抹去金额不能超过总消费金额"));
                }

                //不找零的消费 积分+储值+抹零+优惠券+店铺活动优惠
                decimal cons = consumeInfo.Score + consumeInfo.CardMoney
                                             + consumeInfo.Erase
                                             + retail.CouponConsume
                                             + retail.StoreActivityDiscount;

                //不找零的消费大于总消费时，不刷卡不使用现金
                if (cons > consumeInfo.ConsumeCoun)
                {
                    retail.CashConsume = 0;
                    retail.SwipeConsume = 0;
                    if (consumeInfo.Cash != 0 || consumeInfo.SwipCard != 0)
                    {
                        var mes = isMember
                            ? "积分+储值+抹除+优惠券+活动已经大于消费额度，不应该再有现金和刷卡消费"
                            : "抹除+优惠券+活动已经大于消费额度，不应该再有现金和刷卡消费";
                        return Json(new OperationResult(OperationResultType.Error, mes));
                    }
                }

                retail.CashConsume = consumeInfo.Cash;
                retail.SwipeConsume = consumeInfo.SwipCard;

                //判断各项支付总额是有与扣除等级折扣之后的价格一致
                var retailConsu = Math.Round(cons + consumeInfo.SwipCard + consumeInfo.Cash - consumeInfo.ReturnMoney, 2);
                var moneyAfterLevelDiscount = Math.Round((consumeInfo.ConsumeCoun * consumeInfo.LevelDiscount.Value), 2);
                if (retailConsu - moneyAfterLevelDiscount != 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "实际付款与应付款不一致,差额:" + Math.Abs(retailConsu - moneyAfterLevelDiscount).ToString()));
                }

                if ((retail.CashConsume + retail.SwipeConsume) <= 0 && retail.ReturnMoney > 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "现金和刷卡消费为0的情况下，找零不应该大于0"));
                }


                retail.LevelDiscount = consumeInfo.LevelDiscount;
                retail.LevelDiscountAmount = Math.Round(consumeInfo.ConsumeCoun - moneyAfterLevelDiscount, 2);
                retail.ConsumeCount = consumeInfo.ConsumeCoun;
                retail.EraseConsume = consumeInfo.Erase;
                retail.ReturnMoney = consumeInfo.ReturnMoney;
                retail.ScoreConsume = consumeInfo.Score;
                retail.StoredValueConsume = consumeInfo.CardMoney;
                if (canGetScore && scoreRuleEntity != null)
                {
                    retail.GetScore = CalcArchiveScore(scoreRuleEntity, retail);
                }

                #endregion 消费金额校验
                if (isCheck)
                {
                    return Json(OperationResult.OK());
                }
                var optRes = SaveRetail(retail, inventoryList, memberEntity, canGetScore);
                return optRes;
            }
            catch (Exception e)
            {
                _Logger.Error(e.StackTrace + e.Message);
                return Json(new OperationResult(OperationResultType.Error, e.Message));
            }
        }
        /// <summary>
        /// 处理非会员零售
        /// </summary>
        /// <param name="retailInfoDTO"></param>
        /// <param name="isCheck">是否校验模式</param>
        /// <returns></returns>
        private JsonResult ProcessNonMemberRetail(RetaiInfoDTO retailInfoDTO, bool isCheck = false)
        {
            const bool isMember = false;
            try
            {
                var retail = new Retail()
                {
                    SwipeCardType = retailInfoDTO.ConsumeInfo.SwipeCardType,
                    ConsumerId = null,
                    ScoreRuleId = null,
                    CouponNumber = null,
                    RetailNumber = GetOnlyNumb(),
                    OperatorId = retailInfoDTO.ConsumeInfo.Operat,
                    OutStorageDatetime = retailInfoDTO.ConsumeInfo.OutStoragTime,
                    CollocationNumber = retailInfoDTO.MemberInfo == null ? "" : retailInfoDTO.MemberInfo.CollNum,
                    StoreId = retailInfoDTO.ConsumeInfo.OutStoreId,
                    GetScore = 0,
                    CouponConsume = 0,
                    ScoreConsume = 0,
                    StoredValueConsume = 0,
                    RealStoredValueConsume = 0,
                    RemainValue = 0,
                    RemainScore = 0,
                    LevelDiscount = null,
                    LevelDiscountAmount = 0,
                    TradeCredential = retailInfoDTO.TradeCredential,
                    TradeReferNumber = retailInfoDTO.TradeReferNumber
                };

                #region 商品校验

                // 数量校验
                if (!retailInfoDTO.Products.Any())
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品不为空"));
                }

                var consumeInfo = retailInfoDTO.ConsumeInfo;
                if (consumeInfo == null)
                {
                    return Json(new OperationResult(OperationResultType.Error, "消费信息不可为空"));
                }

                var barcodes = retailInfoDTO.Products.SelectMany(p => p.Barcodes).ToList();
                var productNumbers = retailInfoDTO.Products.Select(p => p.ProdNum).ToList();
                var productList = _productContract.Products.Where(c => !c.IsDeleted && c.IsEnabled && productNumbers.Contains(c.ProductNumber)).ToList();
                var inventoryList = _inventoryContract.Inventorys.Where(c => barcodes.Contains(c.ProductBarcode)).ToList();
                if (barcodes.Count != inventoryList.Count)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品流水号有误"));
                }
                if (productList.Count != productNumbers.Count)
                {
                    return Json(new OperationResult(OperationResultType.Error, "商品信息有误"));
                }

                var res = GenerateRetailItem(retail, retailInfoDTO.Products, productList, inventoryList, isMember, consumeInfo, null);
                if (!res.Item1)
                {
                    return Json(new OperationResult(OperationResultType.Error, res.Item2));
                }
                if (res.Item3 == null)
                {
                    throw new Exception("生成商品明细异常");
                }
                retail.RetailItems = res.Item3;

                #endregion 商品校验

                //校验店铺活动
                if (consumeInfo.StoreActivityId.HasValue)
                {
                    var checkRes = CheckStoreActivity(consumeInfo, null);
                    if (!checkRes.Item1)
                    {
                        return Json(new OperationResult(OperationResultType.Error, checkRes.Item2));
                    }
                    //记录店铺活动优惠
                    retail.StoreActivityId = consumeInfo.StoreActivityId;
                    retail.StoreActivityDiscount = consumeInfo.storeActivityDiscountMoney;
                }
                else
                {
                    retail.StoreActivityDiscount = 0; //没有参加店铺活动
                }

                #region 消费金额校验

                // 不找零的消费 抹零+店铺活动优惠
                decimal cons = consumeInfo.Erase + retail.StoreActivityDiscount;

                // 不找零的消费大于总消费时，不刷卡不使用现金
                if (cons > consumeInfo.ConsumeCoun)
                {
                    retail.CashConsume = 0;
                    retail.SwipeConsume = 0;
                    if (consumeInfo.Cash != 0 || consumeInfo.SwipCard != 0)
                    {
                        var mes = isMember
                            ? "积分+储值+抹除+优惠券+活动已经大于消费额度，不应该再有现金和刷卡消费"
                            : "抹除+优惠券+活动已经大于消费额度，不应该再有现金和刷卡消费";
                        return Json(new OperationResult(OperationResultType.Error, mes));
                    }
                }

                retail.CashConsume = consumeInfo.Cash;
                retail.SwipeConsume = consumeInfo.SwipCard;

                //判断价格是否一致
                var retailConsu = cons + consumeInfo.SwipCard + consumeInfo.Cash - consumeInfo.ReturnMoney;
                if (retailConsu != consumeInfo.ConsumeCoun)
                {
                    return Json(new OperationResult(OperationResultType.Error, "实际付款与应付款不一致,差额:" + Math.Abs(retailConsu - consumeInfo.ConsumeCoun).ToString()));
                }

                if ((retail.CashConsume + retail.SwipeConsume) <= 0 && retail.ReturnMoney > 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "现金和刷卡消费为0的情况下，找零不应该大于0"));
                }

                retail.ConsumeCount = consumeInfo.ConsumeCoun;
                retail.EraseConsume = consumeInfo.Erase;
                retail.ReturnMoney = consumeInfo.ReturnMoney;
                retail.ScoreConsume = consumeInfo.Score;
                retail.StoredValueConsume = consumeInfo.CardMoney;

                #endregion 消费金额校验
                if (isCheck)
                {
                    return Json(OperationResult.OK());
                }
                var optRes = SaveRetail(retail, inventoryList, memberEntityToUpdate: null, isGetScore: false);
                return optRes;
            }
            catch (Exception e)
            {
                _Logger.Error(e.StackTrace + e.Message);
                return Json(new OperationResult(OperationResultType.Error, e.Message));
            }
        }

        private JsonResult SaveRetail(Retail retailEntityToSave, List<Inventory> inventoryListToUpdate, Member memberEntityToUpdate, bool isGetScore)
        {

            using (var transactin = _retailContract.GetTransaction())
            {
                try
                {
                    // 会员订单,修改用户账户
                    if (memberEntityToUpdate != null)
                    {
                        //会员消费记录,记录消费了多少储值和积分
                        if (retailEntityToSave.StoredValueConsume > 0 || retailEntityToSave.ScoreConsume > 0)
                        {
                            _memberConsumeContract.LogWhenRetail(retailEntityToSave);
                        }

                        // 获得积分记录到充值记录中
                        if (isGetScore && retailEntityToSave.GetScore > 0)
                        {
                            _memberDepositContract.LogGetScoreWhenRetail(retailEntityToSave.StoreId.Value, memberEntityToUpdate, retailEntityToSave.GetScore, retailEntityToSave.RetailNumber);
                        }

                        // 更新储值
                        if (retailEntityToSave.StoredValueConsume > 0)
                        {
                            if (memberEntityToUpdate.Balance < retailEntityToSave.StoredValueConsume)
                            {
                                throw new Exception("实际消费的储值大于会员现有储值");
                            }
                            memberEntityToUpdate.Balance -= retailEntityToSave.StoredValueConsume;
                            retailEntityToSave.RemainValue = memberEntityToUpdate.Balance;

                            //计算储值成本
                            var realBalance = CheckBalanceCost(memberEntityToUpdate.Id, retailEntityToSave.StoredValueConsume);
                            if (!realBalance.Item1)
                            {
                                throw new Exception("会员储值系数计算错误");
                            }
                            else if (realBalance.Item3 > 1.0M || realBalance.Item2 > retailEntityToSave.StoredValueConsume)
                            {
                                throw new Exception($"会员储值系数异常：{realBalance.Item3}");
                            }
                            retailEntityToSave.RealStoredValueConsume = realBalance.Item2;
                            retailEntityToSave.Quotiety = realBalance.Item3;
                        }
                        // 更新积分
                        if (retailEntityToSave.ScoreConsume > 0)
                        {
                            if (memberEntityToUpdate.Score < retailEntityToSave.ScoreConsume)
                            {
                                throw new Exception("实际消费的积分大于会员现有积分");
                            }
                            memberEntityToUpdate.Score -= retailEntityToSave.ScoreConsume;
                        }

                        // 赠送操作积分
                        if (isGetScore && retailEntityToSave.GetScore > 0)
                        {

                            memberEntityToUpdate.Score += retailEntityToSave.GetScore;
                            retailEntityToSave.RemainScore = memberEntityToUpdate.Score;

                        }
                        var memberDto = Mapper.Map<MemberDto>(memberEntityToUpdate);
                        var updateRes = _memberContract.UpdateScore(memberDto);
                        if (updateRes.ResultType == OperationResultType.Error)
                        {
                            throw new Exception("用户储值积分更新失败");
                        }
                        // 清除登录态
                        var hasClear = _retailContract.ClearMemberDTOInfo(memberEntityToUpdate.Id);
                        if (!hasClear)
                        {
                            _Logger.Error("零售删除会员key失败" + memberEntityToUpdate.Id.ToString());
                        }
                    }

                    // 保存订单
                    var result = _retailContract.Insert(false, retailEntityToSave);

                    if (result.ResultType != OperationResultType.Success)
                    {
                        throw new Exception("订单保存失败");
                    }

                    // 更新库存 修改订单中商品库存的状态
                    inventoryListToUpdate.Each(c =>
                    {
                        c.Status = InventoryStatus.JoinOrder;
                    });
                    result = _inventoryContract.Update(inventoryListToUpdate.ToArray());
                    if (result.ResultType == OperationResultType.Error)
                    {
                        throw new Exception("库存状态更新失败");
                    }
                    // 优惠券失效处理
                    if (!string.IsNullOrEmpty(retailEntityToSave.CouponNumber))
                    {
                        result = _couponContract.SetCouponItemUsed(retailEntityToSave.CouponNumber);
                        if (result.ResultType != OperationResultType.Success)
                        {
                            throw new Exception("优惠券状态更新失败");
                        }
                    }

                    //商品零售 添加追踪记录
                    var storeEntity = _storeContract.Stores.FirstOrDefault(s => s.Id == retailEntityToSave.StoreId.Value);
                    var StoreName = storeEntity.StoreName;
                    var storePhone = storeEntity.MobilePhone ?? string.Empty;
                    foreach (var item in retailEntityToSave.RetailItems)
                    {
                        var barcode = item.RetailInventorys.Select(i => i.ProductBarcode).ToList();
                        foreach (var barcodeitem in barcode)
                        {
                            if (!string.IsNullOrEmpty(barcodeitem))
                            {
                                #region 商品追踪
                                ProductTrackDto pt = new ProductTrackDto();
                                pt.ProductNumber = barcodeitem.Substring(0, barcodeitem.Length - 3);
                                pt.ProductBarcode = barcodeitem;
                                pt.Describe = string.Format(ProductOptDescTemplate.ON_PRODUCT_RETAIL, retailEntityToSave.Store.StoreName);
                                _productTrackContract.Insert(pt);
                                #endregion
                            }
                        }
                    }
                    if (!ConfigurationHelper.IsDevelopment())
                    {
                        //会员短信通知
                        //1、$storeName：店铺
                        //2、$memberName：会员昵称
                        //3、$saleTime：销售时间
                        //4、$orderNumber：订单编号
                        //5、$storePhone：店铺电话（有电话取电话,否则取手机号）
                        //6、$realPayMoney：实际支付金额
                        var config = RedisCacheHelper.GetSMSConfig();
                        if (memberEntityToUpdate != null && config["retail"] == "1")
                        {
                            var tempDict = new Dictionary<string, object> {
                            {"storeName", StoreName},
                            {"memberName", memberEntityToUpdate.RealName},
                            {"saleTime", DateTime.Now},
                            {"orderNumber", retailEntityToSave.RetailNumber},
                            {"storePhone", storePhone},
                            {"realPayMoney", retailEntityToSave.CashConsume+retailEntityToSave.SwipeConsume},
                        };
                            _smsContract.SendSms(memberEntityToUpdate.MobilePhone, TemplateNotificationType.StoreRetail, tempDict);
                        }
                    }



                    // 事务提交
                    transactin.Commit();
                    return Json(new OperationResult(OperationResultType.Success, "success", retailEntityToSave.RetailNumber));
                }
                catch (Exception e)
                {
                    transactin.Rollback();
                    return Json(OperationResult.Error(e.Message));
                }



            }

        }

        private Tuple<bool, string, List<RetailItem>> GenerateRetailItem(Retail retailEntityToSave, List<ProductInfo> products, List<Product> productEntities, List<Inventory> inventoryList, bool isMember, ConsumeInfo consumerInfo, ScoreRule scoreRule)
        {
            // 生成明细
            List<RetailItem> retailItems = new List<RetailItem>();
            foreach (var prod in products)
            {
                decimal sumMoney = 0;//小计
                decimal saleCampaignDiscount = 10;//商品活动折扣,1...10之间
                decimal brandDiscount = 1;//品牌折扣,0...1 之间
                decimal retailPrice = 0;//零售价

                // 商品信息校验
                var product = productEntities.FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.ProductNumber == prod.ProdNum);
                if (product == null)
                {
                    return Tuple.Create<bool, string, List<RetailItem>>(false, "未找到商品信息", null);
                }

                // 商品活动校验
                if (prod.CampId.HasValue) // 零售价 => 吊牌价*商品活动折扣
                {
                    var res = CheckProductDiscount(prod, product.ProductOriginNumber.BigProdNum, isMember, consumerInfo);
                    if (!res.Item1)
                    {
                        return Tuple.Create<bool, string, List<RetailItem>>(false, res.Item2, null);
                    }

                    saleCampaignDiscount = res.Item3.Value;
                    retailPrice = (decimal)product.ProductOriginNumber.TagPrice * (saleCampaignDiscount / 10); //discount 存储的不是小数0.75,0.85而是7.5,8.5....所以要除以10,得到小数

                    // 只使用商品折扣
                    brandDiscount = 1;
                }
                else  //零售价 => 吊牌价 * 品牌折扣
                {
                    retailPrice = (decimal)(product.ProductOriginNumber.TagPrice * product.ProductOriginNumber.Brand.DefaultDiscount); //discount 存储的不是小数0.75,0.85而是7.5,8.5....所以要除以10,得到小数

                    // 只使用品牌折扣
                    saleCampaignDiscount = 10;
                    brandDiscount = (decimal)product.ProductOriginNumber.Brand.DefaultDiscount;
                }

                // 得到小计                                                                                                                                  // 零售价*数量  总价
                sumMoney = retailPrice * prod.Quantity;

                // 获取售出库存所在的仓库
                var productInventory = inventoryList.Where(i => prod.Barcodes.Contains(i.ProductBarcode)).ToList();
                var storageids = productInventory.Select(c => c.StorageId).Distinct();

                // 生成零售明细
                var retailItem = new RetailItem()
                {
                    ProductId = product.Id,                       //商品id
                    OutStorageIds = string.Join(",", storageids), //商品所在的仓库id
                    ProductTagPrice = (decimal)product.ProductOriginNumber.TagPrice,   //吊牌价
                    BrandDiscount = prod.CampId.HasValue ? 1M : (decimal)product.ProductOriginNumber.Brand.DefaultDiscount, //品牌折扣,与商品活动互斥
                    SalesCampaignId = prod.CampId,                  //商品活动id
                    SalesCampaignDiscount = saleCampaignDiscount,             //商品活动折扣
                    ProductRetailPrice = retailPrice,            //零售价
                    RetailCount = prod.Quantity,                  //数量
                    ProductRetailItemMoney = sumMoney,              //总价  零售价*数量
                    OperatorId = consumerInfo.Operat,               //经办人
                    Retail = retailEntityToSave //与零售关联
                };

                // 为明细生成对应的库存流水号记录
                foreach (var inventory in productInventory)
                {
                    retailItem.RetailInventorys.Add(new RetailInventory()
                    {
                        InventoryId = inventory.Id,     //库存id
                        RetailItem = retailItem,
                        RetailNumber = retailEntityToSave.RetailNumber, //零售订单号
                        ProductBarcode = inventory.ProductBarcode,
                    });
                }
                retailItems.Add(retailItem);
            }
            return Tuple.Create(true, string.Empty, retailItems);
        }

        private Tuple<bool, string> CheckStoreActivity(ConsumeInfo consumeInfo, ERP.Models.Member memberEntity)
        {
            var activityEntity = _storeActivityContract.StoreActivities.FirstOrDefault(a => !a.IsDeleted
                                                                                            && a.IsEnabled
                                                                                            && a.StoreIds.Contains(consumeInfo.OutStoreId.ToString())
                                                                                            && a.Id == consumeInfo.StoreActivityId.Value);
            if (activityEntity == null
                || activityEntity.StartDate > DateTime.Now
                || activityEntity.EndDate < DateTime.Now
                || consumeInfo.storeActivityDiscountMoney > activityEntity.DiscountMoney
                || consumeInfo.ConsumeCoun < activityEntity.MinConsume)
            {
                return Tuple.Create(false, "无效的店铺活动");
            }
            if (memberEntity != null) //仅适用于会员订单
            {
                // 如果活动有参与次数限制,校验是否之前参加过活动
                if (activityEntity.OnlyOncePerMember.HasValue
                    && activityEntity.OnlyOncePerMember.Value
                    && _retailContract.Retails.Any(r => r.ConsumerId.Value == memberEntity.Id && r.StoreActivityId.Value == activityEntity.Id))
                {
                    return Tuple.Create(false, "会员之前已参与过该店铺活动,无法再次参与");
                }
            }
            return Tuple.Create(true, string.Empty);
        }

        /// <summary>
        /// 获取商品活动折扣
        /// </summary>
        private Tuple<bool, string, decimal?> CheckProductDiscount(ProductInfo prod, string BigProdNum, bool isMember, ConsumeInfo consumeInfo)
        {
            var salecamp = CacheAccess.GetSalesCampaign(_salesCampaignContract)
                                       .FirstOrDefault(c => c.IsEnabled && !c.IsDeleted && c.Id == prod.CampId);
            if (salecamp == null)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动不存在", null);
            }
            if (!salecamp.BigProdNums.Contains(BigProdNum))
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动款号不包含该商品", null);
            }
            prod.CampType = (SalesCampaignType)salecamp.SalesCampaignType;
            if (prod.CampType == null)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动类型不可为空", null);
            }

            // 活动用户类型校验
            if (isMember && prod.CampType == SalesCampaignType.NoMemberOnly)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动类型仅适用于非会员", null);
            }
            if (!isMember && prod.CampType == SalesCampaignType.MemberOnly)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动类型仅适用于会员", null);
            }

            // 活动用户折扣校验
            if (isMember && prod.CampDiscount != salecamp.MemberDiscount)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动会员折扣不一致", null);
            }
            if (!isMember && prod.CampDiscount != salecamp.NoMmebDiscount)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动非会员折扣不一致", null);
            }

            // 活动限制规则校验
            if (!salecamp.OtherCampaign && consumeInfo.StoreActivityId.HasValue && consumeInfo.storeActivityDiscountMoney > 0)
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动无法同时与店铺活动一起参与", null);
            }
            if (!salecamp.OtherCashCoupon && !string.IsNullOrEmpty(consumeInfo.CouponNum))
            {
                return Tuple.Create<bool, string, decimal?>(false, "商品活动无法同时与代金券一起使用", null);
            }

            var discount = prod.CampType == SalesCampaignType.MemberOnly ? salecamp.MemberDiscount : salecamp.NoMmebDiscount;
            return Tuple.Create<bool, string, decimal?>(true, string.Empty, (decimal)discount);
        }

        /// <summary>
        ///商品零售时 计算储值成本
        /// </summary>
        /// <param name="balance"></param>
        private Tuple<bool, decimal, decimal> CheckBalanceCost(int memberId, decimal balance)
        {
            decimal realBalance = 0M; //储值成本
            var deposEntity = _memberDepositContract.MemberDeposits
                                                    .Where(c => c.MemberId == memberId && c.IsEnabled && !c.IsDeleted)
                                                    .OrderByDescending(c => c.CreatedTime)
                                                   .FirstOrDefault();
            // 储值记录为空的情况
            if (deposEntity == null)
            {
                //储值系数为空,默认使用1来算
                return Tuple.Create(true, balance, 1.0M);
            }

            // 储值记录不为空的情况
            decimal quotiety = deposEntity.Quotiety <= 0 ? 1 : deposEntity.Quotiety;// 历史储值数据可能为0

            realBalance = quotiety * balance;
            if (realBalance < 0)
            {
                return Tuple.Create(false, 0M, 0M);
            }
            return Tuple.Create(true, realBalance, quotiety);
        }

        /// <summary>
        /// 判断是否可以获得积分
        /// </summary>
        /// <param name="consumeCoun"></param>
        /// <param name="scoreRule"></param>
        /// <returns></returns>
        private decimal CalcArchiveScore(ScoreRule scoreRule, Retail retailEntity)
        {
            if (scoreRule == null || retailEntity == null)
            {
                throw new ArgumentNullException("scoreRule,retailEntity不能为空");
            }

            // 首先计算扣除掉一系列优惠,实际消费了多少钱
            // 获取商品零售价总价钱
            var productPrice = retailEntity.ConsumeCount;

            // 等级折扣优惠
            if (retailEntity.LevelDiscount.HasValue)
            {
                productPrice = productPrice * retailEntity.LevelDiscount.Value;
            }

            // 店铺活动优惠
            if (retailEntity.StoreActivityId.HasValue && retailEntity.StoreActivityDiscount > 0)
            {
                productPrice -= retailEntity.StoreActivityDiscount;
            }

            // 优惠券
            if (!string.IsNullOrEmpty(retailEntity.CouponNumber) && retailEntity.CouponConsume > 0)
            {
                productPrice -= retailEntity.CouponConsume;
            }
            // 消费/积分比例
            var ratio = (decimal)(scoreRule.ScoreUnit / scoreRule.ConsumeUnit);
            var getScore = Math.Ceiling(productPrice * ratio);
            return getScore;
        }



        /// <summary>
        /// 打印购物凭证
        /// </summary>
        /// <param name="numb">销售编号</param>
        /// <returns></returns>
        public ActionResult PrintReceipt(string numb, bool onlyData = false)
        {
            var retail = _retailContract.Retails.Include(c => c.Consumer).FirstOrDefault(c => c.RetailNumber == numb);
            if (onlyData) //返回model
            {
                var data = new
                {
                    retail.RetailNumber,
                    retail.ConsumerId,
                    MemberName = retail.Consumer?.MemberName,
                    UniquelyIdentifies = retail.Consumer?.UniquelyIdentifies,
                    RetailItems = retail.RetailItems.Select(item => new
                    {
                        item.Product.ProductNumber,
                        ProductName = item.Product.ProductName ?? item.Product.ProductOriginNumber.ProductName,
                        item.Product.Color.ColorName,
                        item.Product.Size.SizeName,
                        item.RetailCount,
                        item.ProductTagPrice,
                        item.SalesCampaignId,
                        item.SalesCampaignDiscount,
                        item.BrandDiscount,
                        item.ProductRetailPrice,
                        item.ProductRetailItemMoney

                    }),
                    retail.ConsumeCount,
                    retail.LevelDiscount,
                    retail.LevelDiscountAmount,
                    retail.EraseConsume,
                    retail.ScoreConsume,
                    retail.StoredValueConsume,
                    retail.StoreActivityDiscount,
                    retail.CouponConsume,
                    retail.SwipeConsume,
                    retail.CashConsume,
                    retail.ReturnMoney,
                    CashAndSwipeCardTotal = retail.CashConsume + retail.SwipeConsume,
                    retail.GetScore,
                    CreatedTime = retail.CreatedTime.ToUnixTime(),
                    retail.Store.MobilePhone

                };
                return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
            }
            else   //返回页面
            {
                return PartialView(retail);

            }

        }

        /// <summary>
        /// 获取店铺商品列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetProductsByStore()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Inventory, bool>> predicate = FilterHelper.GetExpression<Inventory>(request.FilterGroup);

            var proall = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Where(predicate);
            var proli =
                proall.OrderBy(c => c.Id)
                    .Skip(request.PageCondition.PageIndex)
                    .Take(request.PageCondition.PageSize)
                    .Select(c => new
                    {
                        Id = c.Product.Id,
                        Name = c.Product.ProductName,
                        ProNum = c.Product.ProductNumber,
                        Brand = c.Product.ProductOriginNumber.Brand.BrandName,
                        Size = c.Product.Size.SizeName,
                        Seaso = c.Product.ProductOriginNumber.Season.SeasonName,
                        Colo = c.Product.Color.IconPath,
                        TagPrice = c.Product.ProductOriginNumber.TagPrice,
                        Thumbnail = c.Product.ThumbnailPath,
                        ColoName = c.Product.Color.ColorName
                    }).ToList();
            return Json(new GridData<object>(proli, proall.Count(), request.RequestInfo));
        }

        /// <summary>
        /// 根据当前用户的的访问权限获取他所在店铺的商品
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="existProductNumbers"></param>
        /// <param name="productName"></param>
        /// <returns></returns>
        public JsonResult GetProductsCurrentUser(int storeId, string existProductNumbers, string productName)
        {
            //var storeIds = System.Web.HttpContext.Current.Items["_storeids_"] as List<int>;
            var existProductNumberArr = new List<string>();
            if (!string.IsNullOrEmpty(existProductNumbers))
            {
                existProductNumberArr.AddRange(existProductNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries));
            }
            GridRequest request = new GridRequest(Request);

            Expression<Func<Inventory, bool>> predicate = FilterHelper.GetExpression<Inventory>(request.FilterGroup);

            var query = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Where(predicate).Where(i => i.Status == InventoryStatus.Default);
            //过滤已经加入到列表中的货号
            if (existProductNumberArr.Count > 0)
            {
                query = query.Where(i => !existProductNumberArr.Contains(i.Product.ProductNumber));
            }
            if (!string.IsNullOrEmpty(productName))
            {
                query = query.Where(i => i.Product.ProductOriginNumber.ProductName.Contains(productName));
            }

            var productNums = query.GroupBy(c => c.Product.ProductNumber)
                                    .Select(c => c.Key)
                                    .ToList();

            GridData<object> dat = new GridData<object>(new List<object>(), 0, request.RequestInfo);
            if (productNums != null)
            {
                var proall =
                    _productContract.Products
                        .Where(c => productNums.Contains(c.ProductNumber) && c.IsEnabled && !c.IsDeleted);
                var proli = proall.OrderBy(c => c.Id)
                    .Skip(request.PageCondition.PageIndex)
                    .Take(request.PageCondition.PageSize)
                    .Select(c => new
                    {
                        c.Id,
                        Name = c.ProductName,
                        ProNum = c.ProductNumber,
                        Brand = c.ProductOriginNumber.Brand.BrandName,
                        Categ = c.ProductOriginNumber.Category.CategoryName,
                        Size = c.Size.SizeName,
                        Seaso = c.ProductOriginNumber.Season.SeasonName,
                        Colo = c.Color.IconPath,
                        c.ProductOriginNumber.TagPrice,
                        Thumbnail = c.ThumbnailPath ?? c.ProductOriginNumber.ThumbnailPath,
                        ColoName = c.Color.ColorName
                    }).ToList().Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.ProNum,
                        x.Brand,
                        x.Categ,
                        x.Size,
                        x.Seaso,
                        x.Colo,
                        x.TagPrice,
                        x.Thumbnail,
                        x.ColoName,
                        Cou = GetInventoryCountFromAllStore(x.Id),
                        EnabCou = GetInventoryAvailableCountFromCurrentStore(x.Id, storeId)
                    }).ToList();
                dat = new GridData<object>(proli, proall.Count(), request.RequestInfo);
            }

            return Json(dat);
        }

        /// <summary>
        /// 根据商品货号获取商品信息
        /// </summary>
        /// <param name="nums"></param>
        /// <param name="storeid"></param>
        /// <returns></returns>
        public ActionResult GetProductsByNums(string[] nums, int storeid)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error, "操作异常");
            nums = GetProductNums(nums);
            if (nums != null && nums.Any())
            {
                var t = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract).Where(c => nums.Contains(c.ProductNumber) && c.IsEnabled && !c.IsDeleted && c.StoreId == storeid).GroupBy(c => c.ProductNumber).Select(c => c.FirstOrDefault()).Select(c => new
                {
                    c.Id,
                    c.ProductId,
                    c.ProductNumber,
                    c.Product.ProductName,
                    c.Product.ProductOriginNumber.TagPrice,
                    c.Product.ProductOriginNumber.Brand.BrandName,
                    c.Product.ProductOriginNumber.Category.CategoryName,
                    c.Product.Color.IconPath,
                    c.Product.Color.ColorName,
                    c.Product.Size.SizeName,
                    c.Product.ProductOriginNumber.Season.SeasonName,
                    c.Product.ThumbnailPath,
                }).ToList().Select(c => new
                {
                    c.Id,
                    c.ProductNumber,
                    c.ProductName,
                    c.TagPrice,

                    c.BrandName,
                    c.CategoryName,
                    c.IconPath,
                    c.ColorName,
                    c.SizeName,
                    c.SeasonName,
                    c.ThumbnailPath,
                    Cou = GetInventoryAvailableCountFromCurrentStore(c.Id, storeid),
                    stores = GetStoresByProId(c.Id)
                }).ToList();

                if (t.Count > 0)
                    resul = new OperationResult(OperationResultType.Success, "ok") { Data = t };
            }
            return Json(resul);
        }

        /// <summary>
        /// 根据流水号获取商品的信息
        /// </summary>
        /// <param name="storeId"></param>
        /// <param name="barcode"></param>
        /// <returns></returns>
        public ActionResult GetProductsByBarcode(int? storeId, string barcode)
        {
            if (!storeId.HasValue)
            {
                return Json(new OperationResult(OperationResultType.Error, "参数错误-storeId"));
            }
            GridRequest gr = new GridRequest(Request);
            List<object> li = new List<object>();
            GridDataResul<object> data;
            OperationResult result = new OperationResult(OperationResultType.Error, "操作异常");

            try
            {
                // 参数处理
                var barcodearr = barcode.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var len = barcodearr.Count();
                if (len <= 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "参数错误"));
                }

                var expre = FilterHelper.GetExpression<Inventory>(gr.FilterGroup);

                // 过滤当前用户可以访问的仓库
                var inventoryQuery = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                                    .Where(c => c.IsEnabled && !c.IsDeleted)
                                                    .Where(c => c.StoreId == storeId)
                                                    .Include(c => c.Product)
                                                    .Include(c => c.Product.Size)
                                                    .Include(c => c.Product.Color)
                                                    .Include(c => c.Product.ProductOriginNumber)
                                                    .Include(c => c.Product.ProductOriginNumber.Brand)
                                                    .Include(c => c.Product.ProductOriginNumber.Category)
                                                    .Include(c => c.Product.ProductOriginNumber.Season);




                // 检查库存数量与传入库存条码个数
                var searchQuery = inventoryQuery.Where(expre);
                if (searchQuery.Count() != len)
                {
                    result.Message = "该商品不存在或当前用户无权限访问";
                    throw new Exception();
                }

                // 检查库存状态是否被其他用户锁定
                var isLock = CheckIsLockInveByBarcode(searchQuery.Select(c => c.ProductBarcode).ToArray());
                if (isLock.Item1)
                {
                    result.Message = isLock.Item2;
                    throw new Exception();
                }

                // 将提交的流水号锁定
                LockInvetory(barcodearr);

                // 获取品牌折扣字典,用来计算零售价
                var brandList = _brandContract.Brands.Where(b => !b.IsDeleted && b.IsEnabled)
                                                        .Select(b => new { b.BrandName, b.DefaultDiscount })
                                                        .ToList();
                Dictionary<string, double> brandDiscountDic = new Dictionary<string, double>();
                foreach (var item in brandList)
                {
                    if (!brandDiscountDic.ContainsKey(item.BrandName))
                    {
                        brandDiscountDic.Add(item.BrandName, item.DefaultDiscount);
                    }
                }

                var groupData = (from c in inventoryQuery.Where(expre).ToList()

                                     //let discount = (float)(c.Product.ProductOriginNumber.Brand.DefaultDiscount * c.Product.ProductOriginNumber.TagPrice / c.Product.ProductOriginNumber.TagPrice * 10)

                                 select new RetailInventoryData
                                 {
                                     Id = c.Id,
                                     ProductId = c.Product.Id,
                                     ProductBarcode = c.ProductBarcode,
                                     ProductNumber = c.Product.ProductNumber,
                                     ProductName = c.Product.ProductName,
                                     TagPrice = c.Product.ProductOriginNumber.TagPrice,
                                     BrandName = c.Product.ProductOriginNumber.Brand.BrandName,
                                     CategoryName = c.Product.ProductOriginNumber.Category.CategoryName,
                                     IconPath = c.Product.Color.IconPath,
                                     ColorName = c.Product.Color.ColorName,
                                     SizeName = c.Product.Size.SizeName,
                                     SeasonName = c.Product.ProductOriginNumber.Season.SeasonName,
                                     ThumbnailPath = c.Product.ThumbnailPath ?? c.Product.ProductOriginNumber.ThumbnailPath,
                                     StoreName = c.Store.StoreName,
                                     StoreId = c.StoreId,
                                     RetailPrice = (float)(c.Product.ProductOriginNumber.TagPrice * brandDiscountDic[c.Product.ProductOriginNumber.Brand.BrandName]),  //商品零售价 = 吊牌价*商品品牌折扣
                                     RetailDiscount = (float)(c.Product.ProductOriginNumber.Brand.DefaultDiscount * 10) //折扣显示, 0.8->八折, 0.9->9折...
                                 }).GroupBy(c => c.ProductNumber).ToList();

                if (groupData.Count <= 0)
                {
                    result.Message = "未找到可用的该商品记录";
                    throw new Exception();
                }

                // 获取店铺下所有商品活动
                var storeSalesCampaignList = GetAvailableSalesCampaignsByStore(storeId.Value);

                //组装成parent-child格式的数据
                foreach (var item in groupData)
                {
                    //如果得到多个店铺id，说明库存出自不同的仓库，则返回提示信息

                    var storeids = item.Select(c => c.StoreId).Distinct().ToArray();
                    if (storeids.Count() != 1)
                    {
                        result.Message = "当前扫入的商品所属店铺与已选中的店铺不一致";
                        throw new Exception();
                    }
                    storeId = storeids[0];
                    int productId = item.Select(g => g.ProductId).FirstOrDefault();
                    float tagprice = item.Select(g => g.TagPrice).FirstOrDefault();

                    li.Add(new
                    {
                        Id = "p" + productId,
                        ParentId = "",
                        ProductNumber = item.Key,
                        TagPrice = tagprice,
                        RetailPrice = item.Select(g => g.RetailPrice).FirstOrDefault(),
                        BrandName = item.Select(g => g.BrandName).FirstOrDefault(),
                        CategoryName = item.Select(g => g.CategoryName).FirstOrDefault(),
                        IconPath = item.Select(g => g.IconPath).FirstOrDefault(),
                        ColorName = item.Select(g => g.ColorName).FirstOrDefault(),
                        SizeName = item.Select(g => g.SizeName).FirstOrDefault(),
                        SeasonName = item.Select(g => g.SeasonName).FirstOrDefault(),
                        RetailDiscount = item.Select(g => g.RetailDiscount).FirstOrDefault(),
                        ThumbnailPath = item.Select(g => g.ThumbnailPath).FirstOrDefault(),
                        SalesCampaign = GetSalesCampaignsByProId(storeSalesCampaignList, productId),
                        CurCou = item.Count(),                              //选购数量
                        AllCou = GetInventoryCountFromAllStore(productId),  //所有店铺下可用库存
                        Cou = GetInventoryAvailableCountFromCurrentStore(productId, storeId.Value) //本店铺内可用库存
                    });
                    li.AddRange(
                        item.Select(c => new
                        {
                            c.Id,
                            ParentId = "p" + item.Select(g => g.ProductId).FirstOrDefault(),
                            ProductNumber = c.ProductBarcode,
                            TagPrice = c.TagPrice,
                            RetailPrice = c.RetailPrice,
                            RetailDiscount = c.RetailDiscount,
                            BrandName = c.BrandName,
                            CategoryName = c.CategoryName,
                            IconPath = c.IconPath,
                            ColorName = c.ColorName,
                            SizeName = c.SizeName,
                            SeasonName = c.SeasonName,
                            ThumbnailPath = c.ThumbnailPath
                        }).ToList());
                }
                if (li.Any())
                {
                    result = new OperationResult(OperationResultType.Success, "ok");
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                data = new GridDataResul<object>(li, li.Count, Request);
                data.Other = new { result, storeId };
            }

            return Json(data);
        }

        /// <summary>
        /// 查询对应商品货号下可用于销售的库存流水号
        /// 根据已有的条码和提供的商品编号获取指定数量的新条码
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult GetBarcodes(List<RetailMod> retailMods, int storeId)
        {
            List<string> barcodes = new List<string>();
            var productNumbers = retailMods.Select(c => c.ProductNumb).ToArray();
            var inves = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                .Where(c => productNumbers.Contains(c.ProductNumber) && c.IsEnabled && !c.IsDeleted)
                .Where(c => !c.IsLock && c.Status == InventoryStatus.Default)
                .Where(c => c.StoreId == storeId);

            foreach (var ite in retailMods)
            {
                if (ite.ExisBarcode == null)
                {
                    ite.ExisBarcode = new string[0];
                }

                int excou = ite.ExisBarcode == null ? 0 : ite.ExisBarcode.Count();
                var takecou = ite.NeedCou - excou; //需要新提取的条码 = 总共需要的数量-已经拿过的条码
                if (takecou < 0)  //数量减少
                {
                    var codes = ite.ExisBarcode.Take(ite.NeedCou);
                    barcodes.AddRange(codes);
                }
                else   //数量增加
                {
                    var codes = inves.Where(c => !ite.ExisBarcode.Contains(c.ProductBarcode)
                                                && c.ProductNumber == ite.ProductNumb)
                    .Take(takecou).Select(c => c.ProductBarcode).ToList();
                    barcodes.AddRange(codes);
                    barcodes.AddRange(ite.ExisBarcode);
                }
            }
            return Json(barcodes);
        }

        [HttpPost]
        public ActionResult MemberValid(string loginName, string passwd, int? storeId)
        {
            try
            {
                if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(passwd))
                {
                    return Json(new OperationResult(OperationResultType.Error, "请填写会员登录名"));
                }
                if (!storeId.HasValue)
                {
                    return Json(new OperationResult(OperationResultType.Error, "请选择店铺"));
                }

                var res = _memberContract.MemberRetailLogin(loginName, passwd, null, storeId.Value, MemberRetailLoginModeEnum.NORMAL_WITH_PASSWORD);

                return Json(res);

            }
            catch (Exception e)
            {
                _Logger.Error(e.Message + e.StackTrace);
                return Json(new OperationResult(OperationResultType.Error, "会员登录失败"));
            }
        }

        [HttpPost]
        public ActionResult ReleseInventoryLock(string barcodes)
        {
            try
            {
                var arr = barcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                _inventoryContract.SetInventoryUnLocked(arr);
                return Json(new OperationResult(OperationResultType.Success, string.Empty));
            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, e.Message));
            }
        }

        /// <summary>
        /// 获取优惠券信息
        /// </summary>
        /// <param name="cardNum"></param>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        public ActionResult DiscountCoupon(int? cardNum, string couponNum)
        {
            if (!cardNum.HasValue)
            {
                return Json(new OperationResult(OperationResultType.Error, "会员未登录时无法使用优惠券"));
            }
            var memberEntity = _memberContract.Members.FirstOrDefault(m => !m.IsDeleted && m.IsEnabled && m.UniquelyIdentifies == cardNum.Value.ToString());
            if (memberEntity == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "找不到该会员"));
            }
            if (string.IsNullOrEmpty(couponNum))
            {
                return Json(new OperationResult(OperationResultType.Error, "参数不可为空"));
            }
            //优惠券校验
            var couponItemEntity = _couponContract.CouponItems.Include(c => c.Coupon).FirstOrDefault(c => !c.IsDeleted && c.IsEnabled && c.CouponNumber == couponNum);
            if (couponItemEntity == null || couponItemEntity.Coupon == null)
            {
                return Json(new OperationResult(OperationResultType.Error, "优惠券不存在"));
            }
            //有效期校验
            if (!couponItemEntity.Coupon.IsForever)
            {
                if (couponItemEntity.Coupon.StartDate > DateTime.Now)
                {
                    return Json(new OperationResult(OperationResultType.Error, "优惠券尚未生效"));
                }
                if (couponItemEntity.Coupon.EndDate < DateTime.Now)
                {
                    return Json(new OperationResult(OperationResultType.Error, "优惠券已失效"));
                }
            }

            if (couponItemEntity.IsUsed)
            {
                return Json(new OperationResult(OperationResultType.Error, "优惠券之前已被使用"));
            }

            if (!couponItemEntity.MemberId.HasValue || couponItemEntity.MemberId.Value != memberEntity.Id)
            {
                return Json(new OperationResult(OperationResultType.Error, "此优惠券不属于该会员,无法使用"));
            }
            var retData = new
            {
                CouponName = couponItemEntity.Coupon.CouponName,
                CouponNum = couponItemEntity.CouponNumber,
                DiscountAmount = couponItemEntity.Coupon.CouponPrice
            };
            return Json(new OperationResult(OperationResultType.Success, string.Empty, retData));
        }

        /// <summary>
        /// 获取商品在所有仓库下可销售的数量
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        private int GetInventoryCountFromAllStore(int productId)
        {
            var opeid = AuthorityHelper.OperatorId;
            var count = _inventoryContract.Inventorys.Where(c => c.IsEnabled && !c.IsDeleted && c.ProductId == productId)
                .Where(c => !string.IsNullOrEmpty(c.ProductNumber)
                            && !string.IsNullOrEmpty(c.OnlyFlag)
                            && !string.IsNullOrEmpty(c.ProductBarcode))   //过滤垃圾数据
                .Where(c => !c.IsLock)
                .Where(c => c.Status == InventoryStatus.Default)
                .Count();
            return count;
        }

        /// <summary>
        /// 获取针对当前用户和当前店铺下的商品的可用库存数
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        private int GetInventoryAvailableCountFromCurrentStore(int productId, int storeId)
        {
            var optId = AuthorityHelper.OperatorId.Value;

            //根据用户对仓库的管理权限限定缩小查找范围
            var inventoryQuerySouce = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract);

            //根据店铺id,进一步缩小范围
            inventoryQuerySouce = inventoryQuerySouce.Where(i => i.StoreId == storeId);

            var barcodes = inventoryQuerySouce.Where(c => c.IsEnabled && !c.IsDeleted && c.ProductId == productId)
                                                .Where(c => !c.IsLock)
                                                .Where(c => !string.IsNullOrEmpty(c.ProductBarcode))
                                                .Where(c => c.Status == InventoryStatus.Default)
                                                .Select(i => i.ProductBarcode)
                                                .Distinct()
                                                .ToList();
            var count = barcodes.Count;
            //判断是否在缓存中被锁定
            foreach (var barcode in barcodes)
            {
                if (_inventoryContract.IsInventoryDisable(barcode, optId))
                {
                    count--;
                }
            }
            if (count < 0)
            {
                return 0;
            }
            return count;
        }

        /// <summary>
        /// 获取商品所属的仓库对应的店铺并返回当前用户有权限访问的仓库对应的店铺
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        private object GetStoresByProId(int proId)
        {
            List<int> storageIds = _storeContract.QueryManageStorageId(AuthorityHelper.OperatorId.Value);
            List<object> li = new List<object>();
            if (storageIds != null && storageIds.Any())
            {
                li =
                   _inventoryContract.Inventorys.Where(
                       c => c.IsEnabled && !c.IsDeleted && c.ProductId == proId && storageIds.Contains(c.StorageId))
                       .Select(x => new
                       {
                           x.StoreId,
                           x.Store.StoreName
                       }).DistinctBy(x => x.StoreId).ToList<object>();
            }
            return li;
        }



        private string[] GetProductNums(string[] nums)
        {
            if (nums != null && nums.Any())
            {
                for (int i = 0; i < nums.Count(); i++)
                {
                    var te = nums[i];
                    if (te.Length > 14)
                        nums[i] = te.Substring(0, te.Length - 3);
                }
                return nums;
            }
            else
            {
                throw new NullReferenceException("参数为空");
            }
        }

        private List<SalesCampaign> GetAvailableSalesCampaignsByStore(int storeId)
        {
            if (storeId <= 0)
            {
                throw new Exception("参数异常");
            }
            return _salesCampaignContract.GetAvailableSalesCampaignsByStore(storeId);

        }

        /// <summary>
        /// 根据商品的基本信息获取该商品可以参与的商品活动
        /// </summary>
        /// <param name="campaignList"></param>
        /// <param name="productId"></param>
        /// <returns></returns>
        private object GetSalesCampaignsByProId(List<SalesCampaign> campaignList, int productId)
        {
            if (campaignList == null || productId == 0)
            {
                throw new Exception("参数异常");
            }

            //获取原始款号
            var originNumberEntity = _productContract.Products.Single(c => c.Id == productId).ProductOriginNumber;

            //获取本款商品的所有商品活动
            var campaigns = campaignList.Where(c => c.ProductOriginNumbers.Contains(originNumberEntity))
                                        .Select(x => new
                                        {
                                            Id = x.Id,
                                            CampaignName = x.CampaignName,
                                            SalesCampaignType = (int)x.SalesCampaignType,
                                            MemberDiscount = x.MemberDiscount,
                                            NoMmebDiscount = x.NoMmebDiscount,
                                            OtherCashCoupon = x.OtherCashCoupon,
                                            OtherCampaign = x.OtherCampaign
                                        }).ToList<object>();
            return campaigns;
        }




        /// <summary>
        /// 生成零售订单号
        /// </summary>
        /// <returns></returns>
        private string GetOnlyNumb()
        {
            try
            {
                lock (_objlock)
                {
                    var maxid = _retailContract.Retails.OrderByDescending(c => c.Id).Select(c => c.Id).FirstOrDefault();
                    XKMath36.Math36 math = new Math36();
                    var basenum = math.To36(maxid);
                    var loopcou = 10 - basenum.Length;
                    Random random = new Random();
                    List<int> codebseli = new List<int>();
                    for (int i = 0; i < loopcou; i++)
                    {
                        codebseli.Add(random.Next(0, 36));
                    }
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < loopcou; i++)
                    {
                        var d = codebseli[i];
                        if (d <= 9)
                            sb.Append(d);
                        else
                        {
                            sb.Append((char)(d - 10 + 65));
                        }
                    }
                    return sb.ToString() + basenum;
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// 判断优惠券是否可用，不可用则返回错误信息
        /// </summary>
        /// <param name="couponNum"></param>
        /// <returns></returns>
        private Tuple<string, CouponItem> CouponValid(string couponNum)
        {
            string err = null;
            CouponItem coupon = null;
            if (!string.IsNullOrEmpty(couponNum))
            {
                coupon = _couponContract.CouponItems.FirstOrDefault(c => c.CouponNumber == couponNum);
                if (coupon == null)
                {
                    err = "优惠券不存在";
                }
                else if (coupon.IsUsed)
                {
                    err = "优惠券已使用";
                }
                else if (!coupon.IsEnabled)
                {
                    err = "优惠券被禁用";
                }
                else if (coupon.IsDeleted)
                {
                    err = "优惠券被删除";
                }
                else
                {
                    var time = DateTime.Now;

                    // 非永久有效的优惠券,需要校验有效期
                    if (!coupon.Coupon.IsForever)
                    {
                        if (coupon.Coupon.StartDate > time || time > coupon.Coupon.EndDate)
                        {
                            err = "优惠券已经超出了使用时间";
                        }
                    }
                }
            }
            return new Tuple<string, CouponItem>(err, coupon);
        }

        /// <summary>
        /// 是否获取积分
        /// </summary>
        /// <param name="totalConsume">总消费</param>
        /// <param name="ismemb">是否会员</param>
        /// <param name="scoreConsume">积分消费</param>
        /// <param name="cardMoneyConsume">储值消费</param>
        /// <returns></returns>
        private bool IsGetScore(ScoreRule scoreRuleEntity, decimal totalConsume, bool ismemb, decimal scoreConsume, decimal cardMoneyConsume, bool isBelongToCurrentStore)
        {
            if (!ismemb)
            {
                return false;
            }
            if (scoreRuleEntity == null)
            {
                return false;
            }

            if (totalConsume < (decimal)scoreRuleEntity.MinConsum)
            {
                return false;
            }

            if (!scoreRuleEntity.IsConsumeScoreGetScore && scoreConsume > 0)
            {
                return false;
            }
            if (!scoreRuleEntity.IsConsumeCardMoneyGetScore && cardMoneyConsume > 0)
            {
                return false;
            }

            // 非归属店铺
            if (!isBelongToCurrentStore && !scoreRuleEntity.CanGetScoreWhenNotBelongToStore)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 检出库存状态是否为可用
        /// </summary>
        /// <param name="barcodes"></param>
        /// <returns></returns>
        private Tuple<bool, string> CheckIsLockInveByBarcode(string[] barcodes)
        {
            string err = "";
            bool islock = false;

            //校验缓存中被锁定的库存
            foreach (var barcode in barcodes)
            {
                if (_inventoryContract.IsInventoryDisable(barcode, AuthorityHelper.OperatorId.Value))
                {
                    islock = true;
                    err += string.Format("{0}已经被锁定", barcode);
                }
            }

            var inventoryList = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                           .Where(c => barcodes.Contains(c.ProductBarcode));
            if (!inventoryList.Any())
            {
                err = "库存不存在";
                islock = true;
                return new Tuple<bool, string>(islock, err);
            }

            //检索数据库中处于非正常状态的库存
            inventoryList = inventoryList.Where(c => c.Status != InventoryStatus.Default);
            if (inventoryList.Any())
            {
                err = "";
                foreach (var t in inventoryList)
                {
                    if (t.Status == InventoryStatus.JoinOrder)
                    {
                        err += t.ProductBarcode + "已售出,";
                    }
                    else
                    {
                        err += "商品欠损或其他原因不可销售";
                    }
                }
                islock = true;
            }

            return new Tuple<bool, string>(islock, err);
        }

        /// <summary>
        /// 在缓存中锁定库存
        /// </summary>
        /// <param name="barcodes"></param>
        private void LockInvetory(string[] barcodes)
        {
            var lockList = CacheAccess.GetAccessibleInventorys(_inventoryContract, _storeContract)
                                            .Where(c => barcodes.Contains(c.ProductBarcode)).Include(i => i.Product)
                                            .Select(i => new LockInventoryDto
                                            {
                                                OperatorId = AuthorityHelper.OperatorId.Value,
                                                ProductBarcode = i.ProductBarcode,
                                            }).ToArray();
            _inventoryContract.SetInventoryLocked(TimeSpan.FromMinutes(5), lockList);
        }




        /// <summary>
        /// app登录确认推送
        /// </summary>
        /// <param name="loginName"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult PushAPPConfirmLogin(string loginName)
        {
            // 获取会员登录名
            if (string.IsNullOrEmpty(loginName))
            {
                return Json(OperationResult.Error("登录名不能为空"));
            }

            var memberIdQuery = _memberContract.Members.Where(c => !c.IsDeleted && c.IsEnabled)
                                              .Where(c => c.MemberName == loginName
                                                     || c.MobilePhone == loginName
                                                     || c.CardNumber == loginName
                                                     || c.UniquelyIdentifies == loginName)
                                              .Select(m => m.Id);
            if (!memberIdQuery.Any())
            {
                return Json(OperationResult.Error("未找到设备登录信息"));
            }


            // 查询会员的jpushId
            var memberId = memberIdQuery.First();
            var hashId = RedisCacheHelper.KEY_MEMBER_JPUSH;
            var jpushId = RedisCacheHelper.GetValueFromHash(hashId, memberId.ToString());
            // 推送给APP一条登录确认信息
            ApiMemberHub.SendNoti(new
            {
                context = "CONFIRM_LOGIN",
                title = "PC登录确认"
            }, memberId);

            if (!string.IsNullOrEmpty(jpushId))
            {
                var res = MobileAPIPushHelper.PushConfirmLogin(jpushId);

                if (!res)
                {
                    return Json(OperationResult.Error("推送信息失败"));
                }

            }




            return Json(new OperationResult(OperationResultType.Success, "推送成功", new { memberId = memberId }));
        }


        /// <summary>
        /// 查询app登录确认状态
        /// </summary>
        /// <param name="memberId"></param>
        /// <param name="storeId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult QueryLoginStat(int? memberId, int? storeId)
        {
            if (!memberId.HasValue || !storeId.HasValue)
            {
                return Json(OperationResult.Error("参数错误"));
            }

            var key = RedisCacheHelper.KEY_MEMBER_LOGIN_STAT_PREFIX + memberId.Value.ToString();

            if (!RedisCacheHelper.ContainsKey(key))
            {
                return Json(new OperationResult(OperationResultType.Success, ((int)RetailMemberLoginConfimStat.等待确认中).ToString(), null));
            }


            var hasConfirm = RedisCacheHelper.Get<int>(key);
            var hasRemove = RedisCacheHelper.Remove(key);
            if (!hasRemove)
            {
                _Logger.Error($"删除key失败key:{key}");
            }
            if (hasConfirm != 1)
            {
                return Json(new OperationResult(OperationResultType.Error, ((int)RetailMemberLoginConfimStat.已拒绝).ToString(), null));
            }
            var res = _memberContract.MemberRetailLogin(string.Empty, string.Empty, memberId, storeId.Value, MemberRetailLoginModeEnum.APP_CONFIRM);

            if (res.ResultType != OperationResultType.Success)
            {
                return Json(res);
            }
            return Json(new OperationResult(OperationResultType.Success, ((int)RetailMemberLoginConfimStat.已确认).ToString(), res.Data));

        }

        public enum RetailMemberLoginConfimStat
        {
            等待确认中 = 0,
            已确认 = 1,
            已拒绝 = 2
        }



        private void CheckParameter(string[] barcodes)
        {
            if (barcodes == null || barcodes.Length <= 0)
            {
                throw new Exception("商品条码不可为空");
            }
            if (barcodes.GroupBy(x => x).Any(g => g.Count() > 1))
            {
                throw new Exception("检测到重复的流水号");
            }
            if (Request.HttpMethod == "GET" && barcodes.Length > 50)
            {
                throw new Exception("商品条码数量过多");

            }
            else if (barcodes.Length > 200)
            {
                throw new Exception("商品条码数量过多");

            }

        }

        public ActionResult CheckBarcode(int storeId, string barcodes)
        {
            try
            {
                var adminId = AuthorityHelper.OperatorId.Value;
                if (string.IsNullOrEmpty(barcodes))
                {
                    return Json(OperationResult.Error("条码不能为空"));
                }
                var barcodeArr = barcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                CheckParameter(barcodeArr);

                barcodeArr.Each(b => b = b.Trim().ToUpper());

                if (barcodeArr.Any(b => b.Length != 14))
                {
                    return Json(OperationResult.Error("商品条码长度不符合要求"));
                }

                var dict = _inventoryContract.CheckBarcodes(InventoryCheckContext.零售, storeId, null,adminId, barcodeArr);
                if (dict.Any(d => !d.Value.Item1))
                {
                    return Json(new OperationResult(OperationResultType.Error, string.Empty, dict.Where(d => !d.Value.Item1)));
                }

                dict.Where(d => d.Value.Item1)
                    .Select(d => d.Key)
                    .ToList()
                    .ForEach(barcode => _inventoryContract.SetInventoryLocked(TimeSpan.FromMinutes(10), new LockInventoryDto { OperatorId = adminId, ProductBarcode = barcode }));
                return Json(new OperationResult(OperationResultType.Success, string.Empty, dict), JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }

        [HttpPost]
        public ActionResult CheckOrder(int storeId, string orderInfo)
        {
            try
            {
                var retailInfoDTO = JsonHelper.FromJson<RetaiInfoDTO>(orderInfo);
                if (retailInfoDTO.ConsumeInfo.OutStoreId <= 0)
                {
                    return Json(new OperationResult(OperationResultType.Error, "没有选择出货店铺"));
                }

                var barcodes = retailInfoDTO.Products.SelectMany(p => p.Barcodes);
                var res = CheckBarcode(storeId, string.Join(",", barcodes)) as JsonResult;
                var resType = res.Data as OperationResult<object>;
                if (resType.ResultType != OperationResultType.Success)
                {
                    return Json(res.Data, JsonRequestBehavior.AllowGet);
                }

                if (retailInfoDTO.IsMember)
                {
                    res = ProcessMemberRetail(retailInfoDTO, true);
                }
                else
                {
                    res = ProcessNonMemberRetail(retailInfoDTO, true);
                }
                return res;

            }
            catch (Exception e)
            {
                return Json(new OperationResult(OperationResultType.Error, "系统繁忙,请稍候再试"));
            }
        }



        /// <summary>
        /// 获取商品价格信息
        /// </summary>
        /// <param name="isMember">是否为会员价</param>
        /// <param name="barcodes">商品流水号</param>
        /// <returns></returns>
        public ActionResult GetProductsInfo(int storeId, string memberCard, string barcodes, bool isFirstQuery = true, string selectedSaleCamps = null)
        {
            try
            {
                var adminId = AuthorityHelper.OperatorId.Value;
                var res = CheckBarcode(storeId, barcodes) as JsonResult;
                var barcodeArr = barcodes.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var resType = res.Data as OperationResult<object>;
                if (resType.ResultType != OperationResultType.Success)
                {
                    return Json(res.Data, JsonRequestBehavior.AllowGet);
                }

                List<CustomSaleCampsEntry> customSelects;
                if (string.IsNullOrEmpty(selectedSaleCamps))
                {
                    customSelects = new List<CustomSaleCampsEntry>();
                }
                else
                {
                    customSelects = JsonHelper.FromJson<List<CustomSaleCampsEntry>>(selectedSaleCamps);
                    if (customSelects == null || selectedSaleCamps.Length <= 0)
                    {
                        throw new Exception("customSaleCamps参数错误");
                    }
                }

                resType = _retailContract.GetProductsInfo(storeId, adminId, memberCard, barcodeArr, isFirstQuery, customSelects);
                return Json(resType, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));

            }

        }




        /// <summary>
        /// 获取可用的会员优惠券,店铺活动及等级折扣
        /// </summary>
        /// <param name="storeId">店铺id</param>
        /// <param name="memberCard">会员卡号</param>
        /// <param name="selectedSaleCampIds">商品活动id</param>
        /// <returns></returns>
        public ActionResult GetEnableCoupon(int storeId, string memberCard, string selectedSaleCampIds)
        {
            try
            {
                var arr = new List<int>();
                if (!string.IsNullOrEmpty(selectedSaleCampIds))
                {
                    arr.AddRange(selectedSaleCampIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)));
                }
                var res = _retailContract.GetEnableCoupon(storeId, memberCard, arr.ToArray());
                return Json(res, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(OperationResult.Error(e.Message));
            }

        }
    }
}