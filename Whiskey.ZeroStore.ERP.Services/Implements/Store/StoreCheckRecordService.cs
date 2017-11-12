using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Models.DTO;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class StoreCheckRecordService : ServiceBase, IStoreCheckRecordContract
    {
        private readonly IRepository<StoreCheckRecord, int> _storeCheckRecordRepository;
        private readonly IRepository<StoreCheckItem, int> _storeCheckItemRepository;
        public StoreCheckRecordService(
            IRepository<StoreCheckRecord, int> _storeTypeRepository,
            IRepository<StoreCheckItem, int> storeCheckItemRepository
            ) : base(_storeTypeRepository.UnitOfWork)
        {
            _storeCheckRecordRepository = _storeTypeRepository;
            _storeCheckItemRepository = storeCheckItemRepository;
        }

        public IQueryable<StoreCheckRecord> Entities
        {
            get
            {
                return _storeCheckRecordRepository.Entities;
            }
        }

        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _storeCheckRecordRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }



        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _storeCheckRecordRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public StoreCheckRecord View(int Id)
        {
            return _storeCheckRecordRepository.GetByKey(Id);
        }

        public OperationResult Insert(StoreCheckRecordDTO dto, List<CheckInfoModel> checkInfos)
        {
            var allChecks     = _storeCheckItemRepository.Entities.Where(s => !s.IsDeleted && s.IsEnabled).ToList();
            var notExistCount = checkInfos.Select(c => c.checkItemId).Except(allChecks.Select(c => c.Id));
            if (notExistCount.Any())
            {
                return OperationResult.Error($"提交数据有误,以下id不存在:{string.Join(",", notExistCount)}");
            }

            // 生成json数据
            var list = new List<StoreCheckRecordSerializeModel>();
            foreach (var item in allChecks)
            {
                var checkInfo = checkInfos.FirstOrDefault(c => c.checkItemId == item.Id);
                var model = new StoreCheckRecordSerializeModel()
                {
                    Id          = item.Id,
                    CheckName   = item.CheckName,
                    ItemsCount  = item.ItemsCount,
                    Desc        = item.Desc,
                    Standard    = item.Standard,
                    PunishScore = item.PunishScore

                };
                list.Add(model);

                if (checkInfo == null)
                {

                    var checkDetails = item.Items.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries)
                                                 .Select(option => new CheckDetail()
                                                 {
                                                     OptionName = option,
                                                     IsCheck    = false
                                                 });

                    model.CheckDetails.AddRange(checkDetails);


                }
                else
                {
                    var checkDetails = JsonHelper.FromJson<CheckDetail[]>(item.Items);
                    foreach (var checkDetail in checkDetails)
                    {
                        if (checkInfo.checkInfo.Contains(checkDetail.OptionName))
                        {
                            model.CheckDetails.Add(new CheckDetail() { IsCheck = true, OptionName = checkDetail.OptionName });
                        }
                        else
                        {
                            model.CheckDetails.Add(new CheckDetail() { IsCheck = false, OptionName = checkDetail.OptionName });
                        }
                    }

                }
            }

            //计算总罚分
            var standard = 0;

            var punishScore = 0;
            foreach (var item in list)
            {
                standard = item.Standard;
                if (item.CheckDetails.Count(o => o.IsCheck) >= standard)
                {
                    item.GetScore = 1;
                }
                else
                {
                    item.GetScore = 0;
                    punishScore += item.PunishScore; //未达到标准
                }
            }
            dto.TotalPunishScore = punishScore;
            decimal sumScore = list.Any() ? list.Sum(s => s.GetScore) : 0;
            dto.RatingPoints = Math.Round(sumScore / allChecks.Count * 5, 1);
            dto.CheckDetails = JsonHelper.ToJson(list);
            var entity = Mapper.Map<StoreCheckRecord>(dto);

            var count = _storeCheckRecordRepository.Insert(entity);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            else
            {
                return OperationResult.Error("插入失败");
            }
        }

        public OperationResult Insert(params StoreCheckRecordDTO[] dtos)
        {
            throw new NotImplementedException();
        }

        public OperationResult Update(params StoreCheckRecordDTO[] dtos)
        {
            throw new NotImplementedException();
        }

        public StoreCheckRecordDTO Edit(int Id)
        {
            throw new NotImplementedException();
        }

        public OperationResult Insert(params StoreCheckRecord[] entities)
        {
            throw new NotImplementedException();
        }

        public OperationResult Update(params StoreCheckRecord[] entities)
        {
            throw new NotImplementedException();
        }
    }
}
