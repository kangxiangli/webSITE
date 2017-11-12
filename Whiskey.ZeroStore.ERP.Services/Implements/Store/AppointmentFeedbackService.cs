
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class AppointmentFeedbackService : ServiceBase, IAppointmentFeedbackContract
    {

        private readonly IRepository<AppointmentFeedback, int> _repo;
        private readonly IRepository<Appointment, int> _appointmentRepo;
        private readonly IRepository<Product, int> _productRepo;
        private readonly IStoreContract _storeContract;

        public AppointmentFeedbackService(IRepository<AppointmentFeedback, int> repo
            , IRepository<Appointment, int> appointmentRepo
            , IRepository<Product, int> productRepo
            , IStoreContract storeContract
            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _appointmentRepo = appointmentRepo;
            _productRepo = productRepo;
            _storeContract = storeContract;

        }
        public IQueryable<AppointmentFeedback> Entities => _repo.Entities;


        public OperationResult Insert(params AppointmentFeedback[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params AppointmentFeedback[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<AppointmentFeedback> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public AppointmentFeedback Edit(int id)
        {
            return _repo.GetByKey(id);
        }

        public List<AppointmentFeedbackOptionDto> GetOptions()
        {
            var dto = RedisCacheHelper.Get<List<AppointmentFeedbackOptionDto>>("appointmentfeedback:options");
            return dto;
        }

        public OperationResult UpdateOptions(List<AppointmentFeedbackOptionDto> options)
        {
            if (options.Select(o => o.Id).Distinct().Count() != options.Count)
            {
                return OperationResult.Error("有重复的id");
            }
            foreach (var item in options)
            {
                if (item.Options.Select(o => o.Value).Distinct().Count() != item.Options.Count)
                {
                    return OperationResult.Error($"itemId:{item.Id}有重复的Value");
                }
                if (item.Options.Any(o => string.IsNullOrEmpty(o.Title)))
                {
                    return OperationResult.Error($"itemId:{item.Id} 检测到空项");
                }
            }
            var dto = RedisCacheHelper.Set("appointmentfeedback:options", options);
            return OperationResult.OK();
        }

        /// <summary>
        /// 提交试穿反馈信息
        /// </summary>
        /// <param name="appointmentId">预约id</param>
        /// <param name="productNumber">商品货号</param>
        /// <param name="feedbacks">反馈信息</param>
        /// <returns></returns>
        public OperationResult SubmitFeedbacks(int adminId, string appointmentId, List<FeedbackEntry> entries)
        {
            try
            {

                if (entries == null || entries.Count <= 0)
                {
                    return OperationResult.Error("提交信息不可为空");
                }


                // options 批量校验
                var options = GetOptions();
                foreach (var entry in entries)
                {
                    foreach (var optionItem in entry.CheckOptions)
                    {
                        var option = options.FirstOrDefault(o => o.Id == optionItem.OptionId);
                        if (option == null)
                        {
                            return OperationResult.Error($"productNumber:{entry.ProductNumber},optionid:{optionItem.OptionId}无效");
                        }
                        if (!option.Multiple && optionItem.Checked.Length > 1)
                        {
                            return OperationResult.Error($"productNumber:{entry.ProductNumber},optionid:{optionItem.OptionId}不可多选");
                        }

                        // 校验check option是否合法
                        if (optionItem.Checked.Any(x => !option.Options.Any(o => o.Value == x)))
                        {
                            return OperationResult.Error("存在无效的选中项");
                        }
                    }

                }


                var appointmentEntity = _appointmentRepo.Entities.Where(a => !a.IsDeleted && a.IsEnabled && a.Number == appointmentId)
                                                                     .Include(a => a.CollocationPlans)
                                                                     .FirstOrDefault();
                if (appointmentEntity == null)
                {
                    return OperationResult.Error("预约信息不存在");
                }
                // 店铺权限校验
                var storeIds = _storeContract.QueryManageStoreId(adminId);
                if (!storeIds.Contains(appointmentEntity.StoreId))
                {
                    return OperationResult.Error("店铺权限错误,无权处理当前预约信息");
                }


                // 预约状态校验
                if (appointmentEntity.State != AppointmentState.已接收)
                {
                    return OperationResult.Error("预约状态不是已接收状态,无法提交反馈");
                }


                // 商品货号校验
                var feedbackProductNumbers = entries.Select(e => e.ProductNumber).Distinct().ToArray();
                var productNumberEntities = _productRepo.Entities.Where(p => !p.IsDeleted && p.IsEnabled && feedbackProductNumbers.Contains(p.ProductNumber))
                                     .Select(p => new { p.Id, p.ProductNumber }).ToArray();
                if (productNumberEntities.Length != feedbackProductNumbers.Length)
                {
                    return OperationResult.Error("反馈商品货号有误");
                }


                if (!appointmentEntity.SelectedPlanId.HasValue)
                {
                    return OperationResult.Error("预约信息未确认搭配方案");
                }

                // 搭配方案货号与提交货号对比

                var selectProductNumbers = appointmentEntity.CollocationPlans.Where(c => !c.IsDeleted && c.IsEnabled && c.Id == appointmentEntity.SelectedPlanId)
                                                                        .Select(c => c.Rules)
                                                                        .ToList()
                                                                        .SelectMany(r => JsonHelper.FromJson<CollocationRulesEntry[]>(r))
                                                                        .Select(r => r.ProductNumber)
                                                                        .Distinct()
                                                                        .ToList();
                if (feedbackProductNumbers.Intersect(selectProductNumbers).Count() != feedbackProductNumbers.Length)
                {
                    return OperationResult.Error("有未包含在搭配方案中的反馈商品");
                }

                // 重复反馈检测
                if (_repo.Entities.Any(e => !e.IsDeleted && e.IsEnabled && e.AppointmentId == appointmentEntity.Id && feedbackProductNumbers.Contains(e.ProductNumber)))
                {
                    return OperationResult.Error("存在已反馈的商品货号");
                }
                using (var transaction = _repo.GetTransaction())
                {
                    // 预约状态改为已试穿
                    appointmentEntity.State = AppointmentState.已试穿;
                    appointmentEntity.UpdatedTime = DateTime.Now;
                    _appointmentRepo.Update(appointmentEntity);

                    var entityToAdd = entries.Select(e => new AppointmentFeedback
                    {
                        AppointmentId = appointmentEntity.Id,
                        Feedbacks = JsonHelper.ToJson(e.CheckOptions),
                        ProductNumber = e.ProductNumber,
                        ProductId = productNumberEntities.First(p => p.ProductNumber == e.ProductNumber).Id
                    }).ToArray();


                    var res = Insert(entityToAdd);
                    transaction.Commit();
                    return res;
                }
              

            }
            catch (Exception)
            {
                return OperationResult.Error("系统错误");

            }
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _repo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public AppointmentFeedback View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params AppointmentFeedback[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }
    }
}
