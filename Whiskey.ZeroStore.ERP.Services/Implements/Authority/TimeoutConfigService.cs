using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Services.Content;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.Utility.Extensions;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Extensions;
using Whiskey.Web.Helper;
using AutoMapper;
using System.Linq.Expressions;
using Whiskey.Core.Data;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Authority
{
    public class TimeoutConfigService : ServiceBase, ITimeoutConfigContract
    {
        private readonly IRepository<TimeoutConfig, int> _timeoutConfigRepository;
        public TimeoutConfigService(
            IRepository<TimeoutConfig, int> _timeoutConfigRepository
            )
            : base(_timeoutConfigRepository.UnitOfWork)
        {
            this._timeoutConfigRepository = _timeoutConfigRepository;
        }

        public TimeoutConfig View(int Id)
        {
            var entity = _timeoutConfigRepository.GetByKey(Id);
            return entity;
        }

        public TimeoutConfigDto Edit(int Id)
        {
            var entity = _timeoutConfigRepository.GetByKey(Id);
            var dto = Mapper.Map<TimeoutConfig, TimeoutConfigDto>(entity);
            return dto;
        }

        public IQueryable<TimeoutConfig> TimeoutConfigs
        {
            get { return _timeoutConfigRepository.Entities; }
        }

        public OperationResult Insert(params TimeoutConfigDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _timeoutConfigRepository.Insert(dtos,
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
                return new OperationResult(OperationResultType.Error, "添加失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Insert(params TimeoutConfig[] timeouts)
        {
            timeouts.Each(e => e.OperatorId = AuthorityHelper.GetId());
            return _timeoutConfigRepository.Insert((IEnumerable<TimeoutConfig>)timeouts) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
        }

        public OperationResult Update(params TimeoutConfigDto[] dtos)
        {
            try
            {
                dtos.CheckNotNull("dtos");
                OperationResult result = _timeoutConfigRepository.Update(dtos,
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
                return new OperationResult(OperationResultType.Error, "更新失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Remove(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _timeoutConfigRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _timeoutConfigRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "移除成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "移除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Recovery(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _timeoutConfigRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _timeoutConfigRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "恢复成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "恢复失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Delete(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                OperationResult result = _timeoutConfigRepository.Delete(ids);
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "删除失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Enable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _timeoutConfigRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = true;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _timeoutConfigRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "启用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "启用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public OperationResult Disable(params int[] ids)
        {
            try
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _timeoutConfigRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = false;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    _timeoutConfigRepository.Update(entity);
                }
                var result = UnitOfWork.SaveChanges() > 0 ? new OperationResult(OperationResultType.Success, "禁用成功！") : new OperationResult(OperationResultType.NoChanged, "数据没有变化！");
                return result;
            }
            catch (Exception ex)
            {
                return new OperationResult(OperationResultType.Error, "禁用失败！错误如下：" + ex.Message, ex.ToString());
            }
        }

        public bool CheckExists(Expression<Func<TimeoutConfig, bool>> predicate, int id = 0)
        {
            return _timeoutConfigRepository.ExistsCheck(predicate, id);
        }

        public OperationResult Update(params TimeoutConfig[] timeouts)
        {
            var result = _timeoutConfigRepository.Update(timeouts);
            return result;
        }
    }
}
