using System;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Web.Helper;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Models.Enums;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Models.DTO;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Web;
using System.IO;
using Antlr3.ST;
using Antlr3.ST.Language;
using Whiskey.Utility.Extensions;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Stores.Controllers
{
    [License(CheckMode.Verify)]
    public class AppointmentController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(StoreTypeController));

        protected readonly IAppointmentContract _appointmentContract;
        protected readonly ICategoryContract _categoryContract;
        protected readonly IBrandContract _brandContract;
        protected readonly IColorContract _colorContract;
        protected readonly IStoreContract _storeContract;
        protected readonly IMemberContract _memberContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly ICollocationQuestionnaireContract _collocationQuestionnaireContract;
        protected readonly ICollocationPlanContract _collocationPlanContract;
        public AppointmentController(
            IAppointmentContract appointmentContract,
            ICategoryContract categoryContract,
            IBrandContract brandContract,
            IColorContract colorContract,
            IStoreContract storeContract,
            IMemberContract memberContract,
            IMemberDepositContract memberDepositContract,
            ICollocationQuestionnaireContract collocationQuestionnaireContract,
            ICollocationPlanContract collocationPlanContract
            )
        {
            _appointmentContract = appointmentContract;
            _categoryContract = categoryContract;
            _brandContract = brandContract;
            _colorContract = colorContract;
            _storeContract = storeContract;
            _memberContract = memberContract;
            _memberDepositContract = memberDepositContract;
            _collocationQuestionnaireContract = collocationQuestionnaireContract;
            _collocationPlanContract = collocationPlanContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            return PartialView();
        }

        /// <summary>
        /// 打印搭配方案
        /// </summary>
        /// <param name="numb">销售编号</param>
        /// <returns></returns>
        public ActionResult PrintPlan(int Id)
        {
            var entity = _appointmentContract.Entities.Include(c => c.SelectedPlan).FirstOrDefault(c => c.Id == Id);
            ViewBag.Barcodes = JsonHelper.ToJson(entity.AppointmentPacking.AppointmentPackingItem.Select(i => i.ProductBarcode).ToList());
            var figure = entity.Member.MemberFigures.Select(f => new
            {
                f.Birthday,
                f.Height,
                f.Weight,
                f.Shoulder,
                f.PreferenceColor,
                f.Waistline,
                f.Hips,
                f.Gender,
                f.FigureType,
                f.FigureDes,
                f.Bust,
                f.ApparelSize
            }).FirstOrDefault();
            Func<string, string> getTopSize = str =>
            {
                if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                {
                    return string.Empty;
                }
                if (str.IndexOf(',') == -1)
                {
                    return string.Empty;
                }
                return str.Split(",")[0];

            };
            Func<string, string> getBottomSize = str =>
            {
                if (string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str))
                {
                    return string.Empty;
                }
                if (str.IndexOf(',') == -1)
                {
                    return string.Empty;
                }
                return str.Split(",")[1];

            };
            var figureData = new
            {
                Age = figure.Birthday.HasValue ? (DateTime.Now.Year - figure.Birthday.Value.Year).ToString() : "暂缺",
                Height = figure.Height,
                Weight = figure.Weight,
                Shoulder = figure.Shoulder,
                PreferenceColor = figure.PreferenceColor,
                Waistline = figure.Waistline,
                Hips = figure.Hips,
                Gender = figure.Gender,
                FigureType = figure.FigureType,
                FigureDesc = figure.FigureDes,
                Bust = figure.Bust,
                TopSize = getTopSize(figure.ApparelSize),
                BottomSize = getBottomSize(figure.ApparelSize)
            };
            
            ViewBag.FigureData = JsonHelper.ToJson(figureData);

            #region 线上咨询师问卷调查回答信息
            IDictionary<string, string[]> dic = new Dictionary<string, string[]>();

            var names = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == entity.MemberId).GroupBy(c => new { c.QuestionName }).Select(c => c.Key).ToArray();

            foreach (var name in names)
            {
                string[] values = _collocationQuestionnaireContract.Entities.Where(c => c.MemberId == entity.MemberId && c.QuestionName == name.QuestionName && !c.IsDeleted && c.IsEnabled).Select(c => c.Content).ToArray();

                KeyValuePair<string, string[]> item = new KeyValuePair<string, string[]>(name.QuestionName, values);

                dic.Add(item);
            }

            var question = JsonHelper.ToJson(dic);
            ViewBag.Question = question;
            #endregion
            return PartialView(entity);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> ListAsync(string number, int? storeId, int? categoryId, int? brandId, int? colorId, string realName, string mobilePhone, AppointmentState? state, DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var storeIds = _storeContract.QueryManageStoreId(AuthorityHelper.OperatorId.Value);

            var query = _appointmentContract.Entities.Where(p => storeIds.Contains(p.StoreId));
            if (storeId.HasValue)
            {
                query = query.Where(p => p.StoreId == storeId.Value);
            }

            if (!string.IsNullOrEmpty(number) && number.Length > 0)
            {

                query = query.Where(e => e.Number.StartsWith(number));

            }


            if (!string.IsNullOrEmpty(realName) && realName.Length > 0)
            {

                query = query.Where(p => p.Member.RealName.StartsWith(realName));

            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {

                query = query.Where(p => p.Member.MobilePhone.StartsWith(mobilePhone));
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            if (state.HasValue)
            {
                query = query.Where(p => p.State == state.Value);
            }
            var list = await query.OrderByDescending(a => a.UpdatedTime)
                                  .Skip((pageIndex - 1) * pageSize)
                                  .Take(pageSize)
                                  .Select(s => new
                                  {
                                      s.Id,
                                      s.Number,
                                      s.MemberId,
                                      s.Member.RealName,
                                      s.Member.UniquelyIdentifies,
                                      s.Member.UserPhoto,
                                      s.Member.MobilePhone,
                                      s.Store.StoreName,
                                      s.ProductNumber,
                                      s.CreatedTime,
                                      s.UpdatedTime,
                                      s.State,
                                      s.SelectedPlanId,
                                      s.DislikeProductNumbers,
                                      s.StartTime,
                                      s.EndTime,
                                      Quantity = s.AppointmentPacking.AppointmentPackingItem.Count,
                                      s.AppointmentType,
                                  }).ToListAsync();

            var data = list.Select(s => new
            {
                s.Id,
                s.SelectedPlanId,
                s.MemberId,
                RealName = s.AppointmentType == AppointmentType.自助 ? s.RealName : s.UniquelyIdentifies,
                s.StoreName,
                ProductNumber = s.ProductNumber ?? string.Empty,
                DislikeProductNumbers = s.DislikeProductNumbers ?? string.Empty,
                Number = s.Number ?? string.Empty,
                s.MobilePhone,
                s.UserPhoto,
                s.UpdatedTime,
                s.CreatedTime,
                StartTime = s.StartTime.HasValue ? s.StartTime.Value.ToString("MM-dd HH:mm") : string.Empty,
                EndTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("MM-dd HH:mm") : string.Empty,
                State = s.State.ToString(),
                s.Quantity,
                AppointmentTypeName = s.AppointmentType + ""
            }).ToList();

            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = data,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Edit(int Id)
        {
            var model = _appointmentContract.Entities.Include(i => i.CollocationPlans).FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == Id);
            ViewBag.Notes = model.Notes ?? string.Empty;
            ViewBag.selectedPlans = JsonHelper.ToJson(model.CollocationPlans.Select(p => p.Id).ToArray());
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult Edit(int id, string planIds)
        {
            var entity = _appointmentContract.Entities.Include(i => i.CollocationPlans).FirstOrDefault(i => !i.IsDeleted && i.IsEnabled && i.Id == id);
            if (entity == null)
            {
                return Json(OperationResult.Error("预约信息不存在"));
            }

            if (entity.State != AppointmentState.预约中)
            {
                switch (entity.State.Value)
                {
                    case AppointmentState.预约中:
                        break;
                    case AppointmentState.已处理:
                        return Json(OperationResult.Error("已完成的预约"));

                    case AppointmentState.已预约:
                        return Json(OperationResult.Error("已处理的预约"));
                    default:
                        return Json(OperationResult.Error("预约状态异常"));
                }
            }
            var arr = planIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).Distinct().ToList();


            var planEntities = _collocationPlanContract.Entities.Where(p => !p.IsDeleted && p.IsEnabled && arr.Contains(p.Id)).ToList();
            if (planEntities.Count <= 0 || planEntities.Count != arr.Count)
            {
                return Json(OperationResult.Error("搭配方案错误"));
            }
            entity.State = AppointmentState.已处理;
            entity.CollocationPlans.Clear();
            planEntities.ForEach(e => entity.CollocationPlans.Add(e));

            var res = _appointmentContract.Update(entity);
            return Json(res);
        }


        public ActionResult MemberDetail(int Id)
        {
            var memberEntity = _memberContract.View(Id);
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
            decimal quotiety = 0;


            var memberDeposit = _memberDepositContract.MemberDeposits
                   .Where(x => !x.IsDeleted && x.IsEnabled)
                   .Where(x => x.MemberId == Id && x.MemberActivityType == MemberActivityFlag.Recharge)
                   .OrderByDescending(x => x.Id)
                   .FirstOrDefault();
            if (memberDeposit != null)
            {
                quotiety = memberDeposit.Quotiety;
            }
            if (memberEntity.MemberFigures.Count > 0)
            {
                MemberFigure memberFigures = memberEntity.MemberFigures.OrderByDescending(x => x.Id).FirstOrDefault();
                string[] size = memberFigures.ApparelSize.Split(',');
                shangSize = size[0];
                xiaSize = size[1];
                preColor = memberFigures.PreferenceColor;
                figureType = memberFigures.FigureType;
                fgigureDes = memberFigures.FigureDes;
                height = memberFigures.Height.ToString();
                weight = memberFigures.Weight.ToString();
                shoulder = memberFigures.Shoulder.ToString();
                bust = memberFigures.Bust.ToString();
                waistline = memberFigures.Waistline.ToString();
                hips = memberFigures.Hips.ToString();
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

            return PartialView(memberEntity);
        }


        /// <summary>
        /// 查看单品
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isLike"></param>
        /// <returns></returns>
        public ActionResult ProductSelect(int id, bool isLike = true)
        {
            var query = _appointmentContract.Entities.Where(a => !a.IsDeleted && a.IsEnabled && a.Id == id);
            string numbers;
            if (isLike)
            {
                numbers = query.Select(a => a.ProductNumber).FirstOrDefault();
            }
            else
            {
                numbers = query.Select(a => a.DislikeProductNumbers).FirstOrDefault();
            }
            ViewBag.ProductNumbers = numbers ?? string.Empty;

            return PartialView();
        }

        /// <summary>
        /// 搭配方案列表
        /// </summary>
        /// <param name="selectedIds">选中状态的方案id</param>
        /// <param name="planIds"></param>
        /// <returns></returns>
        public ActionResult CollocationPlanSelect(string selectedIds, string planIds)
        {
            var arr = string.IsNullOrEmpty(selectedIds) ? new string[0] : selectedIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            ViewBag.selectedIds = JsonHelper.ToJson(arr);
            ViewBag.ids = planIds ?? string.Empty;
            return PartialView();
        }


        /// <summary>
        /// 获取预约时的答题项
        /// </summary>
        /// <returns></returns>
        public ActionResult GetOptions()
        {
            return Json(_appointmentContract.GetOptions(), JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 撤销预约
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]

        public ActionResult Cancel(int id)
        {
            var res = _appointmentContract.Cancel(id);
            return Json(res);
        }


        public ActionResult GetPackingId(int id)
        {
            var packingId = _appointmentContract.GetPackingId(id);
            return Json(new OperationResult(OperationResultType.Success, string.Empty, packingId), JsonRequestBehavior.AllowGet);

        }

        public class FinishEntry
        {
            public int Id { get; set; }
            public string Notes { get; set; }
        }

        #region 导出数据

        public ActionResult Export()
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));

            GridRequest request = new GridRequest(Request);
            Expression<Func<Appointment, bool>> predicate = FilterHelper.GetExpression<Appointment>(request.FilterGroup);
            var query = _appointmentContract.Entities.Where(predicate);

            var list = query.Select(s => new
            {
                s.Number,
                s.Store.StoreName,
                s.Member.RealName,
                s.Member.MobilePhone,
                State = s.State + "",
                s.StartTime,
                s.EndTime,
                s.UpdatedTime,
                ProductNumber = s.ProductNumber,
                DislikeProductNumbers = s.DislikeProductNumbers,
                s.SelectedPlanId,
            }).ToList().Select(s => new
            {
                s.Number,
                s.StoreName,
                s.RealName,
                s.MobilePhone,
                s.State,
                StartTime = s.StartTime.HasValue ? s.StartTime.Value.ToString("MM-dd HH:mm") : null,
                EndTime = s.EndTime.HasValue ? s.EndTime.Value.ToString("MM-dd HH:mm") : null,
                s.UpdatedTime,
                PlanCount = s.SelectedPlanId.HasValue ? 1 : 0,
                LikeCount = s.ProductNumber != null ? s.ProductNumber.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count() : 0,
                DislikeCount = s.DislikeProductNumbers != null ? s.DislikeProductNumbers.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Count() : 0,
            }).ToList();

            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return FileExcel(st, "预约管理");
        }

        #endregion

    }

}