using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Utility.Logging;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Transfers.Enum.Collocation;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class MissionService : ServiceBase, IMissionContract
    {
        #region 声明数据层对象

        private readonly IRepository<Mission, int> _missionRepository;

         

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(MissionService));

        public MissionService(IRepository<Mission, int> missionRepository
             )
            : base(missionRepository.UnitOfWork)
        {
            _missionRepository = missionRepository;            
        }
        #endregion

        #region 查看数据
                
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public Mission View(int Id)
        {
            var entity = _missionRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑数据对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public MissionDto Edit(int Id)
        {
            var entity = _missionRepository.GetByKey(Id);
            Mapper.CreateMap<Mission, MissionDto>();
            var dto = Mapper.Map<Mission, MissionDto>(entity);
            return dto;
        }
        #endregion

        #region 获取优惠卷数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Mission> Missions { get { return _missionRepository.Entities; } }
        #endregion

        #region 获取优惠卷详情数据集
        //public IQueryable<MissionItem> MissionItems { get { return _missionItemRepository.Entities;} }
        #endregion

        #region 按条件检查数据是否存在

        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Mission, bool>> predicate, int id = 0)
        {
            return _missionRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
                
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params MissionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                
                OperationResult result = _missionRepository.Insert(dtos,
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
            catch (System.Data.Entity.Validation.DbEntityValidationException ex)
            {
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 更新数据

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params MissionDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled=true;
                IQueryable<Mission> listMission = Missions.Where(x=>x.IsDeleted==false && x.IsEnabled==true);                
                foreach (var dto in dtos)
                {                                         
                }                 
                OperationResult result = _missionRepository.Update(dtos,
                dto =>
                {

                },
                (dto, entity) =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    return entity;
                });
                int resCount =UnitOfWork.SaveChanges();
                return resCount > 0 ? new OperationResult(OperationResultType.Success, "修改成功") : new OperationResult(OperationResultType.Error, "修改失败！");

            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "修改失败！错误如下：" + ex.Message);
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
                var entities = _missionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _missionRepository.Update(entity);
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
                var entities = _missionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _missionRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 删除数据
                
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="ids">要删除的编号</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _missionRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message);
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
                var entities = _missionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _missionRepository.Update(entity);
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
                var entities = _missionRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _missionRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取完成的任务
        public OperationResult GetList(int memberId, int collocationId, int PageIndex, int PageSize)
        {
            try
            {
                IQueryable<Mission> listMission = this.Missions.Where(x => x.IsDeleted == false && x.IsEnabled == true);
                foreach (Mission mission in listMission)
                {
                    if (mission.MissionItems!=null)
                    {
                        List<MissionItem> listItem = mission.MissionItems.Where(x => x.ScheduleType ==(int)ScheduleFlag.Completed).ToList();
                    }
                }
            }
            catch (Exception)
            {
                
                throw;
            }
            return null;
        }
        #endregion
    }
}
