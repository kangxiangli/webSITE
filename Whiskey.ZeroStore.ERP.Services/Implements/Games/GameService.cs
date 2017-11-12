
using AutoMapper;
using System;
using System.Linq;
using System.Collections.Generic;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;
using Whiskey.ZeroStore.ERP.Models.Enums.Members;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class GameService : ServiceBase, IGameContract
    {
        private readonly IRepository<Game, int> _GameRepository;
        private readonly IRepository<Member, int> _MemberRepository;
        private readonly IMemberShareContract _MemberShareContract;
        private readonly IRepository<GameRule, int> _GameRuleRepository;
        public GameService(
            IRepository<Game, int> _GameRepository,
            IRepository<Member, int> _MemberRepository,
            IMemberShareContract _MemberShareContract,
            IRepository<GameRule, int> _GameRuleRepository
            ) : base(_GameRepository.UnitOfWork)
        {
            this._GameRepository = _GameRepository;
            this._MemberRepository = _MemberRepository;
            this._MemberShareContract = _MemberShareContract;
            this._GameRuleRepository = _GameRuleRepository;
        }

        public IQueryable<Game> Entities
        {
            get
            {
                return _GameRepository.Entities;
            }
        }


        public OperationResult EnableOrDisable(bool enable, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _GameRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public OperationResult Insert(params Game[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");

                var tags = entities.Select(s => s.Tag).ToList();
                var strerror = string.Empty;
                var listtag = _GameRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => tags.Contains(w.Tag)).Select(s => s.Tag).Distinct().ToList();
                if (listtag.IsNotNullThis())
                {
                    var listent = entities.ToList();
                    listent.RemoveIf(f => listtag.Contains(f.Tag));
                    entities = listent.ToArray();
                    strerror = $" 游戏标识:{string.Join(",", listtag)} 已存在";
                }

                OperationResult result = _GameRepository.Insert(entities,
                entity =>
                {
                    entity.CreatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });

                result.Message += strerror;

                return result;
            }, Operation.Add);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _GameRepository.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsDeleted = delete;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, delete ? Operation.Delete : Operation.Recovery);
        }

        public OperationResult Update(params Game[] entities)
        {
            return OperationHelper.Try((opera) =>
            {
                entities.CheckNotNull("entities");
                UnitOfWork.TransactionEnabled = true;

                var tags = entities.Select(s => s.Tag).ToList();
                var listtag = _GameRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => tags.Contains(w.Tag)).Select(s => new { s.Id, s.Tag }).ToList();
                var strerror = string.Empty;
                var listhastag = new List<string>();
                if (listtag.IsNotNullThis())
                {
                    var dtosN = entities.ToList();
                    entities.Each(e =>
                    {
                        var hastag = listtag.Any(c => c.Id != e.Id && c.Tag == e.Tag);
                        if (hastag)
                        {
                            dtosN.RemoveAll(r => r.Tag == e.Tag);
                            listhastag.Add(e.Tag);
                        }
                    });
                    entities = dtosN.ToArray();
                    if (listhastag.Count > 0)
                    {
                        strerror = $" 游戏标识:{string.Join(",", listhastag)} 已存在";
                    }
                }

                OperationResult result = _GameRepository.Update(entities,
                entity =>
                {
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                });
                int count = UnitOfWork.SaveChanges();
                result = OperationHelper.ReturnOperationResult(count > 0, opera);
                result.Message += strerror;
                return result;
            }, Operation.Update);
        }

        public Game View(int Id)
        {
            return _GameRepository.GetByKey(Id);
        }

        public OperationResult Insert(params GameDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                var tags = dtos.Select(s => s.Tag).ToList();
                var strerror = string.Empty;
                var listtag = _GameRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => tags.Contains(w.Tag)).Select(s => s.Tag).Distinct().ToList();
                if (listtag.IsNotNullThis())
                {
                    var listent = dtos.ToList();
                    listent.RemoveIf(f => listtag.Contains(f.Tag));
                    dtos = listent.ToArray();
                    strerror = $" 游戏标识:{string.Join(",", listtag)} 已存在";
                }

                OperationResult result = _GameRepository.Insert(dtos, a => { },
                    (dto, entity) =>
                    {
                        if (dto.GameRuleDtos.IsNotNullOrEmptyThis())
                        {
                            entity.GameRules = dto.GameRuleDtos;
                        }
                        entity.CreatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                result = OperationHelper.ReturnOperationResult(count > 0, opera);
                result.Message += strerror;
                return result;
            }, Operation.Add);
        }

        public OperationResult Update(params GameDto[] dtos)
        {
            return OperationHelper.Try((opera) =>
            {
                dtos.CheckNotNull("dtos");
                UnitOfWork.TransactionEnabled = true;

                var tags = dtos.Select(s => s.Tag).ToList();
                var listtag = _GameRepository.Entities.Where(w => w.IsEnabled && !w.IsDeleted).Where(w => tags.Contains(w.Tag)).Select(s => new { s.Id, s.Tag }).ToList();
                var strerror = string.Empty;
                var listhastag = new List<string>();
                if (listtag.IsNotNullThis())
                {
                    var dtosN = dtos.ToList();
                    dtos.Each(e =>
                    {
                        var hastag = listtag.Any(c => c.Id != e.Id && c.Tag == e.Tag);
                        if (hastag)
                        {
                            dtosN.RemoveAll(r => r.Tag == e.Tag);
                            listhastag.Add(e.Tag);
                        }
                    });
                    dtos = dtosN.ToArray();
                    if (listhastag.Count > 0)
                    {
                        strerror = $" 游戏标识:{string.Join(",", listhastag)} 已存在";
                    }
                }

                OperationResult result = _GameRepository.Update(dtos, a => { },
                    (dto, entity) =>
                    {
                        //entity.GameRules.Clear();

                        _GameRuleRepository.Delete(entity.GameRules);

                        entity.GameRules = dto.GameRuleDtos;

                        entity.UpdatedTime = DateTime.Now;
                        entity.OperatorId = AuthorityHelper.OperatorId;
                        return entity;
                    });
                int count = UnitOfWork.SaveChanges();
                result = OperationHelper.ReturnOperationResult(count > 0, opera);
                result.Message += strerror;
                return result;
            }, Operation.Update);
        }

        public GameDto Edit(int Id)
        {
            var entity = _GameRepository.GetByKey(Id);
            Mapper.CreateMap<Game, GameDto>();
            var dto = Mapper.Map<Game, GameDto>(entity);
            dto.GameRuleDtos = entity.GameRules.ToList();
            return dto;
        }

        public OperationResult AddScore(int MemberId, string Tag, decimal Score)
        {
            return OperationHelper.Try((oper) =>
            {
                var result = new OperationResult(OperationResultType.Error);
                result = JudgeGame(MemberId, Tag);
                if (result.ResultType == OperationResultType.Success)
                {
                    var modGame = result.Data as Game;
                    if (Score > 0)
                    {
                        var mod = _MemberRepository.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == MemberId);
                        if (mod.IsNull())
                        {
                            result.Message = "会员不存在";
                            return result;
                        }
                        UnitOfWork.TransactionEnabled = true;

                        var before = mod.Score;
                        mod.Score += Score;
                        var after = mod.Score;
                        mod.MemberDeposits.Add(new MemberDeposit()
                        {
                            BeforeBalance = mod.Balance,
                            AfterBalance = mod.Balance,
                            BeforeScore = before,
                            Score = Score,
                            AfterScore = after,
                            MemberDepositType = Models.Enums.MemberDepositFlag.System,
                            DepositContext = Models.Enums.MemberDepositContextEnum.游戏获取,
                            MemberActivityType = Models.Enums.MemberActivityFlag.Score
                        });
                        _MemberRepository.Update(mod);

                        modGame.GameRecords.Add(new GameRecord()
                        {
                            MemberId = MemberId,
                            Score = Score,
                        });

                        _GameRepository.Update(modGame);

                        return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, oper);
                    }
                    else
                    {
                        result.Message = "暂不支持减少积分";
                    }
                }
                return result;

            }, "积分到账");
        }
        /// <summary>
        /// 判断是否有游戏可玩次数
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        private OperationResult JudgeGame(int MemberId, string Tag)
        {
            var result = new OperationResult(OperationResultType.Error);
            var modGame = _GameRepository.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Tag == Tag);
            if (modGame.IsNull())
            {
                result.Message = "游戏不存在";
                return result;
            }

            var dateNow = DateTime.Now;

            if (modGame.StartTime > dateNow)
            {
                result.Message = "游戏未开始";
                return result;
            }
            else if (modGame.EndTime.HasValue && modGame.EndTime < dateNow)
            {
                result.Message = "游戏已结束";
                return result;
            }

            var dtnow = dateNow.Date;//今天

            var memberCount = modGame.MemberShares.Count(c => c.MemberId == MemberId && c.IsEnabled && !c.IsDeleted);//分享总次数
            var memberDayCount = modGame.MemberShares.Count(c => c.MemberId == MemberId && c.IsEnabled && !c.IsDeleted && c.CreatedTime >= dtnow);//今日分享次数

            var PlayedCount = modGame.GameRecords.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId);
            var PlayedDayCount = modGame.GameRecords.Count(c => c.IsEnabled && !c.IsDeleted && c.MemberId == MemberId && c.CreatedTime >= dtnow);
            var LimitCount = modGame.LimitCount > 0 ? (modGame.LimitCount + memberCount - PlayedCount) + "" : "无限制";
            var LimitDayCount = modGame.LimitDayCount + memberDayCount - PlayedDayCount;

            if (modGame.LimitCount > 0 && (modGame.LimitCount + memberCount - PlayedCount) <= 0)
            {
                result.Message = "次数已用完";
                return result;
            }
            else if (LimitDayCount <= 0)
            {
                result.Message = "次数已用完";
                return result;
            }
            return new OperationResult(OperationResultType.Success, "", modGame);
        }
        /// <summary>
        /// 判断游戏是否有可分享次数
        /// </summary>
        /// <param name="MemberId"></param>
        /// <param name="Tag"></param>
        /// <returns></returns>
        private OperationResult JudgeShareGame(int MemberId, string Tag)
        {
            var result = new OperationResult(OperationResultType.Error);
            var modGame = _GameRepository.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Tag == Tag);
            if (modGame.IsNull())
            {
                result.Message = "游戏不存在";
                return result;
            }

            var dateNow = DateTime.Now;

            if (modGame.StartTime > dateNow)
            {
                result.Message = "游戏未开始";
                return result;
            }
            else if (modGame.EndTime.HasValue && modGame.EndTime < dateNow)
            {
                result.Message = "游戏已结束";
                return result;
            }

            var dtnow = dateNow.Date;//今天

            var memberCount = modGame.MemberShares.Count(c => c.MemberId == MemberId && c.IsEnabled && !c.IsDeleted);//分享总次数
            var memberDayCount = modGame.MemberShares.Count(c => c.MemberId == MemberId && c.IsEnabled && !c.IsDeleted && c.CreatedTime >= dtnow);//今日分享次数

            if (modGame.LimitShareCount > 0 && memberCount >= modGame.LimitShareCount)
            {
                result.Message = "次数已用完";
                return result;
            }
            else if (memberDayCount >= modGame.LimitShareDayCount)
            {
                result.Message = "次数已用完";
                return result;
            }
            return new OperationResult(OperationResultType.Success, "", modGame);
        }

        public OperationResult Share(int MemberId, string Tag, ShareFlag flag = ShareFlag.游戏)
        {
            return OperationHelper.Try((oper) =>
            {
                var result = new OperationResult(OperationResultType.Error);
                var res = JudgeShareGame(MemberId, Tag);
                if (res.ResultType == OperationResultType.Success)
                {
                    var modGame = res.Data as Game;

                    UnitOfWork.TransactionEnabled = true;

                    modGame.MemberShares.Add(new MemberShare()
                    {
                        MemberId = MemberId,
                        Flag = flag,
                    });

                    _GameRepository.Update(modGame);

                    return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, oper);
                }

                return res;

            }, "游戏分享");
        }

        public OperationResult PlayRandom(int MemberId, string Tag)
        {
            return OperationHelper.Try((oper) =>
            {
                var result = new OperationResult(OperationResultType.Error);
                result = JudgeGame(MemberId, Tag);
                if (result.ResultType == OperationResultType.Success)
                {
                    var modGame = result.Data as Game;
                    var rules = modGame.GameRules.Where(w => w.IsEnabled && !w.IsDeleted).DistinctBy(d => d.GIndex).ToArray();
                    if (rules.IsNullOrEmptyThis())
                    {
                        result.Message = "游戏规则未配置";
                        return result;
                    }
                    var mod = _MemberRepository.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Id == MemberId);
                    if (mod.IsNull())
                    {
                        result.Message = "会员不存在";
                        return result;
                    }

                    var award = (from x in Enumerable.Range(0, 1000000)  //最多支100万次骰子
                                 let oneaward = RandomHelper.random.NextItem(rules)
                                 let rand = (decimal)RandomHelper.random.NextDouble() * 100
                                 where rand < oneaward.Rate
                                 select oneaward).First();

                    UnitOfWork.TransactionEnabled = true;

                    var Score = award.Score;
                    if (Score > 0)
                    {
                        var before = mod.Score;
                        mod.Score += Score;
                        var after = mod.Score;
                        mod.MemberDeposits.Add(new MemberDeposit()
                        {
                            BeforeBalance = mod.Balance,
                            AfterBalance = mod.Balance,
                            BeforeScore = before,
                            Score = Score,
                            AfterScore = after,
                            MemberDepositType = Models.Enums.MemberDepositFlag.System,
                            DepositContext = Models.Enums.MemberDepositContextEnum.游戏获取,
                            MemberActivityType = Models.Enums.MemberActivityFlag.Score
                        });
                        _MemberRepository.Update(mod);
                    }

                    modGame.GameRecords.Add(new GameRecord()
                    {
                        MemberId = MemberId,
                        Score = Score >= 0 ? Score : 0,
                    });

                    _GameRepository.Update(modGame);

                    var status = UnitOfWork.SaveChanges() > 0;
                    return OperationHelper.ReturnOperationResult(status, oper, status ? new { award.GIndex, award.Score } : null);
                }
                return result;

            }, "抽奖");
        }

        public OperationResult GetRandomAward(string Tag, int Count = 3)
        {
            var result = new OperationResult(OperationResultType.Error);
            var modGame = _GameRepository.Entities.FirstOrDefault(f => f.IsEnabled && !f.IsDeleted && f.Tag == Tag);
            if (modGame.IsNull())
            {
                result.Message = "游戏不存在";
                return result;
            }

            var dateNow = DateTime.Now;

            if (modGame.StartTime > dateNow)
            {
                result.Message = "游戏未开始";
                return result;
            }
            else if (modGame.EndTime.HasValue && modGame.EndTime < dateNow)
            {
                result.Message = "游戏已结束";
                return result;
            }
            var rules = modGame.GameRules.Where(w => w.IsEnabled && !w.IsDeleted).DistinctBy(d => d.GIndex).ToArray();
            if (rules.IsNullOrEmptyThis())
            {
                result.Message = "游戏规则未配置";
                return result;
            }
            Count = Count > 0 ? Count : 1;
            var data = (from x in Enumerable.Range(0, 1000000)  //最多支100万次骰子
                        let oneaward = RandomHelper.random.NextItem(rules)
                        let rand = (decimal)RandomHelper.random.NextDouble() * 100
                        where rand < oneaward.Rate && oneaward.Score > 0
                        select oneaward).Take(Count).Select(s => new
                        {
                            //s.GIndex,
                            s.Score,
                            MemberName = RandomHelper.RandomFullName(),
                        });
            return OperationHelper.ReturnOperationResult(true, "", data);
        }
    }
}

