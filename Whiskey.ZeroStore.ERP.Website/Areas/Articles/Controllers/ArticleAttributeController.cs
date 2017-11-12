using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
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
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.Utility.Helper;
using System.Text.RegularExpressions;
using System.Text;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Base;

namespace Whiskey.ZeroStore.ERP.Website.Areas.Articles.Controllers
{
    [License(CheckMode.Verify)]
    public class ArticleAttributeController : BaseController
    {
        #region 初始化操作对象
        /// <summary>
        /// 初始化日志
        /// </summary>
        protected static readonly ILogger _Logger = LogManager.GetLogger(typeof(ArticleAttributeController));
        protected readonly IArticleAttributeContract _ArticleAttributeContract;
        protected readonly ITemplateContract _TemplateContract;
        protected readonly ITemplateArticleAttrRelationshipContract _TemplateArticleAttrContract;
        protected readonly IArticleContract _ArticleContract;        
        protected readonly ITemplateArticleAttrRelationshipContract _TemArticleAttrContract;

        /// <summary>
        /// 初始化业务层操作对象
        /// </summary>
        public ArticleAttributeController(IArticleAttributeContract articleAttributeContract,
            ITemplateContract templateContract,
            ITemplateArticleAttrRelationshipContract templateArticleAttrContract,
            IArticleContract articleContract,            
        ITemplateArticleAttrRelationshipContract temArticleAttrContract)
        {
            _ArticleAttributeContract = articleAttributeContract;
            _TemplateContract = templateContract;
            _TemplateArticleAttrContract = templateArticleAttrContract;
            _ArticleContract = articleContract;            
            _TemArticleAttrContract = temArticleAttrContract;            
		}
        #endregion

        #region 初始化界面
        /// <summary>
        /// 初始化界面
        /// </summary>
        /// <returns></returns>
        [Layout]
        public ActionResult Index()
        {
            return View();
        }
        #endregion

        #region 获取属性列表
        /// <summary>
        /// 查询数据
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<ArticleAttribute, bool>> predicate = FilterHelper.GetExpression<ArticleAttribute>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                Func<ICollection<ArticleAttribute>, List<ArticleAttribute>> getTree = null;
                getTree = (source) =>
                {
                    var children = source.OrderBy(o => o.Sequence).ThenBy(o => o.Id);
                    List<ArticleAttribute> tree = new List<ArticleAttribute>();
                    foreach (var child in children)
                    {
                        tree.Add(child);
                        tree.AddRange(getTree(child.Children));
                    }
                    return tree;
                };
                var parents = _ArticleAttributeContract.ArticleAttributes.Where(m => m.ParentId == null).ToList();
                var list = getTree(parents).AsQueryable().Where(predicate).Select(m => new
                {
                    m.ParentId,
                    m.AttributeName,                    
                    m.Description,
                    m.Id,
                    m.IsDeleted,
                    m.IsEnabled,
                    m.Sequence,
                    m.UpdatedTime,
                    m.CreatedTime,
                    m.Operator.Member.MemberName,                    
                });
                return new GridData<object>(list, list.Count(), request.RequestInfo);
            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加属性
        /// <summary>
        /// 初始化添加数据
        /// </summary>
        /// <returns></returns>
        public ActionResult Create()
        {
            var entity = _ArticleAttributeContract.SelectList("请选择");
            ViewBag.ArticleAttribute = entity;
            return PartialView();           
        }

        /// <summary>
        /// 添加数据
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]        
        public JsonResult Create(ArticleAttributeDto dto)
        {
            OperationResult oper = _ArticleAttributeContract.Insert(dto);
            return Json(oper);
        }
        #endregion

        #region 查看属性详情
        /// <summary>
        /// 查看数据详情
        /// </summary>
        /// <returns></returns>
        public ActionResult View(int Id)
        {
            var entity = _ArticleAttributeContract.View(Id);
            return PartialView(entity);
        }
        #endregion

        #region 编辑数据
        /// <summary>
        /// 编辑数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var entity = _ArticleAttributeContract.Edit(Id);
            var list = _ArticleAttributeContract.SelectList("请选择");
            ViewBag.ArticleAttribute = list;
            return PartialView(entity);
        }

        /// <summary>
        /// 保存编辑数据
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]       
        public JsonResult Update(ArticleAttributeDto dto)
        {
            var res= _ArticleAttributeContract.Update(dto);
            return Json(res);
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
            var result = _ArticleAttributeContract.Remove(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion        
        
        #region 获取分页模版集合
        /// <summary>
        /// 获取分页模版集合
        /// </summary>
        /// <returns></returns>
        public JsonResult GetSectionTemplateList() 
        {
            string strType = Request["type"];
            int intType = int.Parse(strType);
            var list = _TemplateContract.Templates.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.TemplateType == intType).Select(x => new
            { 
              x.Id,
              x.TemplateName
            });
            string path = string.Empty;
            string parentPath = string.Empty;
            if (intType==3)
            {
                int id=int.Parse(Request["Id"]);
                var articleAttr= _ArticleAttributeContract.ArticleAttributes.Where(x => x.Id == id).FirstOrDefault();
                //path=articleAttr.AttrPath;
            }
            else if (intType == 2)
            {

            }
            return Json(new { list, path, parentPath }, JsonRequestBehavior.AllowGet);
        }
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
            var result = _ArticleAttributeContract.Recovery(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 计算编码
        /// <summary>
        /// 计算编码
        /// </summary>
        /// <returns></returns>
        private string CalculateCode(IQueryable<ArticleAttribute> listArticleAttr)
        {
            //ASCII  0-9 48-57
            //       A-Z 65-90
            int numStart = 48;
            int numEnd = 57;
            int letterStart = 65;
            int letterEnd = 90;
            if (listArticleAttr != null && listArticleAttr.Count() > 0)
            {
                List<string> listCodeNum = listArticleAttr.Select(x => x.ArticleAttrCode).ToList();
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

    }
}