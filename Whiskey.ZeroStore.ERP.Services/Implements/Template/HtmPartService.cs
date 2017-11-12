using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Class;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Template;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class HtmlPartService : ServiceBase,IHtmlPartContract
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 声明操作对象
        /// </summary>
        private readonly IRepository<HtmlPart, int> _htmlPartRepository;
        /// <summary>
        /// 初始化操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public HtmlPartService(IRepository<HtmlPart, int> templateJSRepository)
            : base(templateJSRepository.UnitOfWork)
        {
            _htmlPartRepository = templateJSRepository;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取JS数据集
        /// </summary>
        public IQueryable<HtmlPart> HtmlParts
        {
            get { return _htmlPartRepository.Entities; }
        }
        #endregion

        #region 查看
        public HtmlPart View(int id)
        {
            HtmlPart htmlPart = _htmlPartRepository.GetByKey(id);
            return htmlPart;
        }
        #endregion

        #region 删除数据（物理）
        /// <summary>
        /// 删除数据（物理）
        /// </summary>
        /// <param name="ids">主键ID集合</param>
        /// <returns></returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                IQueryable<HtmlPart> listJS=HtmlParts.Where(x=>ids.Contains(x.Id));                 
                OperationResult result = _htmlPartRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取集合
        /// <summary>
        /// 获取集合
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title) 
        {

            List<SelectListItem> list = new List<SelectListItem>();
            var entities = _htmlPartRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true);
            foreach (var entity in entities)
            {
                list.Add(new SelectListItem() { Value=entity.Id.ToString(),Text=entity.PartName});
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = "" });    
            }            
            return list;
        }
        #endregion

        #region 获取编辑数据对象
        /// <summary>
        /// 获取编辑数据对象
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        public HtmlPartDto Edit(int Id)
        {
            HtmlPart js = HtmlParts.Where(x => x.Id == Id).FirstOrDefault();
            Mapper.CreateMap<HtmlPart, HtmlPartDto>();
            var dto = Mapper.Map<HtmlPart, HtmlPartDto>(js);
            return dto;
        }
        #endregion
 
        #region 添加
        /// <summary>
        /// 添加JS
        /// </summary>
        /// <param name="JSDtos">领域模型实体</param>
        /// <returns></returns>
        public OperationResult Insert(params HtmlPartDto[] dtos)
        {                        
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _htmlPartRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }

        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="JSDtos"></param>
        /// <returns></returns>
        public OperationResult Update(params HtmlPartDto[] JSDtos)
        {
            try
            {                
                JSDtos.CheckNotNull("JSDtos");
                OperationResult result = _htmlPartRepository.Update(JSDtos,
                    dto =>
                    {

                    },
                    (dto, entity) =>
                    {
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;                        
                        return entity;
                    });
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message);
            }
        }
        #endregion

     }
}
