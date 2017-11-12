using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using AutoMapper;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility.Data;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers.Entities;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{

    public class ScoreRuleService : ServiceBase, IScoreRuleContract
    {
        protected readonly IRepository<ScoreRule, int> _scoreRuleRepository;
        public ScoreRuleService(IRepository<ScoreRule, int> scoreRuleRepository):base(scoreRuleRepository.UnitOfWork)
        {
            _scoreRuleRepository = scoreRuleRepository;
        }

        public IQueryable<ScoreRule> ScoreRules
        {
            get { return _scoreRuleRepository.Entities; }
        }

        public OperationResult Insert(params ScoreRule[] rules)
        {
            int num = 100000;
            OperationResult resul = new OperationResult(OperationResultType.Error);
            foreach (var rule in rules)
            {
                if (_scoreRuleRepository.Entities.Count() > 0) {
                    num = _scoreRuleRepository.Entities.Max(c => c.ScoreNumber);
                }
                rule.ScoreNumber = num + 1;
                //如果当前的积分规则启用，则禁用已有的积分规则
                if (rule.IsEnabled)
                {
                    var ents = _scoreRuleRepository.Entities.Where(c => c.IsEnabled);
                    ents.Each(c =>
                    {
                        c.IsEnabled = false;
                        c.UpdatedTime = DateTime.Now;
                        
                    });
                    _scoreRuleRepository.Update(ents.ToList());
                }
                resul= _scoreRuleRepository.Insert(rule) > 0 ? new OperationResult(OperationResultType.Success) : new OperationResult(OperationResultType.Error);
            }
            return resul;
        }

        public Utility.Data.OperationResult Update(params ScoreRuleDto[] rules)
        {
          var resul= _scoreRuleRepository.Update(rules,null, (dto, ent) =>
            {
                ent = AutoMapper.Mapper.Map<ScoreRule>(dto);
                ent.UpdatedTime = DateTime.Now;
                ent.OperatorId = AuthorityHelper.OperatorId;
                ent.IsEnabled = false;
                return ent;
            });
            return resul;
        }
        public OperationResult Enable(int id)
        {
           var enty= _scoreRuleRepository.Entities.FirstOrDefault(c => id==c.Id);
            if (enty != null)
            {
                enty.IsEnabled = true;
                enty.OperatorId = AuthorityHelper.OperatorId;
                enty.UpdatedTime = DateTime.Now;
            }
            //将已经启用的禁用
           var ents= _scoreRuleRepository.Entities.Where(c => c.IsEnabled);
            ents.Each(c =>
            {
                c.IsEnabled = false;
                c.OperatorId = AuthorityHelper.OperatorId;
                c.UpdatedTime = DateTime.Now;
            });
            var entys = new List<ScoreRule>();
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

        public ScoreRule Current
        {
            get
            {
                
                var entity = _scoreRuleRepository.Entities.FirstOrDefault(s => s.IsEnabled && !s.IsDeleted);
                if (entity == null)
                {
                    throw new Exception("未找到有效的积分规则");
                }
                return entity;
            }
           
        }
    }
}
