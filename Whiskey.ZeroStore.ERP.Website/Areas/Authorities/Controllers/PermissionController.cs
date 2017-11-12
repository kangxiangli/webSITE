




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
using Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Models;
using Whiskey.ZeroStore.ERP.Services.Content;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{

    public class PermissionController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(PermissionController));

        protected readonly IPermissionContract _permissionContract;
        protected readonly IAdministratorContract _administratorContract;
        protected readonly IModuleContract _moduleContract;

        public PermissionController(IPermissionContract permissionContract, IAdministratorContract administratorContract, IModuleContract moduleContract)
        {
            _permissionContract = permissionContract;
            _administratorContract = administratorContract;
            _moduleContract = moduleContract;
        }


        /// <summary>
        /// 视图数据
        /// </summary>
        /// <returns></returns>
        [Layout]
        [AuthValid]
        public ActionResult Index()
        {
            
            var mod= CacheAccess.GetParentModules(_moduleContract, true);
            ViewBag.ModuleF = mod;
            return View();
        }


        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var mod = CacheAccess.GetParentModules(_moduleContract, false);
            ViewBag.ModuleF=mod;
            int defauModulId=int.Parse(mod[0].Value);
            ViewBag.ModuleT = CacheAccess.GetModules(_moduleContract).Where(c => c.ParentId == defauModulId && c.IsEnabled == true && c.IsDeleted == false).Select(c => new SelectListItem()
            {
                Value = c.Id.ToString(),
                Text = c.ModuleName
            }).ToList();
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
        public ActionResult Create(PermissionDto dto)
        {
            var result = _permissionContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(PermissionDto dto)
        {
            var result = _permissionContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var result = _permissionContract.Edit(Id);
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
            var result = _permissionContract.View(Id);
            ViewBag.moduName = _permissionContract.Permissions.Where(c => c.Id == Id).FirstOrDefault().Module.ModuleName;
            
            return PartialView(result);
        }


        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Permission, bool>> predicate = FilterHelper.GetExpression<Permission>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                var count = 0;
                var list = _permissionContract.Permissions.Where<Permission, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    //m.ModuleId,
                    m.PermissionName,
                    m.Description,
                    m.Identifier,
                    m.Icon,
                    m.Style,
                    m.onClickScripts,
                    m.IsDisplayIcon,
                    m.IsDisplayText,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
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
            var result = _permissionContract.Remove(Id);
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
            var result = _permissionContract.Delete(Id);
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
            var result = _permissionContract.Recovery(Id);
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
            var result = _permissionContract.Enable(Id);
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
            var result = _permissionContract.Disable(Id);
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
            var list = _permissionContract.Permissions.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _permissionContract.Permissions.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
       
        public ActionResult PermissCell()
        {
            return PartialView();
        }
        public ActionResult GetTree()
        {
            var model = new List<Module>();

            var administrator = _administratorContract.Administrators.FirstOrDefault(m => m.Id == AuthorityHelper.OperatorId);
            //var permissions = administrator.Roles.SelectMany(m => m.Permissions).Select(c => c.ModuleId).ToList();
            var permissions = administrator.Roles.SelectMany(m => m.ARolePermissionRelations.Select(s => s.Permission)).Select(c => c.ModuleId).ToList();
            List<int> listModuleId = permissions.Distinct().ToList();

            //var rigtree = _moduleContract.Modules.Where(c => listModuleId.Contains(c.Id) && c.ParentId == null && !c.IsDeleted && c.IsEnabled).ToList();
            var li = GetNode(listModuleId);


            ResJson json = new ResJson()
            {
                msg = "测试",
                obj = li,
                success = true,
                type = "json"
            };
            return Json(json);
        }
        public ActionResult GetChilById()
        {
            //GetChilById?_pid
            OperationResult resul = new OperationResult(OperationResultType.Error);
            var parid = InputHelper.SafeInput(Request["_pid"]);
            if (!string.IsNullOrEmpty(parid))
            {
                int pid = Convert.ToInt32(parid);
             
                var pmodu = _moduleContract.Modules.Where(c => c.Id == pid && c.IsDeleted == false && c.IsEnabled == true).FirstOrDefault();
                if (pmodu != null)
                {
                  var chil= pmodu.Permissions.Select(c => new RightTree()
                     {
                         id = "c"+c.Id,
                         text = c.PermissionName,
                         _checked=false,
                          url="",
                          msg=c.Description
                     });
                    return Json(new ResJson()
                    {
                        obj = chil,
                        msg = "",
                        success = true,

                    });
                }
            }
            return Json(resul);
        }
        private RightTree GetNode(List<int> moduleIds)
        {
            RightTree retree = new RightTree()
            {
                id = "0",
                _checked = false,
                text = "选择权限",
                url = "",
                children = new List<RightTree>()
            };
            var parmodules = _moduleContract.Modules.Where(c => c.ParentId == null && c.IsEnabled == true && c.IsDeleted == false);
            if (parmodules.IsNotNullOrEmptyThis())
            {
                foreach (var item in parmodules)
                {
                    retree.children.Add(new RightTree()
                    {
                        id = item.Id + "",
                        text = item.ModuleName,
                        _checked = false,
                        _isShow = false,
                        url = "",
                        children = GetChild(item.Id, moduleIds),
                    });
                }
            }
            return retree;
        }

        private List<RightTree> GetChild(int parid, List<int> moduleIds)
        {
            List<RightTree> listTree = new List<RightTree>();
            var currentChild = _moduleContract.Modules.Where(c => c.ParentId == parid && moduleIds.Contains(c.Id) && c.IsDeleted == false && c.IsEnabled == true).ToList();
            if (currentChild.IsNotNullOrEmptyThis())
            {
                foreach (var item in currentChild)
                {
                    listTree.Add(new RightTree()
                    {
                        id = item.Id + "",
                        text = item.ModuleName,
                        url = "",
                        _checked = false,
                        _isShow = false,
                        children = GetPermiss(item.Id)
                    });
                }
            }
            return listTree;
        }

        private List<RightTree> GetPermiss(int twoModId)
        {
            var pers = CacheAccess.GetPermissions(_permissionContract).Where(c => c.ModuleId == twoModId && c.IsEnabled == true && c.IsDeleted == false).ToList();
            return pers.Select(c => new RightTree()
            {
                id = "c" + c.Id,
                text = c.PermissionName,
                url = "",
                msg = c.Description,
                _checked = false,
                _isShow = false,
                _gtype = (int?)c.Gtype
            }).ToList();
        }

        //yxk 2015-11-25
        /// <summary>
        /// 获取当前节点的所有子节点，包含当前节点
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="listModuleId"></param>
        /// <returns></returns>
        private RightTree GetChildTreeNodes(RightTree curNode, List<int> listModuleId)
        {
            int moduId = Convert.ToInt32(curNode.id);
            List<RightTree> tre = _moduleContract.Modules.Where(c => c.ParentId == moduId && listModuleId.Contains(c.Id)).Select(c => new RightTree()
            {
                id = "" + c.Id,
                text = c.ModuleName,
                url = "",
                _checked = false
            }).ToList();
            if (tre != null)
            {
                curNode.children = tre;
            }

            foreach (var chTre in tre)
            {
                GetChildTreeNodes(chTre, listModuleId);
            }
            return curNode;
        }
        //yxk 2015-11-25
        /// <summary>
        /// 获取当前节点的所有父节点，包含当前节点
        /// </summary>
        /// <param name="curNode"></param>
        /// <param name="listModuleId"></param>
        /// <returns></returns>
        private RightTree GetParentsTreeNodes(RightTree curNode, List<int> listModuleId)
        {
            RightTree partreeNode = null;
            int moduId = Convert.ToInt32(curNode.id);
            var curmodu = _moduleContract.Modules.Where(c => c.Id == moduId && c.IsDeleted == false && c.IsEnabled == true && listModuleId.Contains(c.Id)).FirstOrDefault();
            if (curmodu == null)
            {
                return null;
            }
            else if (curmodu.ParentId != null)
            {
                partreeNode = _moduleContract.Modules.Where(c => c.Id == curmodu.ParentId && c.IsDeleted == false && c.IsEnabled == true && listModuleId.Contains(c.Id)).Select(c => new RightTree()
               {
                   id = "" + c.Id,
                   text = c.ModuleName,
                   url = "",
                   _checked = false
               }).FirstOrDefault();
                if (partreeNode.children == null)
                {
                    partreeNode.children = new List<RightTree>();

                }
                partreeNode.children.Add(curNode);
                GetParentsTreeNodes(partreeNode, listModuleId);
                return partreeNode;
            }
            else
            {
                return partreeNode;
            }
        }

    }
}

