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
    public class PartnerLevelService : ServiceBase, IPartnerLevelContract
    {
        #region 声明数据层操作对象

        protected readonly ILogger _Logger = LogManager.GetLogger(typeof(PartnerLevelService));

        private readonly IRepository<PartnerLevel, int> _partnerLevelRepository;

        private readonly IRepository<PartnerLevelOrder, int> _partnerLevelOrderRepository;

        private readonly IRepository<Partner, int> _partnerRepository;
        public PartnerLevelService(IRepository<PartnerLevel, int> partnerLevelRepository,
            IRepository<PartnerLevelOrder, int> partnerLevelOrderRepository,
            IRepository<Partner, int> partnerRepository)
            : base(partnerLevelRepository.UnitOfWork)
		{
            _partnerLevelRepository = partnerLevelRepository;
            _partnerLevelOrderRepository = partnerLevelOrderRepository;
            _partnerRepository = partnerRepository;
		}
        #endregion

        #region 查看数据

        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PartnerLevel View(int Id)
        {
            var entity = _partnerLevelRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 获取编辑对象
                
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public PartnerLevelDto Edit(int Id)
        {
            var entity = _partnerLevelRepository.GetByKey(Id);
            Mapper.CreateMap<PartnerLevel, PartnerLevelDto>();
            var dto = Mapper.Map<PartnerLevel, PartnerLevelDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
               
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<PartnerLevel> PartnerLevels { get { return _partnerLevelRepository.Entities; } }
        #endregion

        #region 添加数据
               
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params PartnerLevelDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;                 
                List<PartnerLevel> listPartnerLevel = this.PartnerLevels.Where(x=>x.IsDeleted==false && x.IsEnabled==true).ToList();
                OperationResult oper = new OperationResult(OperationResultType.Error);
                foreach (var dto in dtos)
                {
                    int index = listPartnerLevel.Where(x => x.LevelName == dto.LevelName).Count();
                    if (index > 0)
                    {
                        oper.Message = "名称已经存在";
                        return oper;
                    }
                    index = listPartnerLevel.Where(x => x.Level == dto.Level).Count();
                    if (index>0)
                    {
                        oper.Message = "等级已经存在";
                        return oper;
                    }
                    index = listPartnerLevel.Where(x => x.Experience == dto.Experience).Count();
                    if (index > 0)
                    {
                        oper.Message = "经验值已经存在";
                        return oper;
                    }
                }

                OperationResult result = _partnerLevelRepository.Insert(dtos,
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
        public OperationResult Update(params PartnerLevelDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;                 
                List<PartnerLevel> listPartnerLevel = this.PartnerLevels.Where(x => x.IsDeleted == false && x.IsEnabled == true).ToList();
                OperationResult oper = new OperationResult(OperationResultType.Error);
                foreach (var dto in dtos)
                {
                    int count = listPartnerLevel.Where(x => x.LevelName == dto.LevelName && x.Id!=dto.Id).Count();
                    if (count>0)
                    {
                        oper.Message = "名称已经存在";
                        return oper;
                    }
                    count = listPartnerLevel.Where(x => x.Level == dto.Level && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        oper.Message = "等级已经存在";
                        return oper;
                    }
                    count = listPartnerLevel.Where(x => x.Experience == dto.Experience && x.Id != dto.Id).Count();
                    if (count > 0)
                    {
                        oper.Message = "经验值已经存在";
                        return oper;
                    }
                }
                 

                OperationResult result = _partnerLevelRepository.Update(dtos,
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
                var entities = _partnerLevelRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _partnerLevelRepository.Update(entity);
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
                var entities = _partnerLevelRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _partnerLevelRepository.Update(entity);
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
                var entities = _partnerLevelRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _partnerLevelRepository.Update(entity);
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
                var entities = _partnerLevelRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _partnerLevelRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 购买等级
        public OperationResult OrderLevel(int Id, int PartnerId)
        {
            UnitOfWork.TransactionEnabled = true;
            OperationResult oper = new OperationResult(OperationResultType.Error);
            PartnerLevelOrder order = _partnerLevelOrderRepository.Entities.FirstOrDefault(x => x.IsDeleted == false && x.IsEnabled == true && x.LevelId == Id && x.PartnerId == PartnerId);
            if (order!=null)
            {
                oper.Message = "已经购买了该等级";
                return oper;
            }
            else
            {
                Partner partner = _partnerRepository.GetByKey(PartnerId);
                if (partner!=null)
                {
                    partner.PartnerLevelId = Id;
                    _partnerRepository.Update(partner);
                }
                PartnerLevel level = _partnerLevelRepository.GetByKey(Id);
                order = new PartnerLevelOrder() {
                    Price = level.Price,
                    LevelId=level.Id,
                    PartnerId=PartnerId,
                };
                _partnerLevelOrderRepository.Insert(order);
                int count = UnitOfWork.SaveChanges();
                if (count==0)
                {
                    oper.Message = "购买失败";
                }
                else
                {
                    oper.Message = "购买成功";
                    oper.ResultType = OperationResultType.Success;
                }
                return oper;
            }
        }
        #endregion

    }
}
