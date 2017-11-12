using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Areas.Stores.Models;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.Utility.Extensions;
using Whiskey.Utility.Class;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using System.Linq.Expressions;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class MembSearchController : BaseController
    {
        protected readonly IMemberContract _memberContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IMemberTypeContract _memberTypeContract;
        protected readonly IAdministratorContract _adminContract;
        protected readonly IPermissionContract _permissionContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberActivityContract _memberActivityContract;
        protected readonly IAdjustDepositContract _adjustDepositContract;
        protected readonly IStorageContract _storageContract;
        protected readonly IRecommendMemberCollocationContract _recommendMemberCollocationContract;
        protected readonly IMemberCollocationContract _memberCollocationContract;
        protected readonly IMemberColloEleContract _memberColloEleContract;
        protected readonly IMemberLevelContract _memberLevelContract;
        protected readonly IMemberFaceContract _MemberFaceContract;

        public MembSearchController(IMemberContract memberContract,
            IStorageContract storageContract,
            IRetailContract retailContract,
            IMemberDepositContract memberDepositContract,
            IMemberTypeContract memberTypeContract,
            IAdministratorContract adminContract,
            IStoreContract storeContract,
            IMemberActivityContract memberActivityContract,
            IPermissionContract _permissionContract,
            IAdjustDepositContract adjustDepositContract,
            IRecommendMemberCollocationContract recommendMemberCollocationContract,
           IMemberCollocationContract memberCollocationContract,
           IMemberColloEleContract memberCollocationEleContract,
           IMemberFaceContract _MemberFaceContract,
           IMemberLevelContract memberLevelContract
            )
        {
            _storageContract = storageContract;
            _memberContract = memberContract;
            _retailContract = retailContract;
            _memberDepositContract = memberDepositContract;
            _memberTypeContract = memberTypeContract;
            _adminContract = adminContract;
            _storeContract = storeContract;
            _memberActivityContract = memberActivityContract;
            _adjustDepositContract = adjustDepositContract;
            this._permissionContract = _permissionContract;
            _recommendMemberCollocationContract = recommendMemberCollocationContract;
            _memberCollocationContract = memberCollocationContract;
            _memberColloEleContract = memberCollocationEleContract;
            _memberLevelContract = memberLevelContract;
            this._MemberFaceContract = _MemberFaceContract;
        }
        //
        // GET: /Stores/MembSearch/

        #region 私有方法
        /// <summary>
        /// 发全储值积分调整通知【异步操作】
        /// </summary>
        /// <param name="memberId">会员Id</param>
        /// <param name="balance">需要减少的储值金额</param>
        /// <param name="score">需要减少的积分</param>
        /// <returns></returns>
        private void SendAdjustDepositNotification(int memberId, decimal balance, decimal score)
        {
            ThreadPool.QueueUserWorkItem((obj) =>
            {
                var modMember = EntityContract._memberContract.Members.FirstOrDefault(w => w.Id == memberId);
                if (modMember.IsNotNull())
                {
                    var curmoduleId = Utils.GetCurrPageModuleId("/Members/AdjustDeposit", EntityContract._moduleContract);
                    if (curmoduleId.HasValue)
                    {
                        var receiveAdminIds = EntityContract._adminContract.Administrators.Where(w => !w.IsDeleted && w.IsEnabled && w.IsLogin).Select(s => s.Id).ToList();
                        receiveAdminIds = receiveAdminIds.Where(w => PermissionHelper.HasModulePermission(w, curmoduleId.Value, EntityContract._adminContract, EntityContract._permissionContract)).ToList();

                        if (receiveAdminIds.IsNotNullOrEmptyThis())
                        {
                            var modTN = EntityContract._templateNotificationContract.templateNotifications.FirstOrDefault(f => f.NotifciationType == TemplateNotificationType.ChangeRecharge);
                            if (modTN.IsNotNull())
                            {
                                var modTemp = modTN.Templates.FirstOrDefault(f => f.IsDefault && !f.IsDeleted && f.IsEnabled);
                                if (modTemp.IsNotNull())
                                {
                                    var title = modTemp.TemplateName ?? "储值积分调整申请通知";
                                    var content = modTemp.TemplateHtml ?? "$storeName会员 $memberName,$memberPhone";
                                    var store = modMember.Store;//用户所属店铺
                                    if (store.IsNotNull())
                                    {
                                        content = content.Replace("$storeId", store.Id.ToString()).Replace("$storeName", store.StoreName)
                                            .Replace("$storeAddress", store.Address).Replace("$storePhone", store.Telephone ?? store.MobilePhone);
                                    }

                                    content = content.Replace("$memberId", modMember.Id.ToString()).Replace("$sendTime", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                                                    .Replace("$memberName", modMember.MemberName ?? modMember.RealName).Replace("$memberPhone", modMember.MobilePhone)
                                                    .Replace("$balance", balance.ToString()).Replace("$score", score.ToString());

                                    var result = EntityContract._notificationContract.Insert(sendNotificationAction, new NotificationDto()
                                    {
                                        Title = title,
                                        AdministratorIds = receiveAdminIds,
                                        Description = content,
                                        IsEnableApp = true,
                                        NoticeTargetType = (int)NoticeTargetFlag.Admin,
                                        NoticeType = (int)NoticeFlag.Immediate
                                    });
                                }
                            }
                        }
                    }
                }
            });
        }

        #endregion

        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MemberCollocationSelect(int memberId)
        {
            // 获取已经推荐的搭配id
            var recommendCollocationIds = _recommendMemberCollocationContract.Entities
                                            .Where(m => !m.IsDeleted && m.IsEnabled && m.MemberId == memberId)
                                            .Select(m => m.MemberCollocationId)
                                            .ToList();
            ViewBag.RecommendIds = JsonHelper.ToJson(recommendCollocationIds);
            ViewBag.MemberId = memberId;
            return PartialView();
        }

        public ActionResult List()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Member>(req.FilterGroup);
            //var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var memb = _memberContract.Members.Where(pred).Where(c => c.IsEnabled && !c.IsDeleted)
                //.Where(c => !c.StoreId.HasValue || storeIds.Contains(c.StoreId.Value))
                .Select(c => new
                {
                    c.Id,
                    c.CardNumber,
                    c.Balance,
                    c.Score,
                    c.UpdatedTime,
                    c.UserPhoto,
                    c.Gender,
                    c.RealName,
                    c.UniquelyIdentifies,
                    c.MobilePhone,
                    c.Store.StoreName,
                    c.MemberType.MemberTypeName,
                    c.CreatedTime

                }).ToList();

            GridData<Object> data = new GridData<object>(memb, 1, req.RequestInfo);
            return Json(data, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult CollocationList(string collocationName, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {

            var memberId = _adminContract.GetMemberId(AuthorityHelper.OperatorId.Value);
            var query = _memberCollocationContract.MemberCollocations.Where(e => e.MemberId == memberId);
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (!string.IsNullOrEmpty(collocationName) && collocationName.Length > 0)
            {
                query = query.Where(e => e.CollocationName.StartsWith(collocationName));
            }
            string strApiUrl = ConfigurationHelper.GetAppSetting("ApiUrl");
            var elementQuery = _memberColloEleContract.MemberColloEles.Where(e => !e.IsDeleted && e.IsEnabled)
                                                                      .Where(e => !e.ParentId.HasValue);

            var joinQuery = query.Join(elementQuery, m => m.Id, e => e.MemberColloId.Value, (m, e) => new
            {
                m.Id,
                m.IsDeleted,
                m.IsEnabled,
                m.UpdatedTime,
                m.CollocationName,
                ImagePath = strApiUrl + e.ImagePath,
                IsChecked = false
            });
            var list = joinQuery
            .OrderByDescending(e => e.UpdatedTime)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = joinQuery.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 会员的消费记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ConsumeOrderList()
        {
            GridRequest req = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Retail>(req.FilterGroup);
            var retails = _retailContract.Retails
                .Where(pred)
                .OrderByDescending(c => c.CreatedTime)
                .Take(5)
                .Select(c => new
                {
                    c.Id,
                    c.RetailNumber,
                    c.ConsumeCount,
                    c.CashConsume,
                    Balance = c.StoredValueConsume,
                    c.SwipeConsume,
                    c.ScoreConsume,
                    c.RemainValue,
                    c.RemainScore,
                    c.Store.StoreName,
                    c.CreatedTime
                }).ToList();
            GridData<object> data = new GridData<object>(retails, 5, Request);
            return Json(data, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 会员的储值变动记录
        /// </summary>
        /// <returns></returns>
        public ActionResult BalanceOrderList()
        {
            GridRequest grq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<MemberDeposit>(grq.FilterGroup);
            List<MemberDepositInfo> li = new List<MemberDepositInfo>();
            //充值记录
            var depli =
                _memberDepositContract.MemberDeposits
                    .Where(pred)
                    .Where(c => c.MemberActivityType == 0)
                    .OrderByDescending(c => c.CreatedTime)
                    .Take(5).Select(c => new MemberDepositInfo()
                    {
                        CreateTime = c.CreatedTime,
                        AfterBalance = (float)c.AfterBalance,
                        BeforeBalance = (float)c.BeforeBalance,
                        Other = "充值",
                        MemberId = c.MemberId,
                        Operator = c.Operator.Member.MemberName,
                        MemberNumb = c.Member.UniquelyIdentifies
                    }).ToList();
            li.AddRange(depli);

            int memberid = -1;
            if (li.Any())
            {
                memberid = depli.FirstOrDefault().MemberId;
            }
            if (memberid != -1)
            {
                var dali =
                    _retailContract.Retails
                        .Where(c => c.ConsumerId == memberid && c.StoredValueConsume > 0)
                        .OrderByDescending(c => c.CreatedTime)
                        .Take(5)
                        .Select(c => new MemberDepositInfo()
                        {
                            MemberNumb = c.Consumer.UniquelyIdentifies,
                            CreateTime = c.CreatedTime,
                            BeforeBalance = (float)(c.StoredValueConsume + c.RemainValue),
                            AfterBalance = (float)c.RemainValue,
                            Operator = c.Operator.Member.MemberName,
                            Other = "消费"
                        }).ToList();
                li.AddRange(dali);
            }
            var resul = li.OrderByDescending(c => c.CreateTime).Take(5).Select(c => new
            {
                c.BeforeBalance,
                c.AfterBalance,
                c.Operator,
                c.CreateTime,
                Type = c.Other,
                c.MemberNumb
            }).ToList();
            GridData<object> da = new GridData<object>(resul, 5, Request);
            return Json(da, JsonRequestBehavior.AllowGet);

        }
        /// <summary>
        /// 会员的积分变动记录
        /// </summary>
        /// <returns></returns>
        public ActionResult ScoreOrderList()
        {
            GridRequest grq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<MemberDeposit>(grq.FilterGroup);
            List<MemberDepositInfo> li = new List<MemberDepositInfo>();
            //充值记录
            var depli =
                _memberDepositContract.MemberDeposits
                    .Where(pred)
                    .Where(c => c.MemberActivityType == MemberActivityFlag.Score)
                    .OrderByDescending(c => c.CreatedTime)
                    .Take(5).Select(c => new MemberDepositInfo()
                    {
                        MemberNumb = c.Member.UniquelyIdentifies,
                        CreateTime = c.CreatedTime,
                        BeforeScore = (float)c.BeforeScore,
                        AfterScore = (float)c.AfterScore,
                        Operator = c.Operator.Member.MemberName,
                        MemberId = c.MemberId,
                        MemberName = c.Member.MemberName,
                        Other = "充值"
                    }).ToList();
            li.AddRange(depli);

            int memberid = -1;
            if (li.Any())
            {
                memberid = depli.FirstOrDefault().MemberId;
            }
            if (memberid != -1)
            {
                var dali =
                    _retailContract.Retails
                        .Where(c => c.ConsumerId == memberid && c.ScoreConsume > 0)
                        .OrderByDescending(c => c.CreatedTime)
                        .Take(5)
                        .Select(c => new MemberDepositInfo()
                        {
                            MemberNumb = c.Consumer.UniquelyIdentifies,
                            CreateTime = c.CreatedTime,
                            BeforeScore = (float)(c.ScoreConsume + c.RemainScore),
                            AfterScore = (float)c.ScoreConsume,
                            Operator = c.Operator.Member.MemberName,
                            Other = "消费"
                        }).ToList();
                li.AddRange(dali);
            }
            var resul = li.OrderByDescending(c => c.CreateTime).Take(5).Select(c => new
            {
                c.MemberNumb,
                c.CreateTime,
                c.AfterScore,
                c.BeforeScore,
                c.Operator,
                Type = c.Other
            }).ToList();
            GridData<object> da = new GridData<object>(resul, 5, Request);
            return Json(da, JsonRequestBehavior.AllowGet);

        }


        #region 添加会员
        /// <summary>
        /// 添加会员界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            ViewBag.EnterpriseMemberTypeId = enterpriseBindId;

            var memberTypes = _memberTypeContract.SelectList(string.Empty);
            memberTypes.RemoveAll(m => m.Value == enterpriseBindId.ToString());
            ViewBag.MemberType = memberTypes;
           
            int adminId = AuthorityHelper.OperatorId ?? 0;
            MemberDto dto = new MemberDto();
            var store = _storeContract.QueryManageStore(adminId).Where(s => s.IsAttached).FirstOrDefault();
            if (store.IsNotNull())
            {
                dto.StoreId = store.Id;
                dto.StoreName = store.StoreName;
            }
            return PartialView(dto);
        }

        /// <summary>
        /// 添加会员
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Create(MemberDto dto)
        {
            OperationResult oper = _memberContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 修改密码
        public ActionResult UpdatePass(int Id)
        {
            MemberDto dto = new MemberDto();
            Member member = _memberContract.View(Id);
            dto.MemberName = member.MemberName;
            dto.Id = Id;
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult UpdatePass(MemberDto dto)
        {
            OperationResult oper = _memberContract.UpdatePassWord(dto);
            return Json(oper);
        }
        #endregion
        #region 修改所属店铺
        public ActionResult UpdateStore(int Id, int? StoreId)
        {
            Member member = _memberContract.View(Id);

            var res = _MemberFaceContract.MoveMemberToNewFaceSet(member.Id, StoreId);
            if (res.ResultType != OperationResultType.Success)
            {
                return Json(new OperationResult(OperationResultType.Error, "更新归属店铺失败,人脸信息更新失败"));
            }

            member.StoreId = StoreId;
            OperationResult oper = _memberContract.Update(member);
            return Json(oper);
        }

        #endregion

        #region 充值
        /// <summary>
        /// 初始化充值界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Recharge(int Id)
        {
            var result = _memberContract.Edit(Id);
            Member member = _memberContract.View(Id);
            ViewBag.MemberId = Id;
            ViewBag.MemberTypeId = result.MemberTypeId;
            var memberDepList = _memberDepositContract.MemberDeposits.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == Id);
            decimal price = 0;
            decimal score = 0;
            if (memberDepList.Count() > 0)
            {
                foreach (var memberDep in memberDepList)
                {
                    price = price + memberDep.Price;
                    score = score + memberDep.Score;
                }
            }
            ViewBag.Balance = member.Balance;
            ViewBag.Price = price;
            ViewBag.Score = score;
            MemberDepositDto dto = new MemberDepositDto();
            dto.MemberId = Id;

            return PartialView(dto);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult Recharge(MemberDepositDto dto)
        {
            OperationResult oper = _memberDepositContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 获取充值活动
        /// <summary>
        /// 获取充值活动
        /// </summary>
        /// <param name="MemberType">会员类型</param>
        /// <returns></returns>
        public JsonResult GetRechargeActivity(int MemberTypeId)
        {
            // 店铺权限
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            DateTime dateNow = Convert.ToDateTime(DateTime.Now.ToString("yy/MM/dd"));
            IQueryable<MemberActivity> listMemberActs = _memberActivityContract.MemberActivitys
                .Where(x => !x.IsDeleted && x.IsEnabled)
                .Where(x => x.IsForever == true || (dateNow.CompareTo(x.StartDate) >= 0) && dateNow.CompareTo(x.EndDate) <= 0) //有效期
                .Where(x => x.MemberTypes.Any(k => k.Id == MemberTypeId)); //会员类型


            var memberActs = listMemberActs.Select(x => new
            {
                x.Id,
                x.ActivityName,
                x.Score,
                x.Price,
                x.ActivityType,
                x.RewardMoney,
                x.StoreIds,
                x.IsAllStore
            }).ToList()
            .Select(x => new
            {
                x.Id,
                x.ActivityName,
                x.Score,
                x.Price,
                x.ActivityType,
                x.RewardMoney,
                StoreIds = string.IsNullOrEmpty(x.StoreIds) ? new List<int>() :
                                                             x.StoreIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                             .Select(i => int.Parse(i)).ToList(),
                x.IsAllStore
            }).ToList();

            //  过滤掉非全部店铺且参与店铺不在权限店铺范围的活动
            memberActs.RemoveAll(x => !x.IsAllStore && !x.StoreIds.Intersect(storeIds).Any());


            return Json(memberActs, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 申请对储值和积分的调整
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult AdjustDeposit(int Id)
        {
            AdjustDepositDto dto = new AdjustDepositDto();
            Member member = _memberContract.View(Id);
            dto.MemberName = member.MemberName;
            dto.MemberId = Id;
            ViewBag.Balance = member.Balance;
            ViewBag.Score = member.Score;
            dto.VerifyType = (int)VerifyFlag.Verifing;
            return PartialView(dto);
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AdjustDeposit(AdjustDepositDto dto, bool IsScorePlus = false, bool IsBalancePlus = false)
        {
            dto.Score = IsScorePlus ? Math.Abs(dto.Score) : -Math.Abs(dto.Score);
            dto.Balance = IsBalancePlus ? Math.Abs(dto.Balance) : -Math.Abs(dto.Balance);

            dto.ApplicantId = AuthorityHelper.OperatorId;
            OperationResult oper = _adjustDepositContract.Insert(dto);

            if (oper.ResultType == OperationResultType.Success)
            {
                SendAdjustDepositNotification(dto.MemberId.Value, dto.Balance, dto.Score);
            }

            return Json(oper);
        }
        #endregion

        #region 变更会员等级
        public ActionResult MemberType(int Id)
        {
            Member member = _memberContract.View(Id);
            List<SelectListItem> list = _memberTypeContract.SelectList(string.Empty);
            // 去除企业会员
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            ViewBag.EnterpriseMemberTypeId = enterpriseBindId;
            list.RemoveAll(s => s.Value == enterpriseBindId.ToString());
            foreach (SelectListItem item in list)
            {
                if (item.Value == member.MemberTypeId.ToString())
                {
                    item.Selected = true;
                }
            }
            ViewBag.ListItem = list;
            ViewBag.MemberId = Id;
            return PartialView(member);
        }

        [HttpPost]
        public JsonResult MemberType(int MemberTypeId, int MemberId, int? LevelId)
        {
            if (LevelId == 0)
            {
                LevelId = null;
            }
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            Member memberEntity = _memberContract.View(MemberId);



            // 更新为企业会员需要校验选中的会员等级
            if (MemberTypeId == enterpriseBindId)
            {
                if (!LevelId.HasValue || LevelId.Value <= 0)
                {
                    return Json(OperationResult.Error("企业会员需要选择会员等级类型"));
                }

                var level = _memberLevelContract.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == LevelId.Value).FirstOrDefault();
                if (level.UpgradeType != UpgradeType.企业)
                {
                    return Json(OperationResult.Error("请选择有效的企业会员等级类型"));
                }
                memberEntity.LevelId = LevelId.Value;
            }
            else
            {
                // 更新为非企业会员时,如果是之前是企业会员,那么之前的会员等级需要清空
                if (memberEntity.MemberTypeId == enterpriseBindId)
                {
                    memberEntity.LevelId = null;
                }
            }

            memberEntity.UpdatedTime = DateTime.Now;
            memberEntity.MemberTypeId = MemberTypeId;

            OperationResult oper = _memberContract.Update(memberEntity);
            return Json(oper);
        }
        #endregion

        /// <summary>
        /// 打印会员最近的充值记录
        /// </summary>
        /// <param name="memberId"></param>
        /// <returns></returns>
        public ActionResult PrintMemberLatestRechargeReceip(int memberId)
        {
            var entity = _memberDepositContract.MemberDeposits
                .Where(deposit => deposit.MemberId == memberId)
                .OrderByDescending(deposit => deposit.CreatedTime)
                .FirstOrDefault();
            return PartialView(entity);
        }


        public ActionResult SaveCollocationId(int memberId, params int[] recommendCollocationIds)
        {
            var res = _memberCollocationContract.SaveRecommendCollocationId(memberId, recommendCollocationIds);
            return Json(res);
        }



        public ActionResult GetActivityStores(int activityId)
        {
            // 获取店铺权限
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            if (storeIds == null)
            {
                return Json(OperationResult.Error("店铺权限不足"));
            }

            // 获取活动
            var activity = _memberActivityContract.View(activityId);
            if (activity == null)
            {
                return Json(OperationResult.Error("活动不存在"));
            }

            // 获取参与店铺
            if (activity.IsAllStore)
            {
                return Json(new OperationResult(OperationResultType.Success, string.Empty, storeIds));
            }
            else if (string.IsNullOrEmpty(activity.StoreIds))
            {
                return Json(OperationResult.Error("该活动未设置参与店铺"));
            }
            else
            {
                var idArr = activity.StoreIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(id => int.Parse(id));
                var resultId = idArr.Intersect(storeIds);
                return Json(new OperationResult(OperationResultType.Success, string.Empty, resultId));
            }


        }
    }
}