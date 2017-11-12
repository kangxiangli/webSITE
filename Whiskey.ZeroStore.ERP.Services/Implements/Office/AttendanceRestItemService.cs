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
    public class AttendanceRestItemService : ServiceBase, IAttendanceRestItemContract
    {
         #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(AttendanceRestItemService));

        private readonly IRepository<AttendanceRestItem, int> _attendanceRestItemRepository;

        private readonly IRepository<Administrator, int> _adminRepository;

        private readonly IRepository<Rest, int> _restRepository;

        public AttendanceRestItemService(IRepository<AttendanceRestItem, int> AttendanceRestItemRepository,
            IRepository<Administrator, int> adminRepository,
            IRepository<Rest, int> restRepository)
            : base(AttendanceRestItemRepository.UnitOfWork)
		{
            _attendanceRestItemRepository = AttendanceRestItemRepository;
            _adminRepository = adminRepository;
            _restRepository = restRepository;
		}
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AttendanceRestItem View(int Id)
        {
            var entity = _attendanceRestItemRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public AttendanceRestItemDto Edit(int Id)
        {
            var entity = _attendanceRestItemRepository.GetByKey(Id);
            Mapper.CreateMap<AttendanceRestItem, AttendanceRestItemDto>();
            var dto = Mapper.Map<AttendanceRestItem, AttendanceRestItemDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<AttendanceRestItem> AttendanceRestItems { get { return _attendanceRestItemRepository.Entities; } }
        #endregion

        #region 添加数据
               
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params AttendanceRestItemDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;
                foreach (var item in dtos)
                {
                    Rest rest = _restRepository.Entities.Where(x => x.AdminId == item.AdminId).FirstOrDefault();
                    if (rest==null)
                    {
                        rest = new Rest()
                        {
                            AdminId=item.AdminId,
                            ChangeRestDays=item.Days,
                        };
                        _restRepository.Insert(rest);
                    }
                    else
                    {
                        rest.ChangeRestDays = rest.ChangeRestDays + item.Days;
                        _restRepository.Update(rest);
                    }
                }
                
                OperationResult result = _attendanceRestItemRepository.Insert(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                int resCount = UnitOfWork.SaveChanges();
                return resCount>0?new OperationResult(OperationResultType.Success,"添加成功！")
                    : new OperationResult(OperationResultType.Error,"添加失败！");
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
        public OperationResult Update(params AttendanceRestItemDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;                 
                OperationResult result = _attendanceRestItemRepository.Update(dtos,
                 dto =>
                 {
                 
                 },
                 (dto, entity) =>
                 {
                     entity.UpdatedTime = DateTime.Now;
                     entity.OperatorId = AuthorityHelper.OperatorId;
                     return entity;
                 });
                int resCount = UnitOfWork.SaveChanges();
                return resCount > 0 ? new OperationResult(OperationResultType.Success, "更新成功！")
                    : new OperationResult(OperationResultType.Error, "更新失败！");
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
                var entities = _attendanceRestItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRestItemRepository.Update(entity);
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
                var entities = _attendanceRestItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRestItemRepository.Update(entity);
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
                var entities = _attendanceRestItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRestItemRepository.Update(entity);
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
                var entities = _attendanceRestItemRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _attendanceRestItemRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

 
    }
}
