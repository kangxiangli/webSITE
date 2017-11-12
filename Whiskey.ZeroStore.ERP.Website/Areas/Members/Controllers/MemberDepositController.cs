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
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;
using Whiskey.Web.PaymentHelper;
using WxPayAPI;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Member;
using Whiskey.ZeroStore.ERP.Models.Enums;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Members.Controllers
{
    /// <summary>
    /// 充值
    /// </summary>
    [License(CheckMode.Verify)]
    public class MemberDepositController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(MemberDepositController));

        protected readonly IMemberDepositContract _memberdepositContract;

        protected readonly IMemberContract _memberContract;

        protected readonly IMemberActivityContract _memberActivityContract;

        protected readonly IAdjustDepositContract _adjustDepositContract;

        protected readonly IStoreValueRuleContract _storeValueRuleContract;
        protected readonly IMemberTypeContract _memberTypeContract;
        protected readonly IRechargeOrderContract _rechargeOrderContract;
        /// <summary>
        /// 构造函数初始化对象
        /// </summary>
        /// <param name="memberdepositContract"></param>
		public MemberDepositController(IMemberDepositContract memberdepositContract,
            IMemberContract memberContract,
            IMemberActivityContract memberActivityContract,
            IAdjustDepositContract adjustDepositContract,
            IStoreValueRuleContract storeValueRuleContract,
            IMemberTypeContract memberTypeContract,
            IRechargeOrderContract rechargeOrderContract)
        {
            _memberdepositContract = memberdepositContract;
            _memberContract = memberContract;
            _memberActivityContract = memberActivityContract;
            _adjustDepositContract = adjustDepositContract;
            _storeValueRuleContract = storeValueRuleContract;
            _memberTypeContract = memberTypeContract;
            _rechargeOrderContract = rechargeOrderContract;
        }
        #endregion

        #region 初始化操作界面
        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create(int Id, int AddFlag)
        {
            //从会员列表请求不显示返回上一级,Num=1 表示从会员列表进来
            string strNum = Request["Num"];
            var result = _memberContract.Edit(Id);
            Member member = _memberContract.View(Id);
            ViewBag.MemberId = Id;
            ViewBag.MemberTypeId = result.MemberTypeId;
            var memberDepList = _memberdepositContract.MemberDeposits.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.MemberId == Id);
            decimal price = 0;
            decimal score = 0;
            MemberDeposit memberDepListModel = new MemberDeposit();
            if (memberDepList.Count() > 0)
            {
                foreach (var memberDep in memberDepList)
                {
                    price = price + memberDep.Price;
                    score = score + memberDep.Score;
                }
                memberDepListModel = memberDepList.FirstOrDefault();
            }

            ViewBag.Balance = member.Balance;
            ViewBag.Price = price;
            ViewBag.Score = score;
            ViewBag.Num = strNum;
            ViewBag.AddFlag = AddFlag;
            return PartialView(memberDepListModel);
        }


        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(MemberDepositDto dto)
        {
            var result = _memberdepositContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(MemberDepositDto dto)
        {
            var result = _memberdepositContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _memberdepositContract.Edit(Id);
            return PartialView(result);
        }

        #endregion

        #region 数据详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _memberdepositContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 展示数据列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberDeposit, bool>> predicate = FilterHelper.GetExpression<MemberDeposit>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _memberdepositContract.MemberDeposits.Where<MemberDeposit, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    OrderType = m.OrderType.ToString(),
                    m.RelatedOrderNumber,
                    m.MemberId,
                    m.Member.RealName,
                    m.Member.MobilePhone,
                    m.Quotiety,
                    m.Price,
                    m.Cash,
                    m.Card,
                    m.Coupon,
                    m.Notes,
                    m.Score,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.order_Uid,
                    AdminName = m.Operator.Member.MemberName,
                    m.Operator.Department.DepartmentName,
                    StoreName = m.Store.StoreName ?? string.Empty,
                    DepositContext = m.DepositContext.HasValue ? m.DepositContext.ToString() : string.Empty
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
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
            var result = _memberdepositContract.Remove(Id);
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
            var result = _memberdepositContract.Delete(Id);
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
            var result = _memberdepositContract.Recovery(Id);
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
            var result = _memberdepositContract.Enable(Id);
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
            var result = _memberdepositContract.Disable(Id);
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
            var list = _memberdepositContract.MemberDeposits.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
        public ActionResult Export()
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            GridRequest request = new GridRequest(Request);
            Expression<Func<MemberDeposit, bool>> predicate = FilterHelper.GetExpression<MemberDeposit>(request.FilterGroup);

            var query = _memberdepositContract.MemberDeposits.Where(predicate);
            var list = query.Select(m => new
            {
                OrderType = m.OrderType + "",
                m.RelatedOrderNumber,
                m.MemberId,
                m.Member.RealName,
                m.Member.MobilePhone,
                m.Quotiety,
                m.Price,
                m.Cash,
                m.Card,
                m.Coupon,
                m.Notes,
                m.Score,
                m.UpdatedTime,
                m.order_Uid,
                AdminName = m.Operator.Member.MemberName,
                m.Operator.Department.DepartmentName,
                StoreName = m.Store.StoreName ?? string.Empty,
                DepositContext = m.DepositContext.HasValue ? m.DepositContext + "" : string.Empty
            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "充值记录管理");
        }
        #endregion

        #region 获取充值店铺
        /// <summary>
        /// 获取会员锁定的
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public JsonResult GetStores(int Id)
        {
            return null;
        }

        #endregion

        #region 初始化搜索会员界面
        /// <summary>
        /// 初始化搜索会员界面
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Search(int AddFlag)
        {
            ViewBag.AddFlag = AddFlag;
            return PartialView();
        }

        public async Task<ActionResult> SearchMember(string KeyWord)
        {
            string strKeyWord = string.Empty;
            strKeyWord = KeyWord.Replace("\"", string.Empty);
            GridRequest request = new GridRequest(Request);
            Expression<Func<Member, bool>> predicate = FilterHelper.GetExpression<Member>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                IQueryable<Member> listMember = _memberContract.Members;
                listMember = listMember.Where(x => x.MemberName.Contains(KeyWord) || x.MobilePhone.Contains(KeyWord) || x.CardNumber.Contains(KeyWord));
                List<int> ListId = new List<int>();
                if (listMember != null && listMember.Count() > 0)
                {
                    ListId = listMember.Select(x => x.Id).ToList();
                }
                IQueryable<AdjustDeposit> listAdjustDeposit = _adjustDepositContract.AdjustDeposits.Where(x => x.IsDeleted == false && x.IsEnabled == true && ListId.Contains(x.MemberId ?? 0));
                var list = listMember.Where<Member, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.MemberName,
                    m.CardNumber,
                    m.MobilePhone,
                    IsVerifing = listAdjustDeposit.Where(x => x.MemberId == m.Id && x.VerifyType == (int)VerifyFlag.Verifing).Count() > 0 ? true : false,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });

            return Json(data, JsonRequestBehavior.AllowGet);
        }


        #endregion

        #region 获取充值活动
        /// <summary>
        /// 获取充值活动
        /// </summary>
        /// <param name="MemberType"></param>
        /// <returns></returns>
        public JsonResult GetRechargeActivity(int MemberTypeId)
        {
            DateTime dateNow = DateTime.Now;
            IQueryable<MemberActivity> listMemberActs = _memberActivityContract.MemberActivitys.Where(x => x.IsDeleted == false && x.IsEnabled == true)
                .Where(x => (x.IsForever == true || (dateNow.CompareTo(x.StartDate) >= 0) && dateNow.CompareTo(x.EndDate) <= 0));
            listMemberActs = listMemberActs.Where(x => x.MemberTypes.Where(k => k.Id == MemberTypeId).Count() > 0);
            var memberActs = listMemberActs.Select(x => new
            {
                x.Id,
                x.ActivityName,
                x.Score,
                x.Price,
                x.ActivityType,
                x.RewardMoney,
            }).ToList();
            return Json(memberActs, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 申请修改储值积分数据
        public ActionResult CreateAdjust(int Id, int VerifyType, int AddFlag)
        {
            Member member = _memberContract.View(Id);
            AdjustDepositDto dto = new AdjustDepositDto();
            if (member != null)
            {
                dto.MemberId = member.Id;
                dto.MemberName = member.MemberName;
            }
            dto.VerifyType = VerifyType;
            ViewBag.AddFlag = AddFlag;
            return PartialView(dto);
        }

        [HttpPost]
        public JsonResult CreateAdjust(AdjustDepositDto dto)
        {
            if (dto.VerifyType == (int)VerifyFlag.Pass)
            {
                dto.ReviewersId = AuthorityHelper.OperatorId;
            }
            else if (dto.VerifyType == (int)VerifyFlag.Verifing)
            {
                dto.ApplicantId = AuthorityHelper.OperatorId;
            }
            OperationResult oper = _adjustDepositContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        public ActionResult RechargeWx(int Id, int AddFlag)
        {
            var memberType = _memberContract.Members.Where(x => x.Id == Id).Select(x => x.MemberTypeId).FirstOrDefault();
            var Price = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == true && x.RuleType == 0)
                .Select(c => new
                {
                    c.StoreValueName,
                    Name = c.MemberTypes.Select(x => x.Id),
                    c.Price,
                    c.RewardMoney,
                    c.Score,
                    c.StartDate,
                    c.EndDate,
                    c.IsForever,
                    c.Descrip,
                    c.Id,
                    c.IsDeleted,
                    c.IsEnabled,
                    c.Sequence,
                    c.UpdatedTime,
                    c.Operator.Member.MemberName

                }).ToList();
            string priceDiv = "";
            foreach (var item in Price)
            {
                if (item.Name.Contains(memberType))
                {
                    FashionData fd = new FashionData();
                    fd.SetValue("Id", item.Id);
                    fd.SetValue("Price", item.Price);
                    fd.SetValue("UserId", Id);
                    string sign = fd.MakeSign();
                    priceDiv += "<div class=\"col - md - 3\">";
                    priceDiv += "<button type=\"button\" class=\"btn btn-info btn - block\" data-index=\"" + item.Id + "\" data-amount=\"" + item.Price + "\" data-sign=\"" + sign + "\" style=\"height: 50px\">充" + (int)item.Price + "元送" + (int)item.RewardMoney + "元</button>";
                    priceDiv += "</div><br/>";
                }
            }

            var PriceForever = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == false && x.RuleType == 0)
                .Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                .Select(c => new
                {
                    c.StoreValueName,
                    Name = c.MemberTypes.Select(x => x.Id),
                    c.Price,
                    c.RewardMoney,
                    c.Score,
                    c.StartDate,
                    c.EndDate,
                    c.IsForever,
                    c.Descrip,
                    c.Id,
                    c.IsDeleted,
                    c.IsEnabled,
                    c.Sequence,
                    c.UpdatedTime,
                    c.Operator.Member.MemberName

                }).ToList();
            foreach (var item in PriceForever)
            {
                if (item.Name.Contains(memberType))
                {
                    FashionData fd = new FashionData();
                    fd.SetValue("Id", item.Id);
                    fd.SetValue("Price", item.Price);
                    fd.SetValue("UserId", Id);
                    string sign = fd.MakeSign();
                    priceDiv += "<div class=\"col - md - 3\">";
                    priceDiv += "<button type=\"button\" class=\"btn btn-info btn - block\" data-index=\"" + item.Id + "\"  data-amount=\"" + item.Price + "\"  data-sign=\"" + sign + "\"   style=\"height: 50px\">充" + (int)item.Price + "元送" + (int)item.RewardMoney + "元<br/>";
                    priceDiv += "活动时间：" + item.StartDate.ToString("D") + "-" + item.EndDate.ToString("D") + "</button>";
                    priceDiv += "</div><br/>";
                }
            }

            var Score = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == true && x.RuleType == 1)
                 .Select(c => new
                 {
                     c.StoreValueName,
                     Name = c.MemberTypes.Select(x => x.Id),
                     c.Price,
                     c.RewardMoney,
                     c.Score,
                     c.StartDate,
                     c.EndDate,
                     c.IsForever,
                     c.Descrip,
                     c.Id,
                     c.IsDeleted,
                     c.IsEnabled,
                     c.Sequence,
                     c.UpdatedTime,
                     c.Operator.Member.MemberName

                 }).ToList();
            string ScoreDiv = "";
            foreach (var item in Score)
            {
                if (item.Name.Contains(memberType))
                {
                    FashionData fd = new FashionData();
                    fd.SetValue("Id", item.Id);
                    fd.SetValue("Price", item.Price);
                    fd.SetValue("UserId", Id);
                    string sign = fd.MakeSign();
                    ScoreDiv += "<div class=\"col - md - 3\">";
                    ScoreDiv += "<button type=\"button\" class=\"btn btn-info btn - block\" data-index=\"" + item.Id + "\"  data-amount=\"" + item.Price + "\"  data-sign=\"" + sign + "\"   style=\"height: 50px\">充" + (int)item.Price + "元获得" + (int)item.Score + "积分</button>";
                    ScoreDiv += "</div><br/>";
                }
            }

            var ScoreForever = _storeValueRuleContract.StoreValueRules.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.IsForever == false && x.RuleType == 1)
                .Where(x => x.StartDate <= DateTime.Now && x.EndDate >= DateTime.Now)
                 .Select(c => new
                 {
                     c.StoreValueName,
                     Name = c.MemberTypes.Select(x => x.Id),
                     c.Price,
                     c.RewardMoney,
                     c.Score,
                     c.StartDate,
                     c.EndDate,
                     c.IsForever,
                     c.Descrip,
                     c.Id,
                     c.IsDeleted,
                     c.IsEnabled,
                     c.Sequence,
                     c.UpdatedTime,
                     c.Operator.Member.MemberName

                 }).ToList();
            foreach (var item in ScoreForever)
            {
                if (item.Name.Contains(memberType))
                {
                    FashionData fd = new FashionData();
                    fd.SetValue("Id", item.Id);
                    fd.SetValue("Price", item.Price);
                    fd.SetValue("UserId", Id);
                    string sign = fd.MakeSign();
                    ScoreDiv += "<div class=\"col - md - 3\">";
                    ScoreDiv += "<button type=\"button\" class=\"btn btn-info btn - block\" data-index=\"" + item.Id + "\"  data-amount=\"" + item.Price + "\"  data-sign=\"" + sign + "\"   style=\"height: 50px\">充" + (int)item.Price + "元获得" + (int)item.Score + "积分<br/>";
                    ScoreDiv += "活动时间：" + item.StartDate.ToString("D") + "-" + item.EndDate.ToString("D") + "</button>";
                    ScoreDiv += "</div><br/>";
                }
            }
            ViewData["priceDiv"] = priceDiv;
            ViewData["ScoreDiv"] = ScoreDiv;
            ViewData["UserId"] = Id;
            return PartialView();
        }


        public ActionResult GenerateQRcode(int Id, decimal Amount, string sign, int memberId)
        {

            try
            {
                FashionData fd = new FashionData();
                fd.SetValue("Id", Id);
                fd.SetValue("Price", Amount);
                fd.SetValue("UserId", memberId);
                fd.SetValue("sign", sign);
                if (fd.CheckSign())
                {
                    var Price = _storeValueRuleContract.StoreValueRules.Where(x => x.Id == Id).ToList().FirstOrDefault();

                    if (Price != null)
                    {
                        if (Amount == Price.Price)
                        {
                            Dictionary<string, object> m_value = new Dictionary<string, object>();
                            string bodyStr = "";
                            //商品信息描述
                            if (Price.RuleType == 0)
                            {
                                bodyStr = "零时尚充值中心-储值充值";
                            }
                            else
                            {
                                bodyStr = "零时尚充值中心-积分充值";
                            }
                            RechargeOrder order = new RechargeOrder();
                            order.Amount = Convert.ToDecimal(Amount);
                            order.RechargeType = (MemberActivityFlag)Price.RuleType;
                            order.RuleTypeId = Convert.ToInt32(Price.Id);
                            if (Price.RuleType == 0)
                            {
                                //储值
                                order.TureAmount = Convert.ToDecimal(Price.RewardMoney + Price.Price);
                            }
                            else if (Price.RuleType == 1)
                            {
                                //积分
                                order.TureAmount = Convert.ToDecimal(Price.Score);
                            }
                            order.Pay_Type = Convert.ToInt32(3);
                            order.UserId = memberId;
                            //订单号生成规则 当前时间+随机5位数
                            string out_trade_no = DateTime.Now.ToString("yyyyMMddHHmmssffff").ToString() + new Random().Next(10000, 99999).ToString();
                            order.order_Uid = out_trade_no;
                            var res = _rechargeOrderContract.Insert(order);
                            if (res.ResultType == OperationResultType.Success)
                            {
                                var product_id = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                                    .Select(x => x.Id).FirstOrDefault();
                                WxPayData wxdata = new WxPayData();
                                wxdata.SetValue("body", bodyStr);//商品描述 
                                wxdata.SetValue("attach", "test");
                                wxdata.SetValue("out_trade_no", out_trade_no);
                                //总金额    订单总金额，只能为整数，详见支付金额   注意：金额不带小数点，最小为分，即1元=100分，传数据即可
                                wxdata.SetValue("total_fee", 1);
                                wxdata.SetValue("time_start", DateTime.Now.ToString("yyyyMMddHHmmss"));
                                wxdata.SetValue("time_expire", DateTime.Now.AddMinutes(10).ToString("yyyyMMddHHmmss"));
                                wxdata.SetValue("trade_type", "NATIVE");
                                wxdata.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));
                                wxdata.SetValue("product_id", product_id);
                                wxdata.SetValue("appid", PaymentConfigHelper.Wx_XD_AppId);
                                wxdata.SetValue("mch_id", PaymentConfigHelper.Wx_XD_MCHID);//商户号
                                wxdata.SetValue("spbill_create_ip", PaymentConfigHelper.Ip);//终端ip
                                WxPayData result = WxPayApi.UnifiedOrder(wxdata, 6, 2, 2);
                                ViewData["out_trade_no"] = out_trade_no;
                                if (result.GetValue("return_code").ToString() == "SUCCESS")
                                {
                                    if (result.GetValue("code_url").ToString() != "")
                                    {
                                        string url = result.GetValue("code_url").ToString();//获得统一下单接口返回的二维码链接
                                        ViewBag.image = ImageHelper.CreateQRCode(url, Guid.NewGuid().ToString().Replace("-", ""));
                                    }
                                    else
                                    {
                                        ViewBag.error = "此次充值异常";
                                    }
                                }
                                else
                                {
                                    ViewBag.error = "此次充值异常";
                                }
                            }
                            else
                            {
                                ViewBag.error = "此次充值异常";
                            }
                        }
                        else
                        {
                            ViewBag.error = "充值金额有误";
                        }
                    }
                    else
                    {
                        ViewBag.error = "此次充值异常";
                    }
                }
                else
                {
                    ViewBag.error = "此次充值异常";
                }
            }
            catch (Exception e)
            {
                ViewBag.error = "此次充值异常";
            }
            return PartialView();
        }

        //获取订单支付状态
        //public string GetWxOrderPayStatus(string out_trade_no)
        //{
        //    var a = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no).FirstOrDefault();
        //    var b = (from c in _rechargeOrderContract.RechargeOrders
        //             where c.order_Uid == out_trade_no
        //             select c.pay_status).FirstOrDefault();
        //    return b.ToString();
        //}

        //查询微信支付订单状态
        public string GetWxOrderPayStatus(string out_trade_no)
        {
            WxPayData data = new WxPayData();
            data.SetValue("appid", PaymentConfigHelper.Wx_XD_AppId);
            data.SetValue("mch_id", PaymentConfigHelper.Wx_XD_MCHID);//商户号
            data.SetValue("nonce_str", Guid.NewGuid().ToString().Replace("-", ""));
            data.SetValue("transaction_id", "");
            data.SetValue("out_trade_no", out_trade_no);
            WxPayData result = WxPayApi.OrderQuery(data, 6, 2);
            string return_code = result.GetValue("return_code").ToString();
            string result_code = result.GetValue("result_code").ToString();

            string trade_state = result.GetValue("trade_state").ToString();//订单交易状态

            if (return_code == "SUCCESS" && result_code == "SUCCESS")
            {
                switch (trade_state)
                {

                    case "SUCCESS":
                        #region 支付成功处理
                        string transaction_id = result.GetValue("transaction_id").ToString();
                        var status = _rechargeOrderContract.RechargeOrders.Where(x => x.Prepay_Id == transaction_id && x.order_Uid == out_trade_no)
                        .Select(x => x.pay_status).ToList().FirstOrDefault();
                        if (status != 1)
                        {
                            using (var a = _rechargeOrderContract.GetTransaction())
                            {

                                //status 0 订单生成 1 支付成功 2 支付失败
                                var res = _rechargeOrderContract.PaymentSuccess(transaction_id, 1, out_trade_no);
                                if (res.ResultType == OperationResultType.Error)
                                {
                                    a.Rollback();
                                }
                                else
                                {

                                    var b = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no)
                                        .Select(x => new MemberDepositDto()
                                        {
                                            MemberId = x.UserId,
                                            Price = x.RechargeType == MemberActivityFlag.Recharge ? x.TureAmount : 0,
                                            Score = x.RechargeType == MemberActivityFlag.Score ? x.TureAmount : 0,
                                            Card = x.Amount,
                                            order_Uid = x.order_Uid,
                                            MemberDepositType = MemberDepositFlag.System,
                                            MemberActivityType = x.RechargeType
                                        }).FirstOrDefault();
                                    var updateMember = _memberdepositContract.InsertWx(b);
                                    if (updateMember.ResultType == OperationResultType.Error)
                                    {
                                        a.Rollback();
                                    }
                                    else
                                    {
                                        //更新成功  
                                        a.Commit();
                                    }
                                }
                            }
                        }
                        #endregion
                        break;
                    case "PAYERROR":
                        #region 支付失败处理
                        var resFaile = _rechargeOrderContract.PaymentSuccess(null, 2, out_trade_no);
                        #endregion
                        break;
                    case "REFUND":
                        #region 转入退款
                        #endregion 
                        break;
                    case "NOTPAY":
                        #region 未支付
                        #endregion 
                        break;
                    case "CLOSED":
                        #region 已关闭
                        #endregion 
                        break;
                    case "REVOKED":
                        #region 已撤销（刷卡支付）
                        #endregion 
                        break;
                    case "USERPAYING":
                        #region 用户支付中
                        #endregion 
                        break;
                }
            }
            var order = _rechargeOrderContract.RechargeOrders.Where(x => x.order_Uid == out_trade_no).FirstOrDefault();
            var orderState = (from c in _rechargeOrderContract.RechargeOrders
                              where c.order_Uid == out_trade_no
                              select c.pay_status).FirstOrDefault();
            return orderState.ToString();
        }
    }
}
