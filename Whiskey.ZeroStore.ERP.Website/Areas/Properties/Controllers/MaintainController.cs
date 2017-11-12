using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Antlr3.ST.Language;
using AutoMapper;
using Microsoft.Ajax.Utilities;
using Microsoft.Owin.Security.DataHandler.Serializer;
using WebGrease.Css.Ast;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models.Entities.Properties;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{
    public class MaintainController : BaseController
    {
        //
        // GET: /Properties/Maintain/

        private readonly IMaintainContract _maintainContract;

        public MaintainController(IMaintainContract maintainContract)
        {
            _maintainContract = maintainContract;
        }
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult List()
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            GridRequest req = new GridRequest(Request);
            Expression<Func<MaintainExtend, bool>> exp = FilterHelper.GetExpression<MaintainExtend>(req.FilterGroup);
            var allli = _maintainContract.Maintains.Where(exp);
            int coun = allli.Count();
            var li =
                allli.OrderByDescending(c => c.UpdatedTime)
                    .Skip(req.PageCondition.PageIndex)
                    .Take(req.PageCondition.PageSize);
            var grp = li.Where(c => c.ParentId != null).GroupBy(c => c.ParentId);
            List<Object> datli = new List<object>();
            List<int> parId = new List<int>();
            foreach (var g in grp)
            {
                int _id = g.Key ?? 0;
                parId.Add(_id);
                var par = li.Where(c => c.Id == _id).Select(c => new {
                    c.Id,
                    ParentId = "",
                    c.ExtendName,
                    c.ExtendNumber,
                    c.UpdatedTime,
                    c.CreatedTime,
                    c.IsDeleted,
                    c.ImgPath
                }).FirstOrDefault();


                var childs = g.Select(c => new {
                    ParentId = g.Key,
                    c.Id,
                    c.ExtendName,
                    c.ExtendNumber,
                    c.CreatedTime,
                    c.UpdatedTime,
                    c.IsDeleted,
                    c.ImgPath

                }).ToList();
                if (par != null)
                    datli.Add(par);
                if (childs.Any())
                    datli.AddRange(childs);

            }
            var otherpar = li.Where(c => c.ParentId == null && !parId.Contains(c.Id)).Select(c => new {
                ParentId = "",
                c.Id,
                c.ExtendName,
                c.ExtendNumber,
                c.Descript,
                c.CreatedTime,
                c.UpdatedTime,
                c.IsDeleted,
                c.ImgPath
            }).ToList();
            datli.AddRange(otherpar);

            GridData<object> data = new GridData<object>(datli, coun, req.RequestInfo);
            return Json(data);
        }
        [HttpGet]
        public ActionResult Create()
        {

            var li = _maintainContract.Maintains.Where(c => c.ParentId == null && c.IsEnabled && !c.IsDeleted).Select(c => new SelectListItem()
              {
                  Text = c.ExtendName,
                  Value = c.Id.ToString()
              }).ToList();
            li.Insert(0, new SelectListItem()
            {
                Text = "请下拉选择",
                Value = ""
            });
            ViewBag.Parents = li;

            return PartialView();
        }
        [HttpPost]
        public ActionResult Create(MaintainExtend maintain)
        {

            maintain.Descript = (maintain.Descript ?? "").Replace("%lt%", "<").Replace("%gt%", ">");
            var resul = _maintainContract.Insert(maintain);
            return Json(resul);
        }
        public ActionResult View(int id)
        {
            var rs = _maintainContract.Maintains.FirstOrDefault(c => c.Id == id);
            return PartialView(rs);
        }
        public ActionResult Update(int id)
        {
            var rs = _maintainContract.Maintains.FirstOrDefault(c => c.Id == id);
            rs.Descript = rs.Descript ?? "";

            return PartialView(rs);
        }
        [HttpPost]
        public ActionResult Update(MaintainExtend maintain)
        {
            maintain.Descript = (maintain.Descript ?? "").Replace("%lt%", "<").Replace("%gt%", ">");
            var resul = _maintainContract.Update(maintain);
            return Json(resul);
        }

        public ActionResult Remove(int[] ids)
        {
            var res = _maintainContract.Remove(ids);
            return Json(res);
        }

        public ActionResult Recovery(int[] ids)
        {
            var res = _maintainContract.Recovery(ids);
            return Json(res);
        }
        public ActionResult GetMaintainByIds(string ids)
        {
            OperationResult resul = new OperationResult(OperationResultType.Error);
            if (!string.IsNullOrEmpty(ids))
            {
                List<int> idli =
                    ids.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(c => Convert.ToInt32(c))
                        .ToList();
                var li = _maintainContract.Maintains.Where(c => idli.Contains(c.Id))
                    .Where(c => c.IsEnabled && !c.IsDeleted)
                    .Select(c => new {
                        c.ExtendName,
                        c.Id,
                        c.ParentId,
                        c.ImgPath
                    }).GroupBy(c => c.ParentId).ToList();
                List<object> resLi = new List<object>();
                foreach (var ite in li)
                {
                    var parent = _maintainContract.Maintains.Where(c => c.Id == ite.Key).Select(c => new {
                        c.ExtendName,
                        c.Id,
                        c.ImgPath
                    }).FirstOrDefault();
                    var childrens = ite.Select(c => new {
                        c.ExtendName,
                        c.Id,
                        c.ImgPath
                    }).ToList();
                    if (parent != null)
                    {
                        resLi.Add(new {
                            parent.ExtendName,
                            parent.Id,
                            childrens = childrens,
                            
                        });
                    }


                }
                resul = new OperationResult(OperationResultType.Success);
                resul.Data = resLi;

            }
            else
            {
                var li = _maintainContract.Maintains
                   .Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId != null)
                   .Select(c => new {
                       c.ExtendName,
                       c.Id,
                       ParentId = c.ParentId + "",
                       c.ImgPath
                   }).GroupBy(c => c.ParentId).ToList();

                var parids = li.Select(c => Convert.ToInt32(c.Key)).ToList();
                var othli = _maintainContract.Maintains.Where(c => c.IsEnabled && !c.IsDeleted && c.ParentId == null && !parids.Contains(c.Id))
                     .ToList()
                     .Select(c => new {
                         c.ExtendName,
                         c.Id,
                         ParentId = "e" + Guid.NewGuid().ToString(),
                         c.ImgPath
                     }).GroupBy(c => c.ParentId).ToList();
                li.AddRange(othli);

                List<object> resLi = new List<object>();
                foreach (var ite in li)
                {
                    int _id = -1;
                    if (!ite.Key.StartsWith("e"))
                        _id = Convert.ToInt32(ite.Key);
                    var parent = _maintainContract.Maintains.Where(c => c.Id == _id).Select(c => new {
                        c.ExtendName,
                        c.Id,
                        c.ImgPath
                    }).FirstOrDefault();
                    var childrens = ite.Select(c => new {
                        c.ExtendName,
                        c.Id,
                        c.ImgPath
                    }).ToList();
                    if (parent != null)
                    {
                        resLi.Add(new {
                            parent.ExtendName,
                            parent.Id,
                            childrens = childrens
                        });
                    }
                    else
                    {
                        var te = ite.Select(c => new {
                            c.ExtendName,
                            c.Id,
                            c.ImgPath
                        }).FirstOrDefault();
                        resLi.Add(te);
                    }


                }
                resul = new OperationResult(OperationResultType.Success);
                resul.Data = resLi;
            }
            return Json(resul);
        }
        /// <summary>
        /// 上传保养维护图片
        /// </summary>
        /// <returns></returns>
        public ActionResult UploadImage()
        {
            OperationResult result = new OperationResult(OperationResultType.Error, "文件保存异常");
            int filecoun = Request.Files.Count;
            if (filecoun != 0)
            {
                try
                {
                    var file = Request.Files[0];
                    string abstrucPath = "/Content/UploadFiles/Imgs/Maintain/" + DateTime.Now.ToString("yyyyMMddHH");
                    string savePath = Server.MapPath(abstrucPath);
                    if (!Directory.Exists(savePath))
                        Directory.CreateDirectory(savePath);
                    string name = Guid.NewGuid().ToString().Replace("-", "") + Path.GetExtension(file.FileName);
                    string fullpath = savePath + "\\" + name;
                    file.SaveAs(fullpath);
                    result = new OperationResult(OperationResultType.Success, "ok", abstrucPath + "//" + name);
                }
                catch (Exception ex)
                {

                    throw;
                }
            }
            return Json(result);
        }
    }
}