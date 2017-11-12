




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
using Whiskey.ZeroStore.ERP.Models.DTO;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Commons.Controllers
{

    [License(CheckMode.Verify)]
    public class MemberImportController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(SettingController));

        protected readonly IMemberContract _memberContract;
        protected readonly IMemberImportRecordContract _memberImportRecordContract;

        public MemberImportController(IMemberContract memberContract,
            IMemberImportRecordContract memberImportRecordContract)
        {
            _memberContract = memberContract;
            _memberImportRecordContract = memberImportRecordContract;
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult MemberSelect(int recordId)
        {

            ViewBag.Id = recordId;
            return PartialView();
        }
        public ActionResult View(int id)
        {
            var entity = _memberImportRecordContract.View(id);
            return PartialView(entity);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public ActionResult MemberList(int recordId, string name, string mobilePhone, string memberIds, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memberContract.Members.Where(e => e.MemberImportRecordId.Value == recordId);
            query = query.Where(e => e.IsEnabled == isEnabled);
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

            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.MemberName,
                                e.RealName,
                                e.MobilePhone,
                                e.Store.StoreName,
                                e.Gender,
                                e.CreatedTime,
                                e.UpdatedTime,
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
        public ActionResult List(DateTime? startDate, DateTime? endDate, string name, string mobilePhone, bool isEnabled = true, int pageIndex = 1, int pageSize = 10)
        {
            var adminId = AuthorityHelper.OperatorId;
            var query = _memberImportRecordContract.Entities;
            query = query.Where(e => e.IsEnabled == isEnabled);
            if (startDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(e => e.CreatedTime <= endDate.Value);
            }
            if (!string.IsNullOrEmpty(name) && name.Length > 0)
            {
                query = query.Where(e => e.Members.Any(m => m.MemberName.StartsWith(name) || m.RealName.StartsWith(name)));
            }
            if (!string.IsNullOrEmpty(mobilePhone) && mobilePhone.Length > 0)
            {
                query = query.Where(e => e.Members.Any(m => m.MobilePhone.StartsWith(mobilePhone)));
            }


            var list = query.OrderByDescending(e => e.UpdatedTime)
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(e => new
                            {
                                e.Id,
                                e.IsDeleted,
                                e.IsEnabled,
                                e.TotalCount,
                                e.SuccessCount,
                                e.Store.StoreName,
                                e.CreateStartDate,
                                e.CreateEndtDate,
                                IsChecked = false,
                                e.CreatedTime,
                                e.UpdatedTime
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





    }
}
