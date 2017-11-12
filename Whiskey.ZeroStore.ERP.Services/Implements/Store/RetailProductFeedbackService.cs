
using AutoMapper;
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
using Whiskey.ZeroStore.ERP.Models.Entities;
using Whiskey.ZeroStore.ERP.Services.Contracts;
namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class RetailProductFeedbackService : ServiceBase, IRetailProductFeedbackContract
    {

        private readonly IRepository<RetailProductFeedback, int> _repo;
        private readonly IRepository<Retail, int> _retailRepo;
        private readonly IRepository<Product, int> _productRepo;
        private readonly IRepository<RetailItem, int> _retailItemRepo;

        public RetailProductFeedbackService(IRepository<RetailProductFeedback, int> repo
            , IRepository<Retail, int> retailRepo
            , IRepository<Product, int> productRepo
            , IRepository<RetailItem, int>  retailItemRepo
            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _retailRepo = retailRepo;
            _productRepo = productRepo;
            _retailItemRepo = retailItemRepo;
        }
        public IQueryable<RetailProductFeedback> Entities => _repo.Entities;


        public OperationResult Insert(params RetailProductFeedback[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params RetailProductFeedback[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<RetailProductFeedback> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public RetailProductFeedback Edit(int id)
        {
            return _repo.GetByKey(id);
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

        public RetailProductFeedback View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params RetailProductFeedback[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
                e.UpdatedTime = DateTime.Now;
            });
        }
        private const string _optionKey = "retailproductfeedback:options";

        public List<RetailProductFeedbackOptionDto> GetOptions()
        {
            var dto = RedisCacheHelper.Get<List<RetailProductFeedbackOptionDto>>(_optionKey);
            return dto;
        }

        public OperationResult UpdateOptions(List<RetailProductFeedbackOptionDto> options)
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
            var dto = RedisCacheHelper.Set(_optionKey, options);
            return OperationResult.OK();
        }

        /// <summary>
        /// 提交试穿反馈信息
        /// </summary>
        /// <param name="appointmentId">预约id</param>
        /// <param name="productNumber">商品货号</param>
        /// <param name="feedbacks">反馈信息</param>
        /// <returns></returns>
        public OperationResult SubmitFeedbacks(int memberId, string retailNumber, List<RetailProductFeedbackEntry> entries)
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

                var retailEntity = _retailRepo.Entities.Include(r=>r.RetailItems).Where(a => !a.IsDeleted && a.IsEnabled && a.RetailNumber == retailNumber).FirstOrDefault();
                if (retailEntity == null)
                {
                    return OperationResult.Error("订单信息不存在");
                }
                // 会员校验
                var consumerId = retailEntity.ConsumerId;
                if (!consumerId.HasValue)
                {
                    return OperationResult.Error("非会员订单无法提价反馈");
                }
                else if (consumerId.Value != memberId)
                {
                    return OperationResult.Error("订单信息错误");
                }

                // 商品货号校验
                var feedbackProductNumbers = entries.Select(e => e.ProductNumber).Distinct().ToArray();
             
                var productNumberEntities = _productRepo.Entities.Where(p => !p.IsDeleted && p.IsEnabled && feedbackProductNumbers.Contains(p.ProductNumber))
                                     .Select(p => new { p.Id, p.ProductNumber }).ToArray();
                if (productNumberEntities.Length != feedbackProductNumbers.Length)
                {
                    return OperationResult.Error("反馈商品货号有误");
                }


                // 货号与提交货号对比
                var productNumbersInRetail = retailEntity.RetailItems.Select(i => i.Product.ProductNumber).ToList();
                if (feedbackProductNumbers.Intersect(productNumbersInRetail).Count() != feedbackProductNumbers.Length)
                {
                    return OperationResult.Error("有未包含在订单中的反馈商品");
                }

                // 重复反馈检测
                if (_repo.Entities.Any(e => !e.IsDeleted && e.IsEnabled && e.RetailNumber == retailNumber && feedbackProductNumbers.Contains(e.ProductNumber)))
                {
                    return OperationResult.Error("存在已反馈的商品货号");
                }

                var retailItems = retailEntity.RetailItems.Where(i => feedbackProductNumbers.Contains(i.Product.ProductNumber)).ToList();
                retailItems.Each(i => i.HasFeedback = true);
                _retailItemRepo.Update(retailItems.ToArray());

                var entityToAdd = entries.Select(e => new RetailProductFeedback
                {
                    MemberId = memberId,
                    RetailId = retailEntity.Id,
                    RetailNumber = retailNumber,
                    Feedbacks = JsonHelper.ToJson(e.CheckOptions),
                    ProductNumber = e.ProductNumber,
                    ProductId = productNumberEntities.First(p => p.ProductNumber == e.ProductNumber).Id,
                    RatePoints = e.RatePoints
                }).ToArray();

                var res = Insert(entityToAdd);
                return res;

            }
            catch (Exception)
            {
                return OperationResult.Error("系统错误");

            }
        }
    }
}
