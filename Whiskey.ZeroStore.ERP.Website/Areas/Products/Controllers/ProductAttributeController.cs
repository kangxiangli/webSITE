


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.IO;
using Whiskey.Utility.Class;
using Whiskey.Utility.Filter;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Web.Helper;
using Whiskey.Utility.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Entities.Template;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models.Entities;
using System.Text;
using Whiskey.Utility.Data;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Data.SqlClient;
using System.Data.Mapping;
using System.Data.Linq;
using Antlr3.ST;
using Antlr3.ST.Language;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Products.Controllers
{

    [License(CheckMode.Verify)]
	public class ProductAttributeController : BaseController
    {

        #region 初始化数据层操作对象
        
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ProductAttributeController));

        protected readonly IProductAttributeContract _productattributeContract;

		public ProductAttributeController(IProductAttributeContract productattributeContract) {
			_productattributeContract = productattributeContract;
            
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
            ViewBag.ProductAttribute = (_productattributeContract.SelectList(title).Select(m => new SelectListItem { Text = m.Key, Value = m.Value })).ToList();
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
            ViewBag.ProductAttribute = _productattributeContract.SelectOption(title);
            IQueryable<ProductAttribute> listProductAttr=_productattributeContract.ProductAttributes;
            listProductAttr = listProductAttr.Where(x => x.ParentId == null);
            string strCode = CalculateCode(listProductAttr);            
            ViewBag.CodeNum = strCode;
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
        public ActionResult Create(ProductAttributeDto dto)
        {
            string secondParentId = Request["SecondParentId"];
            string thirdParentId = Request["ThirdParentId"];
            if (!string.IsNullOrEmpty(secondParentId))
            {
                dto.ParentId = int.Parse(secondParentId);
            }
            if (!string.IsNullOrEmpty(thirdParentId))
            {
                dto.ParentId = int.Parse(thirdParentId);
            }            
            var result = _productattributeContract.Insert(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 载入修改数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            string title = "请选择";
            ViewBag.ProductAttribute = _productattributeContract.SelectOption(title);
            var result = _productattributeContract.Edit(Id);
            string parentName = string.Empty;
            if(result.ParentId!=null)
            {
                int parentId = result.ParentId ?? 0;
                parentName = _productattributeContract.Edit(parentId).AttributeName; ;
            }
            ViewBag.Name = parentName;
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
        public ActionResult Update(ProductAttributeDto dto)
        {
            var result = _productattributeContract.Update(dto);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region 获取编码和属性
        /// <summary>
        /// 获取编码和属性
        /// </summary>
        /// <returns></returns>
        public JsonResult GetCode()
        {
            string strParentId = Request["ParentId"];
            IQueryable<ProductAttribute> listProductAttr = _productattributeContract.ProductAttributes;
            string parentCode = string.Empty;
            if (!string.IsNullOrEmpty(strParentId))
            {
                int parentId = int.Parse(strParentId);
                parentCode = listProductAttr.Where(x => x.Id == parentId).FirstOrDefault().CodeNum;                
                listProductAttr = listProductAttr.Where(x => x.ParentId == parentId);
                string strCode = parentCode + CalculateCode(listProductAttr);
                var data = listProductAttr.Select(x => new
                {
                    x.Id,
                    x.AttributeName,
                });
                var entity = new
                {
                    CodeNum = strCode,
                    Data = data,
                };
                return Json(entity, JsonRequestBehavior.AllowGet);
            }
            else
            {
                listProductAttr = listProductAttr.Where(x => x.ParentId == null);
                string strCode = parentCode + CalculateCode(listProductAttr);                
                var entity = new
                {
                    CodeNum = strCode,
                    Data = string.Empty,
                };
                return Json(entity, JsonRequestBehavior.AllowGet);
            }
           
        }
        #endregion

        #region 计算编码
        /// <summary>
        /// 计算编码
        /// </summary>
        /// <returns></returns>
        private string CalculateCode(IQueryable<ProductAttribute> listProductAttr)
        {
            //ASCII  0-9 48-57
            //       A-Z 65-90
            int numStart = 48;
            int numEnd = 57;
            int letterStart = 65;
            int letterEnd = 90;
            if (listProductAttr!=null && listProductAttr.Count() > 0)
            {
                List<string> listCodeNum = listProductAttr.Select(x => x.CodeNum).ToList();                
                int start = 0;
                int end = 0;
                int length = listCodeNum.FirstOrDefault().Length;
                int fristIndex = length - 2;
                int secondIndex = length - 1;
                foreach (string codeNum in listCodeNum)
                {
                    byte[] byffer = Encoding.ASCII.GetBytes(codeNum);
                    if (byffer[fristIndex] > start)
                    {
                        start = byffer[fristIndex];
                        end = byffer[secondIndex];

                    }
                    if (byffer[fristIndex] == start)
                    {
                        if (byffer[secondIndex] > end)
                        {
                            end = byffer[secondIndex];
                        }
                    }
                }
                if (end == numEnd)
                {
                    end = letterStart;
                }
                else if (end == letterEnd)
                {
                    end = numStart;
                    if (start < numEnd)
                    {
                        start += 1;
                    }
                    else
                    {
                        start = letterStart;
                    }
                }
                else
                {
                    end += 1;
                }
                byte[] array = { (byte)start, (byte)end };
                string code = Encoding.ASCII.GetString(array);                
                return code;
            }
            else
            {
                int start = numStart + 1;
                byte[] array = { (byte)numStart, (byte)(start) };
                string code = Encoding.ASCII.GetString(array);
                return code;
            }            
        }
        #endregion



        /// <summary>
        /// 查看数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
		[Log]
        public ActionResult View(int Id)
        {
            var result = _productattributeContract.View(Id);
            return PartialView(result);
        }

        #region 注释代码-查询数据
                
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        //public async Task<ActionResult> List()
        //{
        //    GridRequest request = new GridRequest(Request);
        //    Expression<Func<ProductAttribute, bool>> predicate = FilterHelper.GetExpression<ProductAttribute>(request.FilterGroup);
        //    var data = await Task.Run(() =>
        //    {
        //        var count = 0;
        //        var list = _productattributeContract.ProductAttributes.Where<ProductAttribute, int>(predicate, request.PageCondition, out count).Select(m => new
        //        {
        //            m.ParentId,
        //            m.AttributeName,
        //            m.AttributeLevel,
        //            m.Description,
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

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ProductAttribute, bool>> predicate = FilterHelper.GetExpression<ProductAttribute>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<ProductAttribute>, List<ProductAttribute>> getTree = null;                
                getTree = (source) =>
                {
                    var children = source.OrderByDescending(o => o.Id);
                    List<ProductAttribute> tree = new List<ProductAttribute>();
                    foreach (var child in children)
                    {
                        
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
                int count;
                var parents = _productattributeContract.ProductAttributes.OrderByDescending(x=>x.Id).Where(x=>x.ParentId==null).Where<ProductAttribute,int>(predicate, request.PageCondition, out count).ToList();
                var list = getTree(parents).AsQueryable().Select(m => new
                {
                    m.ParentId,
                    m.AttributeName,
                    m.AttributeLevel,
                    m.Description,
                    m.Id,
                    m.IconPath,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,
                    m.CodeNum
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }



        /// <summary>
        /// 树状列表
        /// </summary>
        /// <returns></returns>
        [License(CheckMode.Check)]
        public async Task<ActionResult> TreeList()
        {

            var data = await Task.Run(() =>
            {
                var list = _productattributeContract.ProductAttributes.OrderBy(o => o.Sequence).ThenBy(t => t.CreatedTime).Select(m => new
                {
                    id = m.Id,
                    pid = m.ParentId,
                    name = m.AttributeName
                });
                return list;
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
			var result = _productattributeContract.Remove(Id);
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
			var result = _productattributeContract.Delete(Id);
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
			var result = _productattributeContract.Recovery(Id);
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
			var result = _productattributeContract.Enable(Id);
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
			var result = _productattributeContract.Disable(Id);
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
            var list = _productattributeContract.ProductAttributes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
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
            var list = _productattributeContract.ProductAttributes.Where(m => Id.Contains(m.Id)).OrderByDescending(m => m.Id).ToList();
            var group = new StringTemplateGroup("all", path, typeof(TemplateLexer));
            var st = group.GetInstanceOf("Exporter");
            st.SetAttribute("list", list);
            return Json(new { version = EnvironmentHelper.ExcelVersion(), html = st.ToString() }, JsonRequestBehavior.AllowGet);
        }




        /// <summary>
        /// 属性描述
        /// </summary>
        /// <param name="Id"></param>  
        /// <returns></returns>
        [License(CheckMode.Check)]
        public ActionResult Tooltip(int Id)
        {
            var result = new OperationResult(OperationResultType.Error, "加载属性描述失败！");
            var entity = _productattributeContract.View(Id);
            if (entity != null)
            {
                result = new OperationResult(OperationResultType.Success, "加载属性描述成功！", new { Description = entity.Description });
            }
            return Json(result);
        }


        #region 上传图片
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <returns></returns>
        public JsonResult UploadImage()
        {
            string savePath = ConfigurationHelper.GetAppSetting("ProductAttrIconPath") + DateTime.Now.ToString("yyyyMMddhhmmss") + ".png";             
            var file = Request.Files;
            bool result = false;
            for (int i = 0; i < file.Count; i++)
            {
                result = FileHelper.SaveUpload(file[i].InputStream, savePath);
            }
            if (result)
            {
                string url = ConfigurationHelper.GetAppSetting("WebUrl") + savePath;
                return Json(new { ResultType = OperationResultType.Success, Path = url }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { ResultType = OperationResultType.Error, path = "" }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion

        public ActionResult GetAttributeImages(int Id)
        {
            var result = new List<object>();
            List<string> imgarr = new List<string>();

            imgarr = _productattributeContract.ProductAttributes.Where(c => c.Id == Id).SelectMany(c => c.ProductAttributeImage).Select(s => s.OriginalPath).ToList();
            if (imgarr.Any())
            {
                var counter = 1;
                foreach (var item in imgarr)
                {
                    var filePath = FileHelper.UrlToPath(item);
                    if (System.IO.File.Exists(filePath))
                    {
                        FileInfo fileInfo = new FileInfo(filePath);
                        result.Add(new
                        {
                            ID = counter.ToString(),
                            FileName = item,
                            FilePath = item,
                            FileSize = fileInfo.Length
                        });
                        counter++;
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}
