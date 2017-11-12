
using System;
using System.Linq;
using System.Web.Mvc;
using Whiskey.Utility.Class;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Data;
using System.Data.Entity;
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.Web.Helper;
using System.Collections.Generic;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Notices;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Offices.Controllers
{
    [License(CheckMode.Verify)]
    public class TimeoutRequestController : BaseController
    {
        private static readonly ILogger _Logger = LogManager.GetLogger(typeof(TimeoutRequestController));

        private readonly ITimeoutRequestContract _contract;
        private readonly ITimeoutSettingContract _timeoutSettingContract;
        private readonly IAdministratorContract _adminContract;
        private readonly IStoreContract _storeContract;
        private readonly IModuleContract _moduleContract;
        private readonly IPermissionContract _permisstionContract;
        private readonly INotificationContract _notificationContract;
        private string[] _filterFlags = new string[]
        {
            "#pass","#nopass"
        };
        private const string _superVerifyFlags = ".superverify";
        private const string _configKey = "limitdays";

        public TimeoutRequestController(ITimeoutRequestContract contract, ITimeoutSettingContract timeoutSettingContract,
            IAdministratorContract adminContract,
            IStoreContract storeContract,
            IModuleContract moduleContract,
            IPermissionContract permisstionContract,
            INotificationContract notificationContract)
        {
            _contract = contract;
            _timeoutSettingContract = timeoutSettingContract;
            _adminContract = adminContract;
            _storeContract = storeContract;
            _moduleContract = moduleContract;
            _permisstionContract = permisstionContract;
            _notificationContract = notificationContract;
        }

        [Layout]
        public ActionResult Index()
        {
            var timeoutList = _timeoutSettingContract.Settings.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
            ViewBag.TimeoutList = timeoutList;
            var flags = PageFlag();
            ViewBag.PageFlags = JsonHelper.ToJson(flags);
            return View();
        }

        [HttpGet]
        public ActionResult EditConfig()
        {

            return PartialView();
        }
        [HttpPost]
        public ActionResult EditConfig(int limitdays)
        {
            var config = _contract.GetConfig();
            var dict = config.Data as Dictionary<string, string>;
            dict[_configKey] = limitdays.ToString();
            var res = _contract.UpdateConfig(dict);
            return Json(res);
        }
        [HttpGet]
        public ActionResult GetConfig()
        {
            var res = _contract.GetConfig();

            return Json(res, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var timeoutList = _timeoutSettingContract.Settings.Where(s => !s.IsDeleted && s.IsEnabled).Select(s => new SelectListItem { Text = s.Name, Value = s.Id.ToString() }).ToList();
            ViewBag.TimeoutList = timeoutList;
            var flag = PageFlag();
            return PartialView();
        }


        public ActionResult Verify(int requestId, bool isPass)
        {
            var request = _contract.View(requestId);
            if (request.State != TimeoutRequestState.审核中)
            {
                return Json(OperationResult.Error("已审核"));
            }
            var config = _contract.GetConfig().Data as Dictionary<string, string>;
            var configDays = TimeSpan.FromDays(int.Parse(config[_configKey]));
            var timeoutDays = TimeSpan.FromSeconds(request.Timeouts);
            if (timeoutDays > configDays)
            {
                var adminIds = GetVerifyAdminIds(request.DepartmentId, _superVerifyFlags);
                if (!adminIds.Contains(AuthorityHelper.OperatorId.Value))
                {
                    return Json(OperationResult.Error($"超时天数已超过规定最高天数{config[_configKey]},需要更高权限才能继续操作"));
                }
            }
            else
            {
                var adminIds = GetVerifyAdminIds(request.DepartmentId, _filterFlags);
                if (!adminIds.Contains(AuthorityHelper.OperatorId.Value))
                {
                    return Json(OperationResult.Error($"权限不足"));
                }
            }
            request.State = isPass ? TimeoutRequestState.已通过 : TimeoutRequestState.未通过;
            request.VerifyAdminId = AuthorityHelper.OperatorId.Value;
            var res = _contract.Update(request);
            if (res.ResultType == OperationResultType.Success)
            {
                // 获取有审核权限且管辖部门包含申请人所在部门的管理员
                var notiDto = new NotificationDto
                {
                    AdministratorIds = new List<int> { request.RequestAdminId },
                    NoticeType = NoticeFlag.Immediate,
                    NoticeTargetType = (int)NoticeTargetFlag.Admin,
                    Title = "超时申请审核通知",
                    Description = $"您的申请已经审核完毕,审核结果:{request.State}",
                    IsEnableApp = false
                };
                _notificationContract.Insert(sendNotificationAction, notiDto);
            }
            return Json(res);
        }

        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Create([Bind(Include = "TimeoutSettingId,Number,Notes")]TimeoutRequest dto)
        {
            try
            { 
                var res = _contract.Create(dto, sendNotificationAction);
                return Json(res);
            }
            catch (Exception e)
            {

                return Json(OperationResult.Error(e.Message));
            }


        }

        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        public ActionResult View(int Id)
        {
            var result = _contract.Entities.Where(e => e.Id == Id).FirstOrDefault();
            return PartialView(result);
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(string name, string number, int? timeoutSettingId, DateTime? startDate, DateTime? endDate, bool? isUsed, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId.Value;
            var query = _contract.Entities.Where(e => e.IsEnabled == isEnabled);
            // 权限--可管理部门
            var departmentIds = _storeContract.QueryManageDepartmentId(adminId, false);
            query = query.Where(e => e.RequestAdminId == adminId || departmentIds.Contains(e.DepartmentId));
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(e => e.RequestAdmin.Member.RealName.StartsWith(name));
            }

            if (!string.IsNullOrEmpty(number) && number.Length > 0)
            {

                query = query.Where(e => e.Number.StartsWith(number));

            }
            if (timeoutSettingId.HasValue)
            {

                query = query.Where(e => e.TimeoutSettingId == timeoutSettingId.Value);
            }

            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            if (isUsed.HasValue)
            {
                query = query.Where(e => e.IsUsed == isUsed.Value);

            }
            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.CreatedTime,
                                e.UpdatedTime,
                                e.Department.DepartmentName,
                                TimeoutType = e.TimeoutType.Name,
                                e.Number,
                                e.Timeouts,
                                RequestName = e.RequestAdmin.Member.RealName,
                                VerifyName = e.VerifyAdmin.Member.RealName,
                                State = e.State.ToString(),
                                e.IsUsed
                            }).ToList();


            var res = new OperationResult(OperationResultType.Success, string.Empty, new
            {
                pageData = list,
                pageInfo = new PageDto
                {
                    pageIndex = pageIndex,
                    pageSize = pageSize,
                    totalCount = query.Count(),
                }
            });

            return Json(res, JsonRequestBehavior.AllowGet);
        }



        private List<string> PageFlag()
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);

            try
            {
                var listpers = PermissionHelper.GetCurrentUserPageNoPermission(pageUrl, _adminContract, _moduleContract, _permisstionContract)
                    .ToList();

                return listpers.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                    .Select(p => p.OnlyFlag)
                    .ToList();
            }
            catch (Exception ex)
            {
                _Logger.Error("权限包含的页面元素加载出错，错误如下：" + ex.Message + "。");
                throw new Exception("error");
            }

        }

        private List<int> GetVerifyAdminIds(int departmentId, params string[] onlyFlags)
        {
            var controller = RouteData.Values["controller"].ToString();

            var module = CacheAccess.GetModules(_moduleContract)
                .Where(c => !c.IsDeleted && c.IsEnabled)
                .Where(c => c.PageController != null && c.PageAction != null)
                .Where(c => c.PageController == controller)
                .FirstOrDefault();

            var permissionIds = CacheAccess.GetPermissions(_permisstionContract)
                .Where(p => !p.IsDeleted && p.IsEnabled && p.ModuleId == module.Id)
                .Where(p => onlyFlags.Contains(p.OnlyFlag))
                .Select(p => p.Id).ToList();


            var adminIds = _adminContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled)
              .Where(a => a.JobPosition.Departments.Any(d => d.Id == departmentId))
              .Where(a => a.Roles.Any(r => r.ARolePermissionRelations.Any(p => permissionIds.Contains(p.PermissionsId.Value))))
              .Select(a => a.Id)
              .ToList();
            return adminIds;
        }

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _contract.DeleteOrRecovery(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _contract.EnableOrDisable(true, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>

        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _contract.EnableOrDisable(false, Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        public ActionResult UnVerifyCount()
        {
            var count = _contract.Entities.Count(r => !r.IsDeleted && r.IsEnabled && r.State == TimeoutRequestState.审核中);
            return Json(new OperationResult(OperationResultType.Success, string.Empty, count));
        }


    }
}
