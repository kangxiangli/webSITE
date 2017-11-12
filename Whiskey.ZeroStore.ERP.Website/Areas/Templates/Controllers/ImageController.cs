using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Utility.Filter;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Website.Controllers;
using Whiskey.ZeroStore.ERP.Website.Extensions.Attribute;
using Whiskey.ZeroStore.ERP.Website.Extensions.Web;
using Whiskey.Core.Data.Extensions;
using Whiskey.Utility.Helper;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Data;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;
namespace Whiskey.ZeroStore.ERP.Website.Areas.Templates.Controllers
{
    public class ImageController : BaseController
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 声明业务层操作对象
        /// </summary>
        protected readonly IHtmlItemContract _htmlItemContract;
        /// <summary>
        /// 构造函数-初始化业务层操作对象
        /// </summary>
        /// <param name="templateImageContract"></param>
        public ImageController(IHtmlItemContract htmlItemContract) 
        {
            _htmlItemContract = htmlItemContract;
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

        #region 获取数据列表
        /// <summary>
        /// 获取数据列表
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> List()
        {
            GridRequest request = new GridRequest(Request);
            Expression<Func<HtmlItem, bool>> predicate = FilterHelper.GetExpression<HtmlItem>(request.FilterGroup);
            var data = await Task.Run(() =>
            {
                int count = 0;
                var list = _htmlItemContract.HtmlItems.AsQueryable().Where<HtmlItem, int>(predicate, request.PageCondition, out count).Select(m => new
                {
                    m.Id,
                    m.ItemName,
                    m.SavePath,                    
                    m.UpdatedTime
                }).ToList();
                return new GridData<object>(list, count, request.RequestInfo);                

            });
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 初始化添加界面
        /// </summary>
        /// <returns></returns>
        public ActionResult Create() 
        {
            return PartialView();
        }

       
        /// <summary>
        /// 添加图片
        /// </summary>
        /// <param name="Summary">简介</param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Create(HtmlItemDto dto) 
        {
            HttpFileCollectionBase listFile = Request.Files;
            //获取配置文件下的保存路径
            string path = ConfigurationHelper.GetAppSetting("ImagePath");
            var res= _htmlItemContract.Insert(listFile, dto, HtmlItemFlag.Image, path);
            return Json(res);
        }
        #endregion

        #region 修改数据
        /// <summary>
        /// 初始化修改界面
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ActionResult Update(int Id)
        {
            var iamge = _htmlItemContract.Edit(Id);
            return PartialView(iamge);
        }
        /// <summary>
        /// 修改数据
        /// </summary>
        /// <param name="js"></param>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult Update(HtmlItemDto dto)
        {
            HttpFileCollectionBase listFile = Request.Files;
            var entity = _htmlItemContract.View(dto.Id);
            string path=string.Empty;
            if(entity!=null)
            {
                path=entity.SavePath;
            }
            dto.HtmlItemType = (int)HtmlItemFlag.Image;
            List<OperationResult> listOper  = _htmlItemContract.Update(listFile,dto,HtmlItemFlag.Image,path);
            return Json(listOper);
        }

        #endregion

        #region 查看详情
        public ActionResult View(int id) 
        {
            return null;
        }
        #endregion       

        #region 删除数据
        public JsonResult Delete(int Id)
        {
            OperationResult result = _htmlItemContract.Delete(Id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region 获取Image列表
        /// <summary>
        /// 获取Image列表
        /// </summary>
        /// <returns></returns>
        public ActionResult GetList()
        {
            return PartialView();
        }
        #endregion

        #region 获取JS
        /// <summary>
        /// 获取JS集合
        /// </summary>
        /// <returns></returns>
        [Log]
        [HttpPost]
        [ValidateInput(false)]
        public JsonResult GetImageList()
        {
            var result = _htmlItemContract.HtmlItems.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.HtmlItemType == (int)HtmlItemFlag.Image).Select(x => new
            {
                ImageName = x.ItemName,
                ImagePath = x.SavePath,
            });
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}