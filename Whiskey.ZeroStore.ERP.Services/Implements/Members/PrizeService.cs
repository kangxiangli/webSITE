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
using System.Security.Cryptography;
using System.Text;
using Whiskey.Utility.Secutiry;
using System.Web.Mvc;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class PrizeService : ServiceBase, IPrizeContract
    {
        #region 初始化数据层操作对象

        private readonly IRepository<Prize, int> _prizeRepository;
        public PrizeService(IRepository<Prize, int> prizeRepository)
            : base(prizeRepository.UnitOfWork)
        {
            _prizeRepository = prizeRepository;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体</returns>
        public Prize View(int Id)
        {
            var entity = _prizeRepository.GetByKey(Id);
            return entity;
        }
        #endregion

        #region 根据Id获取数据
        /// <summary>
        /// 获取单个DTO数据
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>数据实体模型</returns>
        public PrizeDto Edit(int Id)
        {
            var entity = _prizeRepository.GetByKey(Id);
            Mapper.CreateMap<Prize, PrizeDto>();
            var dto = Mapper.Map<Prize, PrizeDto>(entity);
            return dto;
        }
        #endregion

        #region 获取数据集
        /// <summary>
        /// 获取数据集
        /// </summary>
        public IQueryable<Prize> Prizes { get { return _prizeRepository.Entities; } }
        #endregion

        #region 按条件检查数据是否存在
        /// <summary>
        /// 按条件检查数据是否存在
        /// </summary>
        /// <param name="predicate">检查谓语表达式</param>
        /// <param name="id">更新的编号</param>
        /// <returns>是否存在</returns>
        public bool CheckExists(Expression<Func<Prize, bool>> predicate, int id = 0)
        {
            return _prizeRepository.ExistsCheck(predicate, id);
        }
        #endregion

        #region 添加数据
        /// <summary>
        /// 添加数据
        /// </summary>
        /// <param name="dtos">要添加的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Insert(params PrizeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                foreach (var dto in dtos)
                {
                    IQueryable<Prize> listPrize = _prizeRepository.Entities.Where(x => x.PrizeName == dto.PrizeName);
                    if (listPrize.Count() > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "奖品名称已经存在");
                    }                     
                }
                OperationResult result = _prizeRepository.Insert(dtos,
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

        #region 修改数据
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="dtos">包含更新数据的DTO数据</param>
        /// <returns>业务操作结果</returns>
        public OperationResult Update(params PrizeDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                IQueryable<Prize> listPrize = Prizes.Where(x=>x.IsDeleted==false && x.IsEnabled==true);
                foreach (var dto in dtos)
                {
                    Prize prize = listPrize.Where(x => x.PrizeName == dto.PrizeName).FirstOrDefault();
                    if (prize != null && prize.Id != dto.Id)
                    {
                        return new OperationResult(OperationResultType.Error, "奖品名称已经存在");
                    }
                    Prize reward = Prizes.Where(x => x.Id == dto.Id).FirstOrDefault();
                    int count = dto.Quantity-reward.Quantity;
                    if (count<0)
                    {
                        int diff = reward.Quantity - reward.GetQuantity;
                        if (diff>0)
                        {
                            int num = diff - (-1) * count;
                            if (num<0)
                            {
                                return new OperationResult(OperationResultType.Error, "奖品数量不能小于会员获取数量");
                            }
                        }
                        else
                        {
                            return new OperationResult(OperationResultType.Error, "奖品已经获取完,不能减少奖品数量");
                        }
                    }
                }
                OperationResult result = _prizeRepository.Update(dtos,
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
                var entities = _prizeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _prizeRepository.Update(entity);
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
                var entities = _prizeRepository.Entities.Where(m => ids.Contains(m.Id));
                var listPrize = this.Prizes.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                foreach (var entity in entities)
                {
                    int count = listPrize.Where(x => x.PrizeName == entity.PrizeName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "恢复失败,和正常的数据名字有重复");
                    }
                    else
                    {
                        entity.IsDeleted = false;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _prizeRepository.Update(entity);
                    }
                   
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
                OperationResult result = _prizeRepository.Delete(ids);
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
                var listPrize = this.Prizes.Where(x => x.IsEnabled == true && x.IsDeleted == false);
                var entities = _prizeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    int count = listPrize.Where(x => x.PrizeName == entity.PrizeName).Count();
                    if (count>0)
                    {
                        return new OperationResult(OperationResultType.Error, "启用失败,和已经启用的数据名字有重复");
                    }
                    else
                    {
                        entity.IsEnabled = true;
                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        _prizeRepository.Update(entity);
                    }                    
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
                var entities = _prizeRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _prizeRepository.Update(entity);
                }
                return UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message);
            }
        }
        #endregion

        #region 获取数据键值对
        /// <summary>
        /// 获取数据键值对
        /// </summary>
        /// <param name="title">默认显示标题</param>
        /// <returns></returns>
        public List<SelectListItem> SelectList(string title)
        {
            List<SelectListItem> list = new List<SelectListItem>();
            IQueryable<Prize> listMember = Prizes.Where(x => x.IsEnabled == true && x.IsDeleted == false);
            if (listMember.Count() > 0)
            {

                foreach (var memeber in listMember)
                {
                    list.Add(new SelectListItem() { Text = memeber.PrizeName, Value = memeber.Id.ToString() });
                }
            }
            if (!string.IsNullOrEmpty(title))
            {
                list.Insert(0, new SelectListItem() { Text = title, Value = ""});    
            }            
            return list;
        }
        #endregion

    }
}
