using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Authorities.Controllers
{
    public class PermissionConfigController : BaseController
    {
        //
        // GET: /Authorities/PermissionConfig/
        //与权限相关的配置信息
        protected readonly IModuleContract _moduleContract;
        public PermissionConfigController(IModuleContract moduleContract)
        {
            _moduleContract = moduleContract;
        }
        [Layout]
        public ActionResult Index()
        {
            ViewBag.modules = CacheAccess.GetModules(_moduleContract,null, true);
           
            return View();
        }

        public ActionResult List()
        {
            GridRequest rq = new GridRequest(Request);
            var pred = FilterHelper.GetExpression<Module>(rq.FilterGroup);
            int cou = _moduleContract.Modules.Where(pred).Count();
            var t= _moduleContract.Modules.Where(pred)
                .OrderBy(c => c.CreatedTime)
                .Skip(rq.PageCondition.PageIndex)
                .Take(rq.PageCondition.PageSize)
                .Select(c=>new
                {
                    ParentId="",
                    c.Id,
                    c.ModuleName,
                    CreatedTime= c.CreatedTime,
                    c.IsCompleteRule
                }).ToList().Select(c=>new
            {
                 ParentId=c.ParentId,
                 Id=c.Id,
                 ModuleName = c.ModuleName,
                 CreatedTime=c.CreatedTime.ToString("yyyy-MM-dd"),
                 IsCompleteRule = c.IsCompleteRule
            });
            GridData<object> da=new GridData<object>(t,cou,Request);
            return Json(da);
        }

        public ActionResult CompleteRule(int Id)
        {
           
              var resul=  _moduleContract.CompleteRule(Id);
            return Json(resul);

        }

        public ActionResult NoCompleteRule(int Id)
        {
            var resul = _moduleContract.NoCompleteRule(Id);
            return Json(resul);

        }
        //public ActionResult Create()
        //{
            
        //}
    }
}