using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Globalization;
using AutoMapper;
using Antlr3;
using Antlr3.ST;
using Antlr3.ST.Language;
using Antlr3.ST.Extensions;
using Newtonsoft.Json;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Web.Mvc.Binders;
using Whiskey.Core.Data;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Utility.Helper;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;
using System.Web.Script.Serialization;
using Whiskey.ZeroStore.ERP.Transfers.MemberPhoto;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.Core.Data.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{

    [License(CheckMode.Verify)]
    public class MemberController : BaseController
    {

        #region 声明业务层操作对象
        //日志记录
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberController));
        //声明业务层操作对象
        protected readonly IMemberContract _memberContract;

        protected readonly IStorageContract _storageContract;

        protected readonly IStoreContract _storeContract;

        protected readonly ICollocationContract _collocationContract;

        protected readonly IMemberTypeContract _memberTypeContract;

        protected readonly IAdministratorContract _administratorContract;

        protected readonly IColorContract _colorContract;

        protected readonly IMemberDepositContract _memberDepositContract;

        protected readonly IMemberLevelContract _memberlevelContract;

        protected readonly IAdministratorContract _adminContract;

        public readonly IEntryContract _entryContract;
        public readonly IMemberIMFriendContract _memberIMFriendContract;
        //protected readonly IMemberRoleContract _memberRoleContract;

        protected readonly IDepartmentContract _departmentContract;
        protected readonly IRetailContract _retailContract;
        protected readonly IMemberHeatContract _memberHeatContract;
        protected readonly IAppointmentContract _appointmentContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;

        protected readonly IMemberFigureContract _MemberFigureContract;
        protected readonly IConfigureContract _configureContract;

        //构造函数-初始化业务层操作对象
        public MemberController(IMemberContract memberContract,
            IStorageContract storageContract,
            IStoreContract storeContract,
            ICollocationContract collocationContract,
            IMemberTypeContract memberTypeContract,
            IAdministratorContract administratorContract,
            IColorContract colorContract,
            IMemberDepositContract memberDepositContract,
            IMemberLevelContract memberlevelContract,
            IAdministratorContract adminContract,
            IDepartmentContract departmentContract,
            IRetailContract retailContract,
            IMemberHeatContract memberHeatContract,
            IAppointmentContract appointmentContract,
            IMemberIMFriendContract memberIMFriendContract,
            IMemberFigureContract MemberFigureContract,
            //IMemberRoleContract memberRoleContract,
            IEntryContract entryContract,
            ICollocationQuestionnaireContract collocationQuestionnaireContract,
            IConfigureContract configureContract)
        {
            _memberContract = memberContract;
            _storageContract = storageContract;
            _storeContract = storeContract;
            _collocationContract = collocationContract;
            _memberTypeContract = memberTypeContract;
            _administratorContract = administratorContract;
            _colorContract = colorContract;
            _memberDepositContract = memberDepositContract;
            _memberlevelContract = memberlevelContract;
            _adminContract = adminContract;
            _entryContract = entryContract;
            _retailContract = retailContract;
            _memberHeatContract = memberHeatContract;
            _appointmentContract = appointmentContract;
            _memberIMFriendContract = memberIMFriendContract;
            //_memberRoleContract = memberRoleContract;
            _collocationQuestionnaireContract = collocationQuestionnaireContract;
            _MemberFigureContract = MemberFigureContract;
        }
        #endregion        

        #region 初始化会员管理界面
        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            string title = "请选择";
            ViewBag.MemberType = _memberTypeContract.SelectList(title);
            List<SelectListItem> list = new List<SelectListItem>();
            list.Add(new SelectListItem()
            {
                Value = "",
                Text = "请选择"
            });
            var memberLevel = _memberlevelContract.MemberLevels.Where(x =>
            x.IsDeleted == false && x.IsEnabled == true).Select(x =>
                  new SelectListItem()
                  {
                      Value = x.Id.ToString(),
                      Text = x.LevelName
                  }).ToList();
            if (memberLevel != null)
            {
                list.AddRange(memberLevel);
            }
            ViewBag.MemberLevel = list;
            return View();
        }
        #endregion

        #region 获取会员列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var quotiety = from a in _memberDepositContract.MemberDeposits
                           where !a.IsDeleted && a.IsEnabled
                           group a by new { a.MemberId } into g
                           orderby g.FirstOrDefault().UpdatedTime
                           select new
                           {
                               MemberId = g.FirstOrDefault().MemberId,
                               quotiety = g.FirstOrDefault().Quotiety
                           };

            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

            var memBer = _memberContract.Members.Where(x => storeIds.Contains(x.StoreId.Value));// || x.StoreId == null

            var data = await Task.Run(() =>
            {
                var count = 0;
                var nowDate = DateTime.Now;
                var list = memBer.Where<Member, int>(predicate, request.PageCondition, out count);
                var listHeats = _memberHeatContract.Entities;
                var dataSouce = from m in list
                                let lastRetail = _retailContract.Retails.Where(w => w.ConsumerId == m.Id).OrderByDescending(o => o.CreatedTime).FirstOrDefault()
                                let lastRetailDays = lastRetail != null ? System.Data.Entity.DbFunctions.DiffDays(lastRetail.CreatedTime, nowDate) : null
                                let modHeat = lastRetailDays == null ? listHeats.FirstOrDefault(f => f.DayStart == null && f.DayEnd == null) :
                                                                    listHeats.Where(f => f.DayEnd >= lastRetailDays || (f.DayStart <= lastRetailDays && f.DayEnd == null))
                                                                    .OrderBy(o => o.DayStart).ThenByDescending(b => b.DayEnd).FirstOrDefault()
                                join b in quotiety
                                on m.Id equals b.MemberId into g
                                from y in g.DefaultIfEmpty()
                                select new
                                {
                                    MembNumber = m.UniquelyIdentifies,
                                    m.Store.StoreName,
                                    m.LevelId,
                                    m.RegisterType,
                                    m.MemberName,
                                    m.Balance,
                                    m.Score,
                                    m.CardNumber,
                                    m.MobilePhone,
                                    m.RealName,
                                    m.Gender,
                                    m.UserPhoto,
                                    m.IsLockedStore,
                                    m.Id,
                                    m.IsDeleted,
                                    m.IsEnabled,
                                    m.UpdatedTime,
                                    m.CreatedTime,
                                    m.MemberLevel.LevelName,
                                    AdminName = m.Operator.Member.MemberName,
                                    Name = m.MemberType.MemberTypeName,
                                    quotiety = y == null ? 1 : y.quotiety,
                                    HotLevel = modHeat != null ? modHeat.IconPath : "",
                                    HotLevelTip = modHeat != null ? modHeat.HeatName : "",
                                };
                var dat = dataSouce.ToList();
                return new GridData<object>(dataSouce, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 通过手机号获取会员信息

        public JsonResult GetMemberInfo(string Phone)
        {
            if (Phone.IsNullOrEmpty())
            {
                return Json(new OperationResult(OperationResultType.ValidError, "手机号无效"));
            }
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var query = _memberContract.Members.Where(x => x.IsEnabled && !x.IsDeleted && storeIds.Contains(x.StoreId.Value) && x.MobilePhone == Phone);
            var queryFi = _MemberFigureContract.MemberFigures.Where(w => w.IsEnabled && !w.IsDeleted).OrderByDescending(o => o.Id);
            var data = (from s in query
                        let f = queryFi.Where(w => w.MemberId == s.Id)
                        let d = s.MemberDeposits.Where(w => w.IsEnabled && !w.IsDeleted)
                        select new
                        {
                            MemberId = s.Id,
                            s.MemberName,
                            s.CardNumber,
                            s.Balance,
                            s.Score,
                            HistoryBalance = d.Any() ? d.Sum(s => s.Price) : 0,
                            HistoryScore = d.Any() ? d.Sum(s => s.Score) : 0,
                            s.Store.StoreName,
                            s.MemberLevel.LevelName,
                            s.MemberType.MemberTypeName,
                            s.MobilePhone,
                            UserPhoto = s.UserPhoto != null ? WebUrl + s.UserPhoto : s.UserPhoto,
                            s.CreatedTime,
                            s.MemberTypeId,
                            MemberFigure = f.Select(ss => new
                            {
                                ss.ApparelSize,
                                Birthday = ss.Birthday,
                                ss.PreferenceColor,
                                ss.Bust,
                                ss.FigureDes,
                                ss.FigureType,
                                ss.Gender,
                                ss.Height,
                                ss.Hips,
                                ss.Shoulder,
                                ss.Waistline,
                                ss.Weight
                            }).FirstOrDefault()

                        }).FirstOrDefault();
            return Json(OperationHelper.ReturnOperationResult(data != null, "获取个人信息", data));

        }

        #endregion

        #region 获取会员消费储值积分列表
        /// <summary>
        /// 获取消费列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberConsumeList(int MemberId, int PageIndex = 1, int PageSize = 5)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var hasPer = _memberContract.Members.Where(x => x.IsEnabled && !x.IsDeleted && storeIds.Contains(x.StoreId.Value) && x.Id == MemberId).Any();
            if (!hasPer)
            {
                return Json(new OperationResult(OperationResultType.ValidError, "会员不存在"));
            }

            PageIndex = PageIndex > 0 ? PageIndex : 1;
            PageSize = PageSize > 0 ? PageSize : 1;

            var query = _retailContract.Retails.Where(w => w.IsEnabled && !w.IsDeleted && w.ConsumerId == MemberId);

            var totalCount = query.Count();
            var totalPage = (int)Math.Ceiling((double)totalCount / PageSize);

            var list = (from s in query
                        orderby s.CreatedTime descending
                        select new
                        {
                            s.RetailNumber,
                            s.ConsumeCount,
                            s.CashConsume,
                            Balance = s.StoredValueConsume,
                            s.SwipeConsume,
                            s.ScoreConsume,
                            s.RemainValue,
                            s.RemainScore,
                            s.Store.StoreName,
                            s.CreatedTime,
                        }).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

            var data = new
            {
                totalCount = totalCount,
                totalPage = totalPage,
                List = list,
            };

            return Json(OperationHelper.ReturnOperationResult(true, "获取消费记录", data));
        }
        /// <summary>
        /// 获取储值列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberRechargeList(int MemberId, int PageIndex = 1, int PageSize = 5)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var hasPer = _memberContract.Members.Where(x => x.IsEnabled && !x.IsDeleted && storeIds.Contains(x.StoreId.Value) && x.Id == MemberId).Any();
            if (!hasPer)
            {
                return Json(new OperationResult(OperationResultType.ValidError, "会员不存在"));
            }

            PageIndex = PageIndex > 0 ? PageIndex : 1;
            PageSize = PageSize > 0 ? PageSize : 1;

            var query = ((from s in _memberDepositContract.MemberDeposits.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId && w.MemberActivityType == MemberActivityFlag.Recharge)
                          orderby s.CreatedTime descending
                          select new
                          {
                              CreateTime = s.CreatedTime,
                              AfterBalance = (float)s.AfterBalance,
                              BeforeBalance = (float)s.BeforeBalance,
                              TypeFlag = "充值",
                              Operator = s.Operator.Member.MemberName,
                          }).Union(from s in _retailContract.Retails.Where(w => w.IsEnabled && !w.IsDeleted && w.ConsumerId == MemberId && w.StoredValueConsume > 0)
                                   orderby s.CreatedTime descending
                                   select new
                                   {
                                       CreateTime = s.CreatedTime,
                                       AfterBalance = (float)s.RemainValue,
                                       BeforeBalance = (float)(s.StoredValueConsume + s.RemainValue),
                                       TypeFlag = "消费",
                                       Operator = s.Operator.Member.MemberName,
                                   })).OrderByDescending(o => o.CreateTime);

            var totalCount = query.Count();
            var totalPage = (int)Math.Ceiling((double)totalCount / PageSize);

            var list = query.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

            var data = new
            {
                totalCount = totalCount,
                totalPage = totalPage,
                List = list,
            };
            return Json(OperationHelper.ReturnOperationResult(true, "获取储值变动记录", data));
        }
        /// <summary>
        /// 获取积分列表
        /// </summary>
        /// <returns></returns>
        public JsonResult GetMemberScoreList(int MemberId, int PageIndex = 1, int PageSize = 5)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var hasPer = _memberContract.Members.Where(x => x.IsEnabled && !x.IsDeleted && storeIds.Contains(x.StoreId.Value) && x.Id == MemberId).Any();
            if (!hasPer)
            {
                return Json(new OperationResult(OperationResultType.ValidError, "会员不存在"));
            }

            PageIndex = PageIndex > 0 ? PageIndex : 1;
            PageSize = PageSize > 0 ? PageSize : 1;

            var query = ((from s in _memberDepositContract.MemberDeposits.Where(w => w.IsEnabled && !w.IsDeleted && w.MemberId == MemberId && w.MemberActivityType == MemberActivityFlag.Score)
                          orderby s.CreatedTime descending
                          select new
                          {
                              CreateTime = s.CreatedTime,
                              AfterBalance = (float)s.AfterBalance,
                              BeforeBalance = (float)s.BeforeBalance,
                              TypeFlag = "充值",
                              Operator = s.Operator.Member.MemberName,
                          }).Union(from s in _retailContract.Retails.Where(w => w.IsEnabled && !w.IsDeleted && w.ConsumerId == MemberId && w.ScoreConsume > 0)
                                   orderby s.CreatedTime descending
                                   select new
                                   {
                                       CreateTime = s.CreatedTime,
                                       AfterBalance = (float)s.ScoreConsume,
                                       BeforeBalance = (float)(s.ScoreConsume + s.RemainScore),
                                       TypeFlag = "消费",
                                       Operator = s.Operator.Member.MemberName,
                                   })).OrderByDescending(o => o.CreateTime);

            var totalCount = query.Count();
            var totalPage = (int)Math.Ceiling((double)totalCount / PageSize);

            var list = query.Skip((PageIndex - 1) * PageSize).Take(PageSize).ToList();

            var data = new
            {
                totalCount = totalCount,
                totalPage = totalPage,
                List = list,
            };
            return Json(OperationHelper.ReturnOperationResult(true, "获取积分变动记录", data));
        }



        #endregion

        #region 添加会员
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Create()
        {
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            ViewBag.EnterpriseMemberTypeId = enterpriseBindId;

            ViewBag.MemberType = _memberTypeContract.SelectList(string.Empty);
            ViewBag.MemberLevels = _memberlevelContract.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled && m.UpgradeType == UpgradeType.企业).Select(m => new SelectListItem
            {
                Text = m.LevelName,
                Value = m.Id.ToString()
            }).ToList();
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToDictionary(x => x.ColorName, x => x.IconPath);
            ViewBag.Color = dic;
            int adminId = AuthorityHelper.OperatorId ?? 0;
            Administrator admin = _administratorContract.View(adminId);
            MemberDto dto = new MemberDto();
            if (admin != null)
            {
                Store store = _storeContract.Stores.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.DepartmentId == admin.DepartmentId && x.IsAttached == true);
                if (store != null)
                {
                    dto.StoreId = store.Id;
                    dto.StoreName = store.StoreName;
                }
            }
            return View(dto);
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(MemberDto dto, MemberFigure fig)
        {
            if (dto.LevelId == 0)
            {
                dto.LevelId = null;
            }
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            var memberType = _memberTypeContract.MemberTypes.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == dto.MemberTypeId).FirstOrDefault();
            if (memberType.Id == enterpriseBindId)
            {
                if (!dto.LevelId.HasValue || dto.LevelId <= 0)
                {
                    return Json(OperationResult.Error("企业会员需要选择会员等级类型"));
                }

                var level = _memberlevelContract.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == dto.LevelId.Value).FirstOrDefault();
                if (level.UpgradeType != UpgradeType.企业)
                {
                    return Json(OperationResult.Error("请选择有效的企业会员等级类型"));
                }
            }
            else //创建非企业会员,会员等级默认为null
            {
                dto.LevelId = null;
            }


            string apparelSize = Request["ShangZhuang"] + "," + Request["XiaZhuang"];
            string figureDes = Request["FigureDes"];
            string color = Request["PreferenceColor"];
            fig.ApparelSize = apparelSize;
            fig.PreferenceColor = color;
            fig.FigureDes = figureDes;
            fig.Birthday = dto.DateofBirth;
            dto.MemberFigures.Add(fig);
            var result = _memberContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 修改会员信息

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Layout]
        public ActionResult Update(int Id)
        {
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            ViewBag.EnterpriseMemberTypeId = enterpriseBindId;

            ViewBag.MemberType = _memberTypeContract.SelectList(string.Empty);
            var result = _memberContract.Edit(Id);
            Store store = _storeContract.Stores.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == result.StoreId).FirstOrDefault();
            Collocation coll = _collocationContract.Collocations.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.Id == result.CollocationId).FirstOrDefault();
            if (store != null)
            {
                result.StoreName = store.StoreName;
            }
            if (coll != null)
            {
                result.CollocationName = coll.CollocationName;
            }

            ViewBag.MemberLevels = _memberlevelContract.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled && m.UpgradeType == UpgradeType.企业).Select(m => new SelectListItem
            {
                Text = m.LevelName,
                Value = m.Id.ToString()
            }).ToList();

            return View(result);
        }

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Update(MemberDto dto)
        {
            if (dto.LevelId == 0)
            {
                dto.LevelId = null;
            }
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            Member memberEntity = _memberContract.View(dto.Id);

            // 更新为企业会员需要校验选中的会员等级
            if (dto.MemberTypeId == enterpriseBindId)
            {
                if (!dto.LevelId.HasValue || dto.LevelId <= 0)
                {
                    return Json(OperationResult.Error("企业会员需要选择会员等级类型"));
                }

                var level = _memberlevelContract.MemberLevels.Where(m => !m.IsDeleted && m.IsEnabled && m.Id == dto.LevelId.Value).FirstOrDefault();
                if (level.UpgradeType != UpgradeType.企业)
                {
                    return Json(OperationResult.Error("请选择有效的企业会员等级类型"));
                }

            }
            else
            {
                // 更新为非企业会员时,如果是之前是企业会员,那么之前的会员等级需要清空
                if (memberEntity.MemberTypeId == enterpriseBindId)
                {
                    dto.LevelId = null;
                }
            }


            var result = _memberContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 查看数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _memberContract.View(Id);
            string shangSize = string.Empty;
            string xiaSize = string.Empty;
            string preColor = string.Empty;
            string figureType = string.Empty;
            string fgigureDes = string.Empty;
            string height = string.Empty;
            string weight = string.Empty;
            string shoulder = string.Empty;
            string bust = string.Empty;
            string waistline = string.Empty;
            string hips = string.Empty;
            string age = string.Empty;
            decimal quotiety = 0;
            MemberDeposit memberDeposit = _memberDepositContract.MemberDeposits
                   .Where(x => x.IsDeleted == false && x.IsEnabled == true)
                   .OrderByDescending(x => x.Id)
                   .FirstOrDefault(x => x.MemberId == Id);
            if (memberDeposit.IsNotNull())
            {
                quotiety = memberDeposit.Quotiety;
            }
            if (result.MemberFigures.Count > 0)
            {
                MemberFigure memberFigures = result.MemberFigures.OrderByDescending(x => x.Id).FirstOrDefault();
                if (memberFigures.ApparelSize.IsNotNullAndEmpty())
                {
                    string[] size = memberFigures.ApparelSize.Split(',');
                    if (size.Length == 1)
                    {
                        shangSize = size[0];
                    }
                    if (size.Length > 1)
                    {
                        shangSize = size[0];
                        xiaSize = size[1];
                    }
                }
                preColor = memberFigures.PreferenceColor;
                figureType = memberFigures.FigureType;
                fgigureDes = memberFigures.FigureDes;
                height = memberFigures.Height.ToString();
                weight = memberFigures.Weight.ToString();
                shoulder = memberFigures.Shoulder.ToString();
                bust = memberFigures.Bust.ToString();
                waistline = memberFigures.Waistline.ToString();
                hips = memberFigures.Hips.ToString();
                if (memberFigures.Birthday.HasValue)
                {
                    age = DateTime.Now.Year - memberFigures.Birthday.Value.Year + "";
                }
            }
            ViewBag.ShangSize = shangSize;
            ViewBag.XiaSize = xiaSize;
            ViewBag.PreColor = preColor;
            ViewBag.FigureType = figureType;
            ViewBag.FigureDes = fgigureDes;
            ViewBag.Height = height;
            ViewBag.Weight = weight;
            ViewBag.Shoulder = shoulder;
            ViewBag.Bust = bust;
            ViewBag.Waistline = waistline;
            ViewBag.Hips = hips;
            ViewBag.Quotiety = quotiety;
            ViewBag.MemberId = Id;
            ViewBag.Age = age;

            #region 线上咨询师问卷调查回答信息
            IDictionary<string, string[]> dic = new Dictionary<string, string[]>();

            var names = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == Id).GroupBy(c => new { c.QuestionName }).Select(c => c.Key).ToArray();

            foreach (var name in names)
            {
                string[] values = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == Id && c.QuestionName == name.QuestionName && !c.IsDeleted && c.IsEnabled).Select(c => c.Content).ToArray();

                KeyValuePair<string, string[]> item = new KeyValuePair<string, string[]>(name.QuestionName, values);

                dic.Add(item);
            }

            ViewBag.CollocationQuestionnaireDic = dic;
            #endregion
            ViewBag.Friends = _memberIMFriendContract.GetMyFriends(Id).Select(s => new MemberDto
            {
                UserPhoto = s.UserPhoto,
                MemberName = s.MemberName,
            }).ToList();
            return PartialView(result);
        }
        #endregion

        #region 移除数据
        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _memberContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 删除数据
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _memberContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 恢复数据
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _memberContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 启用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _memberContract.Enable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 禁用数据
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _memberContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印数据
        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _memberContract.Members.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 导出数据
        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _memberContract.Members.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult ExportMember(int? storeId, int? memberTypeId, int? levelId, string memberName, string realName, int? gender, string mobilePhone, DateTime? startDate, DateTime? endDate)
        {

            try
            {


                var query = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled);
                if (storeId.HasValue)
                {
                    query = query.Where(m => m.StoreId == storeId.Value);
                }
                if (gender.HasValue)
                {

                    query = query.Where(m => m.Gender == gender.Value);

                }
                if (memberTypeId.HasValue)
                {
                    query = query.Where(m => m.MemberTypeId == memberTypeId.Value);
                }
                if (levelId.HasValue)
                {

                    query = query.Where(m => m.LevelId == levelId.Value);
                }

                if (startDate.HasValue)
                {

                    query = query.Where(m => m.CreatedTime >= startDate.Value);

                }

                if (endDate.HasValue)
                {

                    query = query.Where(m => m.CreatedTime <= endDate.Value);

                }


                if (!string.IsNullOrEmpty(memberName) && memberName.Length > 0)
                {

                    query = query.Where(m => m.MemberName.StartsWith(memberName));

                }


                if (!string.IsNullOrEmpty(realName) && realName.Length > 0)
                {

                    query = query.Where(m => m.RealName.StartsWith(realName));

                }


                if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
                {

                    query = query.Where(m => m.MobilePhone.StartsWith(mobilePhone));

                }




                var depositQuery = _memberDepositContract.MemberDeposits.Where(d => !d.IsDeleted && d.IsEnabled)
                                                                        .Where(d => d.MemberActivityType == MemberActivityFlag.Recharge)
                                                                        .Where(d => d.Card > 0 || d.Card > 0)
                                                                        .Where(d => d.Quotiety >= 0 && d.Quotiety <= 1);

                var res = query.GroupJoin(depositQuery, m => m.Id, d => d.MemberId, (m, d) => new
                {
                    m.UniquelyIdentifies,
                    StoreName = m.Store.StoreName ?? string.Empty,
                    m.RealName,
                    Gender = m.Gender == 0 ? "女" : "男",
                    m.MobilePhone,
                    m.MemberType.MemberTypeName,
                    LevelName = m.MemberLevel.LevelName ?? "无等级",
                    Quotiety = d.OrderByDescending(i => i.CreatedTime).Select(i => (decimal?)i.Quotiety).FirstOrDefault() ?? 1,
                    m.Balance,
                    m.Score,
                    m.CreatedTime
                }).ToList();


                var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

                var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
                var st = group.GetInstanceOf("ExporterMember");
                st.SetAttribute("list", res);
                var str = st.ToString();
                var buffer = Encoding.UTF8.GetBytes(str);
                var stream = new MemoryStream(buffer);
                return File(stream, "application/ms-excel", "会员记录.xls");


            }
            catch (Exception e)
            {

                return Json(new OperationResult(OperationResultType.Error, e.Message), JsonRequestBehavior.AllowGet);
            }

        }

        #endregion

        #region 上传照片
        [Layout]
        public ActionResult MemberPhoto(int Id)
        {
            ViewBag.MemberId = Id;
            return View();
        }

        /// <summary>
        /// 上传照片
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UploadMemberPhoto()
        {
            try
            {
                string strMemberId = Request["MemberId"];
                if (string.IsNullOrEmpty(strMemberId))
                {
                    return Json(new { ResultType = OperationResultType.Error, Path = "" });
                }
                else
                {
                    int memberId = int.Parse(strMemberId);
                    string conPath = ConfigurationHelper.GetAppSetting("SaveMemberPhoto");
                    string avatar_data = Request["avatar_data"];
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    MemberPhotoData data = js.Deserialize<MemberPhotoData>(avatar_data);
                    var avatar_file = Request.Files["avatar_file"];
                    Guid gid = Guid.NewGuid();
                    string fileName = gid.ToString();
                    fileName = fileName.Substring(0, 15) + ".jpg";
                    DateTime now = DateTime.Now;
                    conPath = conPath + now.Year.ToString() + "/" + now.Month.ToString() + "/" + now.Day.ToString() + "/" + now.ToString("HH") + "/" + fileName;
                    int width = int.Parse(data.Width.ToString("0"));
                    int height = int.Parse(data.Height.ToString("0"));
                    int x = int.Parse(data.X_Axis.ToString("0"));
                    int y = int.Parse(data.Y_Axis.ToString("0"));
                    string savePath = ImageHelper.MakeThumbnail(avatar_file.InputStream, conPath, width, height, x, y, "Jpg");
                    if (string.IsNullOrEmpty(savePath))
                    {
                        return Json(new OperationResult(OperationResultType.Error, "上传失败"));
                    }
                    else
                    {
                        var oper = _memberContract.UploadImage(memberId, conPath);
                        return Json(oper);
                    }
                }


            }
            catch (Exception ex)
            {
                _Logger.Error<string>(ex.ToString());
                return Json(new { ResultType = OperationResultType.Error, Path = "" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region 重置密码发送到用户邮箱
        /// <summary>
        /// 重置密码发送到用户邮箱
        /// </summary>
        /// <param name="Id">主键Id</param>
        /// <returns></returns>
        public JsonResult RestPassWord(int Id)
        {
            var result = _memberContract.RestPassWord(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion         

        #region 获取店铺列表

        /// <summary>
        /// 初始化店铺界面
        /// </summary>
        /// <returns></returns>
        public ActionResult StoreList()
        {
            return PartialView();
        }


        /// <summary>
        /// 获取店铺列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetStoreList()
        {


            int adminId = AuthorityHelper.OperatorId ?? 0;
            GridRequest request = new GridRequest(Request);
            Expression<Func<Store, bool>> predicate = FilterHelper.GetExpression<Store>(request.FilterGroup);
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);
            var query = _storeContract.Stores.Where(s => storeIds.Contains(s.Id));

            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = query.Where<Store, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.StoreName,
                    StoreType = m.StoreType.TypeName,
                    m.Id
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取搭配师列表
        /// <summary>
        /// 初始化搭配师界面
        /// </summary>
        /// <returns></returns>
        public ActionResult CollocationList()
        {
            return PartialView();
        }

        /// <summary>
        /// 获取搭配师列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> GetCollocationList()
        {
            GridRequest request = new GridRequest(Request);
            var data = await Task.Run(() =>
            {
                var rule = request.FilterGroup.Rules.Where(x => x.Field == "MemberName");
                string strMemberName = null;
                IQueryable<Member> listMemeber = _memberContract.Members.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                if (rule.Count() > 0)
                {
                    strMemberName = rule.Select(x => x.Value).FirstOrDefault().ToString();
                    if (!string.IsNullOrEmpty(strMemberName))
                    {
                        listMemeber = _memberContract.Members.Where(x => x.MemberName == strMemberName);
                    }
                }

                IQueryable<Administrator> listAdmin = _administratorContract.Administrators.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                IQueryable<Collocation> listCollo = _collocationContract.Collocations.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                //Expression<Func<Collocation, bool>> predicate = FilterHelper.GetExpression<Collocation>(request.FilterGroup);
                var listTemp = from a in listAdmin
                               join
                               c in listCollo
                               on
                               a.Id equals c.AdminiId
                               join
                               m in listMemeber
                               on
                               a.Member.UniquelyIdentifies equals m.UniquelyIdentifies
                               select new
                               {
                                   c.Id,
                                   m.MemberName
                               };
                var count = 0;
                count = listTemp == null ? 0 : listTemp.Count();
                int pageIndex = request.PageCondition.PageIndex;
                int pageSize = request.PageCondition.PageSize;
                var list = listTemp.OrderByDescending(x => x.Id).Skip(pageIndex * pageSize).Take(pageSize).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取没有成为员工的会员数据
        public async Task<ActionResult> MemberList()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            List<int> entryList = _entryContract.Entrys.Where(x => !x.IsDeleted && x.IsEnabled).Select(x => x.MemberId).Distinct().ToList();
            var data = await Task.Run(() =>
            {
                var count = 0;
                List<string> listCode = _administratorContract.Administrators.Select(x => x.Member.UniquelyIdentifies).ToList();
                IQueryable<Member> listMember = _memberContract.Members.Where(x => !(listCode.Contains(x.UniquelyIdentifies)) && !entryList.Contains(x.Id));
                var list = listMember.Where<Member, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    MembNumber = m.UniquelyIdentifies,
                    m.Store.StoreName,
                    m.LevelId,
                    m.RegisterType,
                    m.MemberName,
                    m.MemberPass,
                    m.MemberTypeId,
                    m.Balance,
                    m.Score,
                    m.CardNumber,
                    m.Email,
                    m.MobilePhone,
                    m.RealName,
                    m.Gender,
                    m.DateofBirth,
                    m.UserPhoto,
                    m.LoginCount,
                    m.LoginTime,
                    m.RecommendId,
                    m.Notes,
                    m.IsLockedStore,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    AdminName = m.Operator.Member.MemberName,
                    Name = m.MemberType.MemberTypeName
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
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
            ViewBag.StoreId = result.StoreId ?? 0;
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

        public ActionResult RechargeGap()
        {
            //Utility.XmlHelper helper = new Utility.XmlHelper("Member", "RechargeGap");
            //var mod = helper.GetElement("GapValue");
            //ViewBag.GapValue = (mod?.Value ?? "30").CastTo<int>();
            ViewBag.GapValue = _configureContract.GetConfigureValue("Member","RechargeGap", "GapValue", "30");
            return PartialView();
        }

        [HttpPost]
        public JsonResult RechargeGap(string gapValue)
        {
            //Utility.XmlHelper helper = new Utility.XmlHelper("Member", "RechargeGap", true);
            //var status = helper.ModifyElement("GapValue", gapValue);
            var status = _configureContract.SetConfigure("Member","RechargeGap", "GapValue", gapValue);
            return Json(OperationHelper.ReturnOperationResult(status, "更新充值间隔"));
        }

        #endregion



        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult PurchaseHistoryList(int memberId, string retailNumber, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _retailContract.Retails;
            query = query.Where(e => e.IsEnabled == isEnabled && e.ConsumerId.Value == memberId);
            if (!string.IsNullOrEmpty(retailNumber) && retailNumber.Length > 0)
            {
                query = query.Where(e => e.RetailNumber.StartsWith(retailNumber));
            }

            var listQuery = query
                .SelectMany(e => e.RetailItems.Where(i => !i.IsDeleted && i.IsEnabled).SelectMany(i => i.RetailInventorys).Select(inve => new
                {
                    inve.RetailItem.Retail.RetailNumber,
                    ThumbnailPath = inve.Inventory.Product.ThumbnailPath ?? inve.Inventory.Product.ProductOriginNumber.ThumbnailPath,
                    inve.Inventory.ProductBarcode,
                    inve.Inventory.Product.ProductName,
                    inve.Inventory.Product.ProductOriginNumber.Brand.BrandName,
                    inve.Inventory.Storage.StorageName,
                    inve.RetailItem.ProductTagPrice,
                    inve.RetailItem.ProductRetailPrice,
                    inve.RetailItem.Retail.UpdatedTime

                }));
            var list = listQuery.OrderBy(r => r.UpdatedTime).Skip((pageIndex - 1) * pageSize).Take(pageSize);





            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = listQuery.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult AppointmentHistoryList(int memberId, string productNumber, AppointmentState? state, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _appointmentContract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled && e.MemberId == memberId);
            if (!string.IsNullOrEmpty(productNumber) && productNumber.Length > 0)
            {
                query = query.Where(e => e.ProductNumber.StartsWith(productNumber));
            }
            if (state.HasValue)
            {
                query = query.Where(p => p.State == state.Value);
            }
            var listQuery = query.OrderByDescending(s => s.UpdatedTime)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize)
                .Select(s => new
                {
                    s.Id,
                    s.MemberId,
                    s.Member.RealName,
                    s.Member.UserPhoto,
                    s.Member.MobilePhone,
                    s.Store.StoreName,

                    s.ProductNumber,

                    s.CreatedTime,
                    s.State
                })
                .ToList()
                .Select(s => new
                {
                    s.Id,
                    s.MemberId,
                    s.RealName,
                    s.StoreName,
                    s.ProductNumber,
                    s.MobilePhone,
                    s.UserPhoto,
                    CreatedTime = s.CreatedTime.ToString("yyyy-MM-dd HH:mm"),
                    State = s.State.ToString()
                }).ToList();





            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = listQuery,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }




        #region 初始化批量导出界面
        public ActionResult BatchImport()
        {
            return PartialView();
        }
        #endregion

        #region 上传Excel表格
        public JsonResult ExcelFileUpload()
        {
            var res = new OperationResult(OperationResultType.Error);
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                string fileName = file.FileName;
                string savePath = Server.MapPath("/Content/UploadFiles/Excels/") + DateTime.Now.ToString("yyyyMMddHH");
                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }
                string fullName = savePath + "\\" + fileName;

                if (System.IO.File.Exists(fullName))
                {
                    System.IO.File.Delete(fullName);
                }
                file.SaveAs(fullName);
                var reda = ExcelToJson(fullName);
                System.IO.File.Delete(fullName);
                if (reda.Any())
                {
                    var list = reda.Select(s => new { RealName = s[0], MobilePhone = s[1], Gender = s[2], Store = string.Empty }).ToList();
                    res = new OperationResult(OperationResultType.Success, string.Empty, list);
                }
            }
            return Json(res);
        }
        #endregion

        #region 读取Excel文件

        private List<List<String>> ExcelToJson(string fileName)
        {
            if (System.IO.File.Exists(fileName))
            {
                var da = new List<List<String>>();
                if (Path.GetExtension(fileName) == ".txt")
                {
                    string st = System.IO.File.ReadAllText(fileName);
                    var retda = st.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    var li = new List<List<string>>();
                    retda.Each(c =>
                    {
                        var t = new List<string>() { c };
                        li.Add(t);
                    });
                    da = li;
                }
                else
                {
                    YxkSabri.ExcelUtility excel = new YxkSabri.ExcelUtility();
                    da = excel.ExcelToArray(fileName);
                }
                return da;
            }
            return null;
        }
        #endregion

        public ActionResult SaveMember(DateTime? CreateStartDate, DateTime? CreateEndDate, params MemberImportEntry[] members)
        {
            if (members == null || members.Length <= 0)
            {
                return Json(OperationResult.Error("数据不能为空"));
            }
            var res = _memberContract.BatchImport(CreateStartDate, CreateEndDate, members);
            return Json(res);

        }



        public ActionResult QueryStoreBalance(int? storeId)
        {
            var storeIds = new List<int>();
            var storeName = "所有店铺";
            if (storeId.HasValue)
            {
                storeIds.Add(storeId.Value);
                storeName = _storeContract.Stores.FirstOrDefault(s => s.Id == storeId.Value).StoreName;
            }
            else
            {
                storeIds.AddRange(_storeContract.Stores.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => s.Id).ToList());
            }
            var memberQuery = _memberContract.Members.Where(m => !m.IsDeleted && m.IsEnabled && m.StoreId.HasValue && storeIds.Contains(m.StoreId.Value));
            object res;
            if (!memberQuery.Any())
            {
                res = new { };
            }
            else
            {
                res = new
                {
                    StoreName = storeName,
                    BalanceAmount = memberQuery.Sum(m => m.Balance),
                    ScoreAmount = memberQuery.Sum(m => m.Score)
                };
            }
            return Json(new OperationResult(OperationResultType.Success, string.Empty, res));


        }

        public ActionResult UpdateFigure(int Id)
        {
            MemberFigure memberFigures = _MemberFigureContract.MemberFigures.Where(x => x.MemberId == Id && x.IsEnabled && !x.IsDeleted).OrderByDescending(o => o.Id).FirstOrDefault();
            var list = _colorContract.Colors.Where(x => x.IsDeleted == false && x.IsEnabled == true && !x.ColorName.Contains("其他")).Select(s => new SelectListItem()
            {
                Text = s.ColorName,
                Value = s.ColorName,
            }).ToList();
            ViewBag.Colors = list;

            if (memberFigures.IsNull())
            {
                memberFigures = new MemberFigure() { MemberId = Id };
            }

            return PartialView(memberFigures);
        }

        [HttpPost]
        public JsonResult UpdateFigure(MemberFigureDto dto)
        {
            if (dto.Id != 0)
            {
                return Json(_MemberFigureContract.Update(dto));
            }
            else
            {
                return Json(_MemberFigureContract.Insert(dto));
            }
        }

        public JsonResult GetMemberType()
        {
            var enterpriseBindId = RedisCacheHelper.Get<int>(RedisCacheHelper.EnterpriseMemberTypeId);
            var list = _memberTypeContract.MemberTypes.Where(m => !m.IsDeleted && m.IsEnabled && m.Id != enterpriseBindId).Select(m => new
            {
                m.MemberTypeName,
                m.Id
            }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, list), JsonRequestBehavior.AllowGet);
        }

        public JsonResult SetMemberType(int memberId,int typeId)
        {
            return Json(_memberContract.SwitchMemberType(memberId, typeId));
        }
    }
}
