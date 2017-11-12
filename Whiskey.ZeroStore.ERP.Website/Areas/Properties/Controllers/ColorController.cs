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
using Whiskey.Utility.Helper;
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

namespace Whiskey.ZeroStore.ERP.Website.Areas.Properties.Controllers
{

    [License(CheckMode.Verify)]
	public class ColorController : BaseController
    {
        #region 初始化操作对象
               
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ColorController));

        protected readonly IColorContract _colorContract;

		public ColorController(IColorContract colorContract) {
			_colorContract = colorContract;
            //ViewBag.Color = (_colorContract.SelectList("选择颜色").Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
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
            return View();
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 载入创建数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            string title = "请选择";
            ViewBag.Color = _colorContract.ParentSelectList(title);
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
        public ActionResult Create(ColorDto dto)
        {             
            var result = _colorContract.Insert(dto);             
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {            
            var result = _colorContract.Edit(Id);            
            return PartialView(result);
        }

        
        /// <summary>
        /// 提交数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
		[Log]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Update(ColorDto dto)
        {
            var result = _colorContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 查看详情
        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _colorContract.View(Id);
            return PartialView(result);
        }
        #endregion

        #region 注释代码--查询数据
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    Expression<Func<Color, bool>> predicate = FilterHelper.GetExpression<Color>(request.FilterGroup);
        //    var data = await Task.Run(() =>
        //    {
        //        var count = 0;
        //        var list = _colorContract.Colors.Where<Color, int>(predicate, request.PageCondition, out count).Select(m => new
        //        {
        //            m.ParentId,
        //            m.ColorName,
        //            m.ColorCode,
        //            m.ColorLevel,
        //            m.Description,
        //            m.MinHue,
        //            m.MaxHue,
        //            m.MinSaturation,
        //            m.MaxSaturation,
        //            m.MinLightness,
        //            m.MaxLightness,
        //            m.RGB,
        //            m.HSL,
        //            m.Id,
        //            m.IsDeleted,
        //            m.IsEnabled,
        //            m.Sequence,
        //            m.UpdatedTime,
        //            m.CreatedTime,
        //            m.Operator.AdminName,
        //        }).ToList();
        //        return new GridData<object>(list, count, request.RequestInfo);
        //    });
        //    return Json(data, JsonRequestBehavior.AllowGet);
        //}
        #endregion

        #region 获取数据列表
        /// <summary>
        ///  获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<Color, bool>> predicate = FilterHelper.GetExpression<Color>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                //Func<ICollection<Color>, List<Color>> getTree = null;
                //getTree = (source) =>
                //{
                //    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                //    List<Color> tree = new List<Color>();
                //    foreach (var child in children)
                //    {
                //        tree.Add(child);
                //        //tree.AddRange(getTree(child.Children));
                //    }
                //    return tree;
                //};

                var lis = _colorContract.Colors.Where(predicate).Select(m => new
                {
                  
                    m.ColorName,
                    m.ColorCode,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.Operator.Member.MemberName,
                    m.IconPath,       
                    m.ColorValue             
                }).ToList();

                return new GridData<object>(lis, lis.Count, request.RequestInfo);
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
			var result = _colorContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 删除数据
        ///// <summary>
        ///// 删除数据
        ///// </summary>
        ///// <param name="Id"></param>
        ///// <returns></returns>
        //[Log]
        //[HttpPost]
        //public ActionResult Delete(int[] Id)
        //{
        //    var result = _colorContract.Delete(Id);
        //    return Json(result, JsonRequestBehavior.AllowGet);
        //}
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
			var result = _colorContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 启用和禁用数据
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
		[HttpPost]
        public ActionResult Enable(int[] Id)
        {
			var result = _colorContract.Enable(Id);
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
			var result = _colorContract.Disable(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 打印和导出数据
        /// <summary>
        /// 打印数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult Print(int[] Id)
        {
            var path = Path.Combine(HttpRuntime.AppDomainAppPath, EnvironmentHelper.TemplatePath(this.RouteData));
            var list = _colorContract.Colors.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _colorContract.Colors.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 计算HSL
        /// <summary>
        /// 计算HSL
        /// </summary>
        /// <param name="R"></param>
        /// <param name="G"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        [License(CheckMode.Check)]
        public ActionResult Calculate(int R, int G, int B)
        {
            double h, s, l;
            var result = new OperationResult(OperationResultType.Error, "");
            ColorHelper.RGB2HSL(new ColorRGB(Convert.ToByte(R), Convert.ToByte(G), Convert.ToByte(B)), out h, out s, out l);
            var hueDeg = Math.Round(h * 360);
            if (hueDeg > 330) hueDeg = hueDeg - 360;
            //var entity = _colorContract.Colors.OrderByDescending(m => m.Id).FirstOrDefault(m => m.ParentId != null && hueDeg >= m.MinHue && hueDeg <= m.MaxHue && s >= m.MinSaturation && s <= m.MaxSaturation && l >= m.MinLightness && l <= m.MaxLightness);
            var entity = _colorContract.Colors.OrderByDescending(m => m.Id).FirstOrDefault();
            if (entity != null)
            {
                var data = new
                {
                    Id = entity.Id,
                    ColorName = entity.ColorName,
                    //RGB = entity.RGB,
                    Hue = h,
                    Saturation = s,
                    Lightness = l,
                };
                result = new OperationResult(OperationResultType.Success, "色彩计算成功！", data);
            }
            else
            {
                result = new OperationResult(OperationResultType.Error, "无法从色彩库中计算出与之匹配的色彩：<div style='width:25px;height:25px;background-color:rgb(" + R + "," + G + "," + B + ");'></div>（色相：" + Math.Round(h * 360, 2) + "Deg，纯度：" + Math.Round(s * 100, 2) + "，明度：" + Math.Round(l * 100, 2) + "）");
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            string savePath = ConfigurationHelper.GetAppSetting("ColorIconPath") + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";            
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
