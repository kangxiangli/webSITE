using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Web.Helper;
using Whiskey.Web.SignalR;
using Whiskey.Web.Http;
using Whiskey.Web.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Helper;
using Whiskey.Utility.Data;
using Whiskey.Utility.Web;
using Whiskey.Utility.Class;
using Whiskey.Utility.Extensions;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Office;
using System.Web.Mvc;
using Whiskey.Utility.Logging;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class WorkLogAttributeService : ServiceBase, IWorkLogAttributeContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(WorkLogService));

        protected readonly IRepository<WorkLog, int> _workLogRepository;

        protected readonly IRepository<WorkLogAttribute, int> _workLogAttributeRepository;


        public WorkLogAttributeService(IRepository<WorkLogAttribute, int> workLogAttributeRepository,
            IRepository<WorkLog, int> workLogRepository)
            : base(workLogAttributeRepository.UnitOfWork)
		{
            _workLogRepository = workLogRepository;
            _workLogAttributeRepository = workLogAttributeRepository;            
		}
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WorkLogAttribute View(int Id)
        {
            var entity = _workLogAttributeRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WorkLogAttributeDto Edit(int Id)
        {
            var entity = _workLogAttributeRepository.GetByKey(Id);
            Mapper.CreateMap<WorkLogAttribute, WorkLogAttributeDto>();
            var dto = Mapper.Map<WorkLogAttribute, WorkLogAttributeDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<WorkLogAttribute> WorkLogAttributes { get { return _workLogAttributeRepository.Entities.Where(w=>w.IsEnabled&&!w.IsDeleted); } }
        #endregion

        #region 添加数据
               
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params WorkLogAttributeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<WorkLogAttribute> listWorkLogAttribute = this.WorkLogAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true);                
                foreach (var dto in dtos)
                {

                    int index = listWorkLogAttribute.Where(x => x.WorkLogAttributeName == dto.WorkLogAttributeName).Count();
                    if (index > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在！");
                    }                                        
                }
                OperationResult result = _workLogAttributeRepository.Insert(dtos,
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
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！");
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params WorkLogAttributeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");

                IQueryable<WorkLogAttribute> listWorkLogAttribute = this.WorkLogAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (var dto in dtos)
                {
                    int count = listWorkLogAttribute.Where(x => x.WorkLogAttributeName == dto.WorkLogAttributeName && x.Id != dto.Id).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在！");
                    }
                }
                OperationResult result = _workLogAttributeRepository.Update(dtos,
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
                _Logger.Error<string>(ex.ToString());
                return new OperationResult(OperationResultType.Error, "服务器忙，请稍后重试！" );
            }
        }
        #endregion

        #region 移除数据

        /// <summary>
        /// 移除数据
        /// </summary>
        /// <param name="ids">要移除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _workLogAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _workLogAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 恢复数据
               
        /// <summary>
        /// 恢复数据
        /// </summary>
        /// <param name="ids">要恢复的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _workLogAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _workLogAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 启用数据
                
        /// <summary>
        /// 启用数据
        /// </summary>
        /// <param name="ids">要启用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Enable(params int[] ids)
        {

            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _workLogAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _workLogAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 禁用数据
        
        /// <summary>
        /// 禁用数据
        /// </summary>
        /// <param name="ids">要禁用的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _workLogAttributeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _workLogAttributeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取父级下拉选项列表
        /// <summary>
        /// 获取数据下拉选项列表
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<WorkLogAttribute> listWorkLogAttribute = this.WorkLogAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId == null);
            foreach (WorkLogAttribute item in listWorkLogAttribute)
            {
                list.Add(new SelectListItem() { Text = item.WorkLogAttributeName, Value = item.Id.ToString() });
            }
            if (!String.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = string.Empty });
            }
            return list;
        }
        #endregion

        #region 获取父级和子集的下拉选项
        public List<SelectListItem> SelectGroupList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<WorkLogAttribute> listWorkLogAttribute = this.WorkLogAttributes.Where(x => x.IsDeleted == false && x.IsEnabled == true && x.ParentId != null);
            foreach (WorkLogAttribute item in listWorkLogAttribute)
            {
                SelectListItem listItem = new SelectListItem()
                {
                    Group = new SelectListGroup() { Name=item.Parent.WorkLogAttributeName},
                    Text = item.WorkLogAttributeName,
                    Value = item.Id.ToString(),
                };
                list.Add(listItem);
            }
            if (!String.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = string.Empty });
            }
            return list;
        }
        #endregion
    }
}
