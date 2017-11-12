

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
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.Utility.Helper;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{

    [License(CheckMode.Verify)]
    public class BrandController : BaseController
    {
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(BrandController));

        protected readonly IBrandContract _brandContract;

        public BrandController(IBrandContract brandContract)
        {
            _brandContract = brandContract;
            ViewBag.Brand = (_brandContract.SelectList("").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
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


        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            ViewBag.Brand = _brandContract.ParentSelectList("请选择！");
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
        public ActionResult Create(BrandDto dto)
        {
            var result = _brandContract.Insert(dto);
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
        public ActionResult Update(BrandDto dto)
        {
            List<BrandDto> listDto = new List<BrandDto>();
            listDto.Add(dto);
            IQueryable<Brand> listBrand=  _brandContract.Brands.Where(x => x.ParentId == dto.Id);
            foreach (var brand in listBrand)
            {
               // brand.DefaultDiscount = dto.DefaultDiscount;
                Mapper.CreateMap<Brand, BrandDto>();
                var entityDto = Mapper.Map<Brand, BrandDto>(brand);
                listDto.Add(entityDto);
            }
            var result = _brandContract.Update(listDto.ToArray());            
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            //var parId = CacheAccess.GetBrands(_brandContract).Where(c => c.Id == Id).Select(c => c.ParentId).FirstOrDefault();
            //if (parId != null)
            //{
            //    ViewBag.brands = CacheAccess.GetBrands(_brandContract).Where(c => c.Id == parId).Select(c => new SelectListItem()
            //       {
            //           Text = c.BrandName,
            //           Value = c.Id.ToString()
            //       }).ToList();
            //}
            ViewBag.Brand = _brandContract.ParentSelectList("请选择！");
            var result = _brandContract.Edit(Id);
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
            var result = _brandContract.View(Id);
            return PartialView(result);
        }

        //yxk 2015-10-23修改
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Brand, bool>> predicate = FilterHelper.GetExpression<Brand>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<List<Brand>, List<Brand>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<Brand> tree = new List<Brand>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        var chil = _brandContract.Brands.Where(c => c.ParentId == child.Id).ToList();
                        if (child.ParentId == null)
                        {
                            child.Products = chil.SelectMany(s => s.Products).ToList();//为了 下边的统计 ProductCount用
                        }
                        tree.AddRange(getTree(chil));
                    }
                    return tree;
                };

                var parents = _brandContract.Brands.Where(m => m.ParentId == null).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate)
                .Select(m => new
                {
                    ParentId = m.ParentId == null ? "" : m.ParentId.ToString(),
                    m.BrandName,
                    m.BrandCode,
                    m.DefaultDiscount,
                    ProductCount = m.Products.Count,
                    //m.BrandLevel,
                    m.IconPath,
                    m.Description,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                   IsStory= (m.BrandStory??"").Trim().Length>0
                }).ToList();
                return new GridData<object>(list, list.Count, request.RequestInfo);
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
            var result = _brandContract.Remove(Id);
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
            var result = _brandContract.Delete(Id);
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
            var result = _brandContract.Recovery(Id);
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
            var result = _brandContract.Enable(Id);
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
            var result = _brandContract.Disable(Id);
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
            var list = _brandContract.Brands.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _brandContract.Brands.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {             
            string savePath = ConfigurationHelper.GetAppSetting("BrandIconPath") + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                result = FileHelper.SaveUpload(file[i].InputStream, savePath);
            }
            if (result)
            {
                string url = ConfigurationHelper.GetAppSetting("WebUrl") + savePath;
                return Json(new { ResultType = OperationResultType.Success, Path = savePath }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion
    }
}
