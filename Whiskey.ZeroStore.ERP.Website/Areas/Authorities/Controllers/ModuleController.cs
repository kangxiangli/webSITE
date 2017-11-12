using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{

    [License(CheckMode.Verify)]
    public class ModuleController : BaseController
    {
        #region 声明业务层操作对象
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ModuleController));

        protected readonly IModuleContract _moduleContract;

        protected readonly IPermissionContract _permissionContract;

        protected readonly IAdministratorContract _adminContract;

        public ModuleController(IModuleContract moduleContract,
            IPermissionContract permissionContract,
            IAdministratorContract adminContract)
        {
            _permissionContract = permissionContract;
            _moduleContract = moduleContract;
            _adminContract = adminContract;
        }
        #endregion

        #region 初始化界面
        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            string title = "请选择";
            ViewBag.ListModule = (_moduleContract.SelectList(title).Select(x => new SelectListItem { Text = x.Value, Value = x.Key })).ToList();
            return View();
        }
        #endregion

        #region 添加模版
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            string title = "请选择";
            ViewBag.ListModule = (_moduleContract.SelectList(title).Select(x => new SelectListItem { Text = x.Value, Value = x.Key })).ToList();
            ViewBag.ListPermission = (_permissionContract.SelectList("").Select(x => new SelectListItem { Text = x.Value, Value = x.Key })).ToList();
            return PartialView();
        }
        /// <summary>
        /// 创建数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(ModuleDto dto)
        {
            if (dto.ParentId == 0)
            {
                dto.ParentId = null;
                dto.ModuleType = 0;
            }
            else dto.ModuleType = 1;
            var result = _moduleContract.Insert(dto);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(ModuleDto dto)
        {
            if (dto.ParentId == 0)
            {
                dto.ModuleType = 0;
                dto.ParentId = null;
            }

            else dto.ModuleType = 1;

            var result = _moduleContract.Update(dto);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _moduleContract.Edit(Id);
            var li = CacheAccess.GetModules(_moduleContract).Where(c => c.IsDeleted == false && c.IsEnabled == true && c.ParentId == null).Select(c => new SelectListItem()
            {
                Text = c.ModuleName,
                Value = c.Id.ToString(),
                Selected = Id == c.Id
            }).ToList();
            li.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = "0"
            });
            ViewBag.parentModu = li;
            return PartialView(result);
        }


        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult View(int Id)
        {
            var result = _moduleContract.View(Id);
            
            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Module, bool>> predicate = FilterHelper.GetExpression<Module>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                List<object> objli = new List<object>();
                var count = 0;

                #region
                var whereli = request.FilterGroup.Rules.Where(x => x.Field == "IsDeleted" || x.Field == "IsEnabled")
                          .Select(x => new
                          {
                              x.Field,
                              x.Value
                          }).ToList();
                bool isDele = false;
                bool isEnable = true;
                foreach (var item in whereli)
                {

                    if (item.Field == "IsDeleted")
                        isDele = item.Value.ToString() == "true" ? true : false;

                    if (item.Field == "IsEnabled")
                        isEnable = item.Value.ToString() == "true" ? true : false;
                }
                #endregion

                //如果没有提供模块父id和模块名，则根据父级模块分页
                var whereearr = request.FilterGroup.Rules.Select(x => x.Field).ToList();
                List<Module> list = new List<Module>();
                if (whereearr.IndexOf("ParentId") == -1 && whereearr.IndexOf("ModuleName") == -1)
                {
                    var parli = CacheAccess.GetModules(_moduleContract)
                        .Where(predicate.Compile())
                        .Where(c => c.ParentId == null);

                    list = parli.OrderBy(x => x.Sequence).Skip(request.PageCondition.PageIndex).Take(request.PageCondition.PageSize).ToList();
                    List<Administrator> listAdmin = _adminContract.Administrators.ToList();
                    count = parli.Count();
                    foreach (var x in list)
                    {
                        var parent = new
                        {
                            x.Id,
                            ParentId = "",
                            x.ModuleName,
                            x.Icon,
                            x.ModuleType,
                            x.Description,
                            x.PageUrl,
                            x.PageArea,
                            x.PageController,
                            x.IsDeleted,
                            x.IsEnabled,
                            x.Sequence,
                            x.UpdatedTime,
                            x.CreatedTime,
                            //AdminName = x.Operator == null ? "" : x.Operator.AdminName
                            AdminName = listAdmin.FirstOrDefault(k => k.Id == x.OperatorId) == null ? string.Empty : listAdmin.FirstOrDefault(k => k.Id == x.OperatorId).Member.MemberName,
                        };
                        objli.Add(parent);
                        var childs =
                            CacheAccess.GetModules(_moduleContract)
                                .Where(c => c.IsDeleted == isDele && c.IsEnabled == isEnable && c.ParentId == x.Id).OrderBy(c=>c.Sequence)
                                .Select(t => new
                                {
                                    t.Id,
                                    ParentId = x.Id,
                                    t.ModuleName,
                                    t.Icon,
                                    t.ModuleType,
                                    t.Description,
                                    t.PageUrl,
                                    t.PageArea,
                                    t.PageController,
                                    t.IsDeleted,
                                    t.IsEnabled,
                                    t.Sequence,
                                    t.UpdatedTime,
                                    t.CreatedTime,
                                    //AdminName = t.Operator == null ? "" : t.Operator.AdminName
                                    AdminName = listAdmin.FirstOrDefault(k => k.Id == x.OperatorId) == null ? string.Empty : listAdmin.FirstOrDefault(k => k.Id == x.OperatorId).Member.MemberName,
                                }).ToList();
                        objli.AddRange(childs);
                    }

                }
                 //否则查找所有模块
                else
                {
                    var objlist = _moduleContract.Modules.Where<Module, int>(predicate, request.PageCondition, out count).Select(m => new
                    {

                        m.ParentId,
                        m.ModuleName,
                        m.Icon,
                        m.ModuleType,
                        m.Description,
                        m.PageUrl,
                        m.PageArea,
                        m.PageController,
                        m.PageAction,
                        m.onClickScripts,
                        m.Id,
                        m.IsDeleted,
                        m.IsEnabled,
                        m.Sequence,
                        m.UpdatedTime,
                        m.CreatedTime,
                        m.Operator.Member.MemberName,
                    }).ToList();

                    var li = objlist.Where(c => c.ParentId != null).GroupBy(x => x.ParentId);
                    foreach (var _item in li)
                    {

                        var paren = CacheAccess.GetModules(_moduleContract).Where(c => c.Id == _item.Key && c.IsDeleted == isDele && c.IsEnabled == isEnable).Select(x => new
                        {

                            x.Id,
                            ParentId = "",
                            x.ModuleName,
                            x.Icon,
                            x.ModuleType,
                            x.Description,
                            x.PageUrl,
                            x.PageArea,
                            x.PageController,
                            x.IsDeleted,
                            x.IsEnabled,
                            x.Sequence,
                            x.UpdatedTime,
                            x.CreatedTime,
                            AdminName = x.Operator == null ? "" : x.Operator.Member.MemberName

                        }).FirstOrDefault();

                        if (paren != null)
                        {
                            objli.Add(paren);
                            var childs = _item.Select(x => new
                            {
                                x.Id,
                                ParentId = _item.Key,
                                x.ModuleName,
                                x.Icon,
                                x.ModuleType,
                                x.Description,
                                x.PageUrl,
                                x.PageArea,
                                x.PageController,
                                x.IsDeleted,
                                x.IsEnabled,
                                x.Sequence,
                                x.UpdatedTime,
                                x.CreatedTime,
                                AdminName = ""
                            }).ToList();
                            objli.AddRange(childs);

                        }
                    }
                }
                return new GridData<object>(objli, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Remove(int[] Id)
        {
            var result = _moduleContract.Remove(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Delete(int[] Id)
        {
            var result = _moduleContract.Delete(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Recovery(int[] Id)
        {
            var result = _moduleContract.Recovery(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Enable(int[] Id)
        {
            var result = _moduleContract.Enable(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        public ActionResult Disable(int[] Id)
        {
            var result = _moduleContract.Disable(Id);
            if (result.ResultType == OperationResultType.Success)
            {
                HttpRuntime.Cache.Remove("moduleUrl");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _moduleContract.Modules.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Printer");
            st.SetAttribute("list", list);
            return Json(new { html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 导出数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [Log]
        public ActionResult Export(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _moduleContract.Modules.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult GetModules(int id)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            List<SelectListItem> li = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == id && c.IsDeleted == false && c.IsEnabled == true).Select(c => new SelectListItem()
            {
                Text = c.ModuleName,
                Value = c.Id.ToString()
            }).ToList();
            li.Insert(0, new SelectListItem()
            {
                Text = "下拉选择",
                Value = ""
            });
            resul = new OperationResult(OperationResultType.Success) { Data = li };
            return Json(resul);

        }



        #region 排序设置
        public JsonResult SetSeq(int Id, int SequenceType)
        {
            OperationResult oper = _moduleContract.SetSeq(Id, SequenceType);
            return Json(oper);
        }
        #endregion  


    }
}
