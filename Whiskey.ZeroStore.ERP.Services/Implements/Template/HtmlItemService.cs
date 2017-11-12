using AutoMapper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
    public class HtmlItemService : ServiceBase,IHtmlItemContract
    {
        #region 声明业务层操作对象
        /// <summary>
        /// 声明操作对象
        /// </summary>
        private readonly IRepository<HtmlItem, int> _htmlItemRepository;
        /// <summary>
        /// 初始化操作对象
        /// </summary>
        /// <param name="articleRepository"></param>
        public HtmlItemService(IRepository<HtmlItem, int> templateJSRepository)
            : base(templateJSRepository.UnitOfWork)
        {
            _htmlItemRepository = templateJSRepository;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取JS数据集
        /// </summary>
        public IQueryable<HtmlItem> HtmlItems
        {
            get { return _htmlItemRepository.Entities; }
        }
        #endregion

        #region 查看
        public HtmlItem View(int id)
        {
            HtmlItem htmlItem = _htmlItemRepository.GetByKey(id);
            return htmlItem;
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
                IQueryable<HtmlItem> listJS=HtmlItems.Where(x=>ids.Contains(x.Id));
                foreach (var item in listJS)
                {
                    if (!string.IsNullOrEmpty(item.SavePath))
                    {
                        FileHelper.Delete(item.SavePath);
                    }
                }
                OperationResult result = _htmlItemRepository.Delete(ids);
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
        public List<Values<string, string>> SelectList(string title)
        {
            var list = (_htmlItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true).Select(c => new Values<string, string> { Key = c.ItemName, Value = c.SavePath, })).ToList();
            list.Insert(0, new Values<string, string> { Key = title, Value = "" });
            return list;
        }
        #endregion

        #region 获取编辑数据对象
        /// <summary>
        /// 获取编辑数据对象
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        public HtmlItemDto Edit(int Id)
        {
            HtmlItem js = HtmlItems.Where(x => x.Id == Id).FirstOrDefault();
            Mapper.CreateMap<HtmlItem, HtmlItemDto>();
            var dto = Mapper.Map<HtmlItem, HtmlItemDto>(js);
            return dto;
        }
        #endregion

        #region 添加
        /// <summary>
        /// 添加JS
        /// </summary>
        /// <param name="JSDtos">领域模型实体</param>
        /// <returns></returns>
        public OperationResult Insert(params HtmlItemDto[] dtos)
        {                        
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _htmlItemRepository.Insert(dtos,
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

        #region 批量添加数据
        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="listFile"></param>
        /// <param name="JSDto"></param>
        /// <returns></returns>
        public  List<OperationResult> Insert(HttpFileCollectionBase listFile, HtmlItemDto dto,HtmlItemFlag flag,string savePath)
        {
            List<OperationResult> listOper = new List<OperationResult>();
            try
            {                                               
                StringBuilder sbSavePath = new StringBuilder();                 
                List<HtmlItem> listHtmlItem = _htmlItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.HtmlItemType == (int)flag).ToList();
                StringBuilder sbFileName = new StringBuilder();
                for (int i = 0; i < listFile.Count; i++)
                {                                      
                    sbFileName.Append(listFile[i].FileName);
                    int count =  listHtmlItem.Where(x => x.ItemName == sbFileName.ToString()).Count();
                    if (count>0)
                    {
                        listOper.Add(new OperationResult(OperationResultType.ValidError, "上传文件已经存在", sbFileName.ToString()));
                    }
                    else
                    {
                        sbSavePath.Append(savePath + sbFileName.ToString());
                        bool isSave = FileHelper.SaveUpload(listFile[i].InputStream, sbSavePath.ToString());
                        if (isSave)
                        {
                            dto.ItemName = sbFileName.ToString();
                            dto.SavePath = sbSavePath.ToString();
                            dto.HtmlItemType = (int)flag;
                            var oper= this.Insert(dto);
                            if (oper.ResultType !=OperationResultType.Success)
                            {
                                listOper.Add(new OperationResult(OperationResultType.Error, "添加失败", sbFileName.ToString()));
                            }
                            else
                            {
                                listOper.Add(new OperationResult(OperationResultType.Success, "添加成功", sbFileName.ToString()));
                            }
                        }
                        else
                        {
                            listOper.Add(new OperationResult(OperationResultType.ValidError, "保存失败", sbFileName.ToString()));
                        }                         
                        sbSavePath.Clear();
                        sbFileName.Clear();
                    }   
                }
                return listOper;
            }
            catch (Exception)
            {
                listOper.Add(new OperationResult(OperationResultType.ValidError, "服务器忙，请稍后重试"));
                return listOper;
            }
        }
        #endregion

        #region 更新数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="JSDtos"></param>
        /// <returns></returns>
        public OperationResult Update(params HtmlItemDto[] JSDtos)
        {
            try
            {                
                JSDtos.CheckNotNull("JSDtos");
                OperationResult result = _htmlItemRepository.Update(JSDtos,
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

        #region 批量更新数据
        /// <summary>
        /// 批量添加数据
        /// </summary>
        /// <param name="listFile"></param>
        /// <param name="JSDto"></param>
        /// <returns></returns>
        public  List<OperationResult> Update(HttpFileCollectionBase listFile, HtmlItemDto dto,HtmlItemFlag flag,string savePath)
        {
            List<OperationResult> listOper = new List<OperationResult>();
            try
            {                
                List<HtmlItemDto> listJSDto = new List<HtmlItemDto>();                
                StringBuilder sbSavePath = new StringBuilder();                 
                List<HtmlItem> listHtmlItem = _htmlItemRepository.Entities.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.HtmlItemType == (int)flag).ToList();
                StringBuilder sbFileName = new StringBuilder();
                for (int i = 0; i < listFile.Count; i++)
                {                                      
                    sbFileName.Append(listFile[i].FileName);
                    int count =  listHtmlItem.Where(x => x.ItemName == sbFileName.ToString() && x.Id!=dto.Id).Count();
                    if (count>0)
                    {
                        listOper.Add(new OperationResult(OperationResultType.ValidError, "上传文件已经存在", sbFileName.ToString()));
                    }
                    else
                    {
                        sbSavePath.Append(savePath);
                        bool isSave = FileHelper.SaveUpload(listFile[i].InputStream, sbSavePath.ToString());
                        if (isSave)
                        {

                            dto.ItemName = sbFileName.ToString();
                            dto.SavePath = sbSavePath.ToString();
                            dto.HtmlItemType = (int)flag;
                            var oper= this.Update(dto);
                            if (oper.ResultType !=OperationResultType.Success)
                            {
                                listOper.Add(new OperationResult(OperationResultType.Error, "编辑失败", sbFileName.ToString()));
                            }
                            else
                            {
                                listOper.Add(new OperationResult(OperationResultType.Success, "编辑成功", sbFileName.ToString()));
                            }
                        }
                        else
                        {
                            listOper.Add(new OperationResult(OperationResultType.ValidError, "保存失败", sbFileName.ToString()));
                        }                         
                        sbSavePath.Clear();
                        sbFileName.Clear();
                    }   
                }
                return listOper;
            }
            catch (Exception)
            {
                listOper.Add(new OperationResult(OperationResultType.ValidError, "服务器忙，请稍后重试"));
                return listOper;
            }
        }
        #endregion
    }
}
