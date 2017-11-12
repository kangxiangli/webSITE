using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;

namespace Whiskey.ZeroStore.ERP.Services.Implements.Property
{
    public class StoreValueRuleService : ServiceBase, IStoreValueRuleContract
    {
        protected readonly IRepository<StoreValueRule, int> _scoreRuleRepository;
        public StoreValueRuleService(IRepository<StoreValueRule, int> scoreRuleRepository) : base(scoreRuleRepository.UnitOfWork)
        {
            _scoreRuleRepository = scoreRuleRepository;
        }

        public IQueryable<StoreValueRule> StoreValueRules
        {
            get { return _scoreRuleRepository.Entities; }
        }

        public OperationResult Insert(int ruleType, params StoreValueRuleDto[] rules)
        {
            try
            {
                foreach (var dto in rules)
                {
                    IQueryable<StoreValueRule> listMember = _scoreRuleRepository.Entities.Where(x => x.StoreValueName == dto.StoreValueName);
                    if (listMember.Count() > 0)
                    {
                        return new OperationResult(OperationResultType.Error, "名称已经存在");
                    }
                    if (dto.IsForever == false && dto.EndDate.CompareTo(dto.StartDate) < 0)
                    {
                        return new OperationResult(OperationResultType.Error, "活动结束时期小于开始日期");
                    }
                }
                OperationResult resul = new OperationResult(OperationResultType.Error);
                OperationResult result = _scoreRuleRepository.Insert(rules,
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

        public Utility.Data.OperationResult Update(params StoreValueRuleDto[] rules)
        {
            var resul = _scoreRuleRepository.Update(rules, null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<StoreValueRule>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                return ent;
            });
            return resul;
        }

        public Utility.Data.OperationResult Update(params StoreValueRule[] rules)
        {
            rules.Each(r =>
            {
                r.UpdatedTime = DateTime.Now;
                r.OperatorId = AuthorityHelper.OperatorId;
            });
            var resul = _scoreRuleRepository.Update(rules);
            return resul;
        }
        public OperationResult Enable(int id, int ruleType)
        {
            var enty = _scoreRuleRepository.Entities.FirstOrDefault(c => id == c.Id);
            if (enty != null)
            {
                enty.IsEnabled = true;
                enty.OperatorId = AuthorityHelper.OperatorId;
                enty.UpdatedTime = DateTime.Now;
            }
            //将已经启用的禁用
            var ents = _scoreRuleRepository.Entities.Where(c => c.IsEnabled && c.RuleType == ruleType);
            ents.Each(c =>
            {
                c.IsEnabled = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            var entys = new List<StoreValueRule>();
            entys.Add(enty);
            entys.AddRange(ents);

            return _scoreRuleRepository.Update(entys);
        }
        public OperationResult Disable(params int[] ids)
        {
            var enty = _scoreRuleRepository.Entities.Where(c => ids.Contains(c.Id));
            enty.Each(c =>
            {
                c.IsEnabled = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _scoreRuleRepository.Update(enty.ToList());
        }
        public OperationResult Remove(params int[] ids)
        {
            var entys = _scoreRuleRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = true;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _scoreRuleRepository.Update(entys.ToList());
        }
        public OperationResult Recovery(params int[] ids)
        {
            var entys = _scoreRuleRepository.Entities.Where(c => ids.Contains(c.Id));
            entys.Each(c =>
            {
                c.IsDeleted = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            return _scoreRuleRepository.Update(entys.ToList());
        }

        public StoreValueRule View(int id)
        {
            var entity = _scoreRuleRepository.GetByKey(id);
            return entity;
        }
        public StoreValueRuleDto Edit(int Id)
        {
            var entity = _scoreRuleRepository.GetByKey(Id);
            Mapper.CreateMap<StoreValueRule, StoreValueRuleDto>();
            var dto = Mapper.Map<StoreValueRule, StoreValueRuleDto>(entity);
            return dto;
        }
    }
}
