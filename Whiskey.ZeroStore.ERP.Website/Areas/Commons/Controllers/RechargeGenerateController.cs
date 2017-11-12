




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
using Whiskey.ZeroStore.ERP.Website.Areas.Offices.Models;
using Whiskey.ZeroStore.ERP.Models.Enums;
using System.Threading;
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{

    [License(CheckMode.Verify)]
    public class RechargeGenerateController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SettingController));

        protected readonly IMemberContract _memberContract;
        protected readonly IRechargeGenerateRecordContract _memeberRechargeGenerateRecordContract;
        protected readonly IMemberActivityContract _memberActivityContract;
        protected readonly IMemberDepositContract _memberDepositContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IModuleContract _moduleContract;
        protected readonly IPermissionContract _permissionContract;

        public RechargeGenerateController(IMemberContract memberContract,
            IRechargeGenerateRecordContract memeberRechargeGenerateRecordContract,
            IMemberActivityContract memberActivityContract,
            IMemberDepositContract memberDepositContract,
            IRechargeGenerateRecordContract rechargeGenerateRecordContract,
            IAdministratorContract administratorContract,
            IModuleContract moduleContract,
            IPermissionContract permissionContract
            )
        {
            _memberContract = memberContract;
            _memeberRechargeGenerateRecordContract = memeberRechargeGenerateRecordContract;
            _memberActivityContract = memberActivityContract;
            _memberDepositContract = memberDepositContract;
            _administratorContract = administratorContract;
            _moduleContract = moduleContract;
            _permissionContract = permissionContract;

        }
        private List<string> PageFlag()
        {
            var area = RouteData.DataTokens.ContainsKey("area") ? RouteData.DataTokens["area"].ToString() : string.Empty;
            var controller = RouteData.Values["controller"].ToString();

            var pageUrl = string.Format("{0}/{1}/Index", area, controller);

            try
            {
                var listpers = PermissionHelper.GetCurrentUserPageNoPermission(pageUrl, _administratorContract, _moduleContract, _permissionContract)
                    .ToList();

                return listpers.Where(p => !string.IsNullOrEmpty(p.OnlyFlag))
                    .Select(p => p.OnlyFlag)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("error");
            }

        }

        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            ViewBag.PageFlags = JsonHelper.ToJson(PageFlag());
            return View();
        }

        public ActionResult Generate()
        {
            return PartialView();
        }

        public ActionResult MemberSelect(int? recordId, string memberIds, int showOptBtn = 0)
        {

            ViewBag.Id = recordId;
            ViewBag.ShowOptBtn = showOptBtn;
            if (string.IsNullOrEmpty(memberIds))
            {
                ViewBag.MemberIds = JsonHelper.ToJson(new int[0]);
            }
            else
            {
                var arr = memberIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => int.Parse(x)).ToArray();
                ViewBag.MemberIds = JsonHelper.ToJson(arr);
            }
            return PartialView();
        }
        public ActionResult View(int id)
        {
            ViewBag.Id = id;
            return PartialView();
        }
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult DepositList(int recordId, int? storeId, string name, string mobilePhone, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;

            var query = _memeberRechargeGenerateRecordContract.Entities.Where(e => e.IsEnabled == isEnabled)
                .Where(e => e.Id == recordId)
               .SelectMany(e => e.MemberDeposits);

            if (storeId.HasValue)
            {
                query = query.Where(e => e.DepositStoreId.Value == storeId.Value);
            }
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                query = query.Where(e => e.Member.MemberName.StartsWith(name) || e.Member.RealName.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.Member.MobilePhone.StartsWith(mobilePhone));
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
                                e.Member.Gender,
                                e.Member.MemberName,
                                e.Member.RealName,
                                e.Member.MobilePhone,
                                e.Store.StoreName,
                                e.BeforeBalance,
                                e.AfterBalance,
                                e.MemberActivity.ActivityName,
                                MemberCreatedTime = e.Member.CreatedTime,
                                e.Price,
                                e.Coupon,
                                IsChecked = false
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


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberList(DateTime? startDate, DateTime? endDate, int? recordId, bool? hasBalance, int? storeId, string name, string mobilePhone, string memberIds, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memberContract.Members;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (recordId.HasValue)
            {
                query = query.Where(e => e.MemberImportRecordId.Value == recordId);
            }
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            if (hasBalance.HasValue)
            {
                if (hasBalance.Value)
                {
                    query = query.Where(e => e.Balance > 0);
                }
                else
                {
                    query = query.Where(e => e.Balance <= 0);
                }
            }
            if (storeId.HasValue)
            {
                query = query.Where(e => e.StoreId.Value == storeId.Value);
            }
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                query = query.Where(e => e.MemberName.StartsWith(name) || e.RealName.StartsWith(name));
            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.MobilePhone.StartsWith(mobilePhone));
            }
            if (!string.IsNullOrEmpty(memberIds) && memberIds.Length > 0)
            {
                var arr = memberIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(i => int.Parse(i)).ToList();
                query = query.Where(e => arr.Contains(e.Id));
            }

            var list = query.OrderByDescending(e => e.CreatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.CreatedTime,
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.MemberName,
                                e.RealName,
                                e.MobilePhone,
                                e.Store.StoreName,
                                e.Gender,
                                e.Balance,
                                IsChecked = false
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

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult List(DateTime? startDate, DateTime? endDate, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memeberRechargeGenerateRecordContract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }

            var list = query.OrderByDescending(e => e.CreatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.RechargeCount,
                                IsChecked = false,
                                e.CreatedTime,
                                e.UpdatedTime,
                                e.MemberActivity.ActivityName
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

        public ActionResult ShowRechargeActivity()
        {
            var data = _memberActivityContract.MemberActivitys.Where(a => !a.IsDeleted && a.IsEnabled)
                .Where(a => a.ActivityType == MemberActivityFlag.Recharge)
                .Where(a => a.IsForever || (a.StartDate <= DateTime.Now && a.EndDate >= DateTime.Now))
                .Select(a => new { a.ActivityName, a.Id }).ToList();
            return Json(new OperationResult(OperationResultType.Success, string.Empty, data), JsonRequestBehavior.AllowGet);
        }


        public ActionResult Save(DateTime start, DateTime end, int rechargeActivityId, params int[] memberIds)
        {
            var res = _memberDepositContract.GenerateRechargeData(start, end, rechargeActivityId, memberIds);
            return Json(res);
        }

        public ActionResult Fix(int id)
        {
            var deposits = _memeberRechargeGenerateRecordContract.Entities.Where(e => e.Id == id).SelectMany(e => e.MemberDeposits).ToList();
            if (!deposits.Any())
            {
                return Json(OperationResult.Error("未找到储值数据"));
            }
            var rand = new Random();
            var flag = 0;
            var tmp = 0M;
            foreach (var item in deposits)
            {
                flag = rand.Next(0, 2);
                if (flag == 1)
                {
                    tmp = item.Cash;
                    item.Cash = item.Card;
                    item.Card = tmp;
                }
                item.DepositContext = MemberDepositContextEnum.线下充值;
            }
            var res =_memberDepositContract.Update(deposits);

            return Json(res);
        }




    }
}
