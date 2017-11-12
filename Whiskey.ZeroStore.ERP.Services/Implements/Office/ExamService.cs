using AutoMapper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Whiskey.Core;
using Whiskey.Core.Data;
using Whiskey.Utility;
using Whiskey.Utility.Collections;
using Whiskey.Utility.Data;
using Whiskey.Utility.Extensions;
using Whiskey.Web.Helper;
using Whiskey.ZeroStore.ERP.Models;
using Whiskey.ZeroStore.ERP.Services.Contracts;
using Whiskey.ZeroStore.ERP.Transfers;

namespace Whiskey.ZeroStore.ERP.Services.Implements
{
    public class ExamService : ServiceBase, IExamContract
    {
        private readonly IRepository<ExamEntity, int> _examRepo;
        public ExamService(IRepository<ExamEntity, int> examRepo) : base(examRepo.UnitOfWork)
        {
            _examRepo = examRepo;


        }
        public IQueryable<ExamEntity> Entities => _examRepo.Entities;


        public OperationResult Insert(params ExamEntity[] entities)
        {
            return _examRepo.Insert(entities, e =>
            {
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params ExamEntity[] entities)
        {
            var count = _examRepo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<ExamEntity> entities)
        {
            return _examRepo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public ExamEntity Edit(int id)
        {
            return _examRepo.GetByKey(id);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _examRepo.Entities.Where(m => ids.Contains(m.Id));
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
                var entities = _examRepo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }

        public ExamEntity View(int Id)
        {
            return _examRepo.GetByKey(Id);
        }

        public OperationResult Update(params ExamEntity[] entities)
        {
            throw new NotImplementedException();
        }
    }


    public class ExamQuestionService : ServiceBase, IExamQuestionContract
    {

        private readonly IRepository<ExamQuestionEntity, int> _examRepo;

        public ExamQuestionService(IRepository<ExamQuestionEntity, int> examRepo) : base(examRepo.UnitOfWork)
        {
            _examRepo = examRepo;


        }
        public IQueryable<ExamQuestionEntity> Entities => _examRepo.Entities;


        public OperationResult Insert(params ExamQuestionEntity[] entities)
        {
            return _examRepo.Insert(entities, e =>
            {
                e.CreatedTime = DateTime.Now;
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params ExamQuestionEntity[] entities)
        {
            var count = _examRepo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<ExamQuestionEntity> entities)
        {
            return _examRepo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public ExamQuestionEntity Edit(int id)
        {
            return _examRepo.GetByKey(id);
        }

        public OperationResult DeleteOrRecovery(bool delete, params int[] ids)
        {
            return OperationHelper.Try((opera) =>
            {
                ids.CheckNotNull("ids");
                UnitOfWork.TransactionEnabled = true;
                var entities = _examRepo.Entities.Where(m => ids.Contains(m.Id));
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
                var entities = _examRepo.Entities.Where(m => ids.Contains(m.Id));
                foreach (var entity in entities)
                {
                    entity.IsEnabled = enable;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                }
                return OperationHelper.ReturnOperationResult(UnitOfWork.SaveChanges() > 0, opera);
            }, enable ? Operation.Enable : Operation.Disable);
        }
    }



    public class ExamRecordService : ServiceBase, IExamRecordContract
    {
        private readonly IRepository<ExamRecordEntity, int> _repo;

        protected readonly IMemberContract _memberContract;
        protected readonly IMemberConsumeContract _memberConsumeContract;
        protected readonly IMemberDepositContract _memberDepositConract;
        private readonly IAdministratorContract _adminContract;
        public ExamRecordService(IRepository<ExamRecordEntity, int> repo,
            IMemberContract memberContract,
            IMemberConsumeContract memberConsumeContract,
            IAdministratorContract adminContract,
            IMemberDepositContract memberDepositConract
            ) : base(repo.UnitOfWork)
        {
            _repo = repo;
            _memberConsumeContract = memberConsumeContract;
            _memberContract = memberContract;
            _adminContract = adminContract;
            _memberDepositConract = memberDepositConract;
        }
        public IQueryable<ExamRecordEntity> Entities => _repo.Entities;


        public OperationResult Insert(params ExamRecordEntity[] entities)
        {
            return _repo.Insert(entities, e =>
            {
                e.CreatedTime = e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }
        public OperationResult Delete(params ExamRecordEntity[] entities)
        {
            var count = _repo.Delete(entities);
            if (count > 0)
            {
                return OperationResult.OK();
            }
            return OperationResult.Error("删除失败");
        }
        public OperationResult Update(ICollection<ExamRecordEntity> entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public ExamRecordEntity Edit(int id)
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

        public ExamRecordEntity View(int Id)
        {
            return _repo.GetByKey(Id);
        }

        public OperationResult Update(params ExamRecordEntity[] entities)
        {
            return _repo.Update(entities, e =>
            {
                e.UpdatedTime = DateTime.Now;
                e.OperatorId = AuthorityHelper.OperatorId;
            });
        }

        public Tuple<bool, decimal, decimal> IsRestartExam(int examRecordId)
        {
            var adminId = AuthorityHelper.OperatorId;
            var entity = _repo.Entities.Include(e => e.Exam)
                .FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == examRecordId && e.AdminId == adminId);

            if (entity.State == ExamRecordStateEnum.已提交 && !entity.IsPass)
            {
                return Tuple.Create<bool, decimal, decimal>(true, entity.Exam.RetryCostScore, entity.Exam.RewardMemberScore ?? 0);
            }

            // 不是补考,返回考试通过获得多少积分
            return Tuple.Create<bool, decimal, decimal>(false, entity.Exam.RewardMemberScore ?? 0, entity.Exam.RetryCostScore);
        }


        public OperationResult StartOrRestartExam(int examRecordId)
        {
            using (var transaction = _repo.GetTransaction())
            {
                try
                {
                    var adminId = AuthorityHelper.OperatorId;

                    var entity = _repo.Entities
                        .Include(e => e.Exam)
                        .Include(e => e.Exam.Questions)
                        .FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == examRecordId && e.AdminId == adminId);
                    if (IsRestartExam(examRecordId).Item1)
                    {
                        // 扣除积分处理
                        var memberId = entity.Admin.MemberId.Value;
                        var memberEntity = _memberContract.Members.FirstOrDefault(m => !m.IsDeleted && m.IsEnabled && m.Id == memberId);
                        var reduceScore = entity.Exam.RetryCostScore;

                        if (memberEntity.Score < reduceScore)
                        {
                            throw new Exception("积分不足");
                        }

                        // 保存积分消费记录
                        _memberConsumeContract.LogScoreWhenRetryExam(memberEntity.Id, reduceScore);


                        // 更新会员积分
                        memberEntity.Score -= reduceScore;
                        var dto = Mapper.Map<MemberDto>(memberEntity);
                        var updateRes = _memberContract.UpdateScore(dto);
                        if (updateRes.ResultType != OperationResultType.Success)
                        {
                            throw new Exception("扣除积分失败");
                        }

                    }


                    // 重置试卷状态
                    entity.State = ExamRecordStateEnum.答题中;
                    entity.StartTimePoint = DateTime.Now;
                    entity.SubmitTimePoint = null;
                    entity.UpdatedTime = DateTime.Now;
                    entity.OperatorId = AuthorityHelper.OperatorId;
                    if (entity.EntryTrainStatus != null && entity.EntryTrainStatus == 0)
                    {
                        entity.EntryTrainStatus = 1;
                    }
                    _repo.Update(entity);


                    // 试卷信息
                    var resData = new tempDTO
                    {
                        ExamRecordId = examRecordId,
                        ExamId = entity.ExamId,
                        ExamName = entity.Exam.Name,
                        ExamMinutesLimit = entity.Exam.MinutesLimit,
                        ExamQuestionCount = entity.Exam.QuestionCount,
                        ExamTotalScore = entity.Exam.TotalScore,
                    };

                    // 试题信息
                    var questions = entity.Exam.Questions.ToList().Select(q => new questionDto
                    {
                        Id = q.Id,
                        Title = q.Title,
                        Score = q.Score,
                        IsMulti = q.IsMulti.Value,
                        AnswerOptions = JsonHelper.FromJson<AnswerOptionEntry[]>(q.AnswerOptions).ToList(),
                        ImgUrl = q.ImgUrl,
                        RightAnswer = q.RightAnswer
                    }).ToList().Shuffle().ToList();
                    resData.Questions = questions;

                    transaction.Commit();
                    return new OperationResult(OperationResultType.Success, string.Empty, resData);
                }
                catch (Exception e)
                {
                    transaction.Rollback();
                    return OperationResult.Error("系统错误" + e.Message);
                }
            }

        }

        public OperationResult SubmitExam(SubmitExamDTO dto)
        {
            using (var transaction = _repo.GetTransaction())
            {
                // 获取试卷信息
                var adminId = AuthorityHelper.OperatorId;
                var examRecordEntity = _repo.Entities
                            .Include(e => e.Exam)
                            .Include(e => e.Exam.Questions)
                            .FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == dto.ExamRecordId && e.AdminId == adminId);

                var examEntity = examRecordEntity.Exam;

                var questionEntities = examEntity.Questions.ToList();

                var answerDetails = dto.AnswerDetail.ToList();

                string[] righAnswer;

                int getScore = 0;
                foreach (var question in questionEntities)
                {
                    righAnswer = question.RightAnswer.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    var userAnswer = answerDetails.FirstOrDefault(a => a.QuestionId == question.Id);
                    if (userAnswer != null && userAnswer.CheckedAnswer != null && userAnswer.CheckedAnswer.Length > 0)
                    {

                        if (userAnswer.CheckedAnswer.Length == righAnswer.Length && userAnswer.CheckedAnswer.Intersect(righAnswer).Count() == userAnswer.CheckedAnswer.Length)
                        {
                            getScore += question.Score;
                        }
                    }
                }


                examRecordEntity.SubmitTimePoint = DateTime.Now;
                examRecordEntity.State = ExamRecordStateEnum.已提交;

                // 判断时间是否有效
                if (examRecordEntity.SubmitTimePoint > examRecordEntity.StartTimePoint.Value.Add(TimeSpan.FromMinutes(examEntity.MinutesLimit)))
                {
                    examRecordEntity.IsPass = false;
                }
                else if (getScore < examEntity.PassLine)
                {
                    examRecordEntity.IsPass = false;
                }
                else
                {
                    examRecordEntity.IsPass = true;
                }

                examRecordEntity.AnswerDetail = JsonHelper.ToJson(dto.AnswerDetail);
                examRecordEntity.UpdatedTime = DateTime.Now;
                examRecordEntity.OperatorId = AuthorityHelper.OperatorId;
                examRecordEntity.GetScore = getScore;


                // 奖励积分
                if (examRecordEntity.IsPass && examEntity.RewardMemberScore.HasValue)
                {
                    examRecordEntity.RewardMemberScore = examEntity.RewardMemberScore.Value;
                    var res = _repo.Update(examRecordEntity);
                    if (res <= 0)
                    {
                        transaction.Rollback();
                        return OperationResult.Error("答题记录保存失败");
                    }


                    var memberEntity = _adminContract.Administrators.Where(a => !a.IsDeleted && a.IsEnabled && a.Id == adminId.Value)
                                        .Select(a => a.Member)
                                        .FirstOrDefault();
                    _memberDepositConract.LogScoreWhenPassExam(memberEntity, examEntity.RewardMemberScore.Value);

                    memberEntity.Score += examEntity.RewardMemberScore.Value;
                    var memberDto = Mapper.Map<MemberDto>(memberEntity);
                    var memberUpdateRes = _memberContract.UpdateScore(memberDto);
                    if (memberUpdateRes.ResultType != OperationResultType.Success)
                    {
                        transaction.Rollback();
                        return OperationResult.Error("会员储值积分更新失败");
                    }
                }
                else // 没有奖励
                {
                    var res = _repo.Update(examRecordEntity);
                    if (res <= 0)
                    {
                        transaction.Rollback();
                        return OperationResult.Error("答题记录保存失败");
                    }
                }

                transaction.Commit();
                return OperationResult.OK();
            }


        }

        public OperationResult GetExamRecordDetail(int examRecordId)
        {
            var entity = _repo.Entities
                      .Include(e => e.Exam)
                      .Include(e => e.Exam.Questions)
                      .FirstOrDefault(e => !e.IsDeleted && e.IsEnabled && e.Id == examRecordId && e.State == ExamRecordStateEnum.已提交);
            if (entity == null)
            {
                return OperationResult.Error("试卷记录未找到或试卷未完成");
            }

            // 试卷信息
            var resData = new tempDTO
            {
                ExamRecordId = examRecordId,
                ExamId = entity.ExamId,
                ExamName = entity.Exam.Name,
                ExamMinutesLimit = entity.Exam.MinutesLimit,
                ExamQuestionCount = entity.Exam.QuestionCount,
                ExamTotalScore = entity.Exam.TotalScore,
                GetScore = entity.GetScore,
                IsPass = entity.IsPass,
                PassLine = entity.Exam.PassLine,
                StartTimePoint = entity.StartTimePoint.Value.ToUnixTime(),
                SubmitTimePoint = entity.SubmitTimePoint.Value.ToUnixTime()
            };

            // 试题信息
            var questions = entity.Exam.Questions.ToList().Select(q => new questionDto
            {
                Id = q.Id,
                Title = q.Title,
                Score = q.Score,
                IsMulti = q.IsMulti.Value,
                AnswerOptions = JsonHelper.FromJson<AnswerOptionEntry[]>(q.AnswerOptions).ToList(),
                RightAnswer = q.RightAnswer,
                ImgUrl = q.ImgUrl
            }).ToList();

            // 答题信息
            var answerDetail = JsonHelper.FromJson<List<Answerdetail>>(entity.AnswerDetail);
            foreach (var question in questions)
            {
                var answer = answerDetail.FirstOrDefault(a => a.QuestionId == question.Id);
                if (answer != null)
                {
                    if (answer.CheckedAnswer != null && answer.CheckedAnswer.Length > 0)
                        foreach (var ansOpt in question.AnswerOptions)
                        {
                            if (answer.CheckedAnswer.Contains(ansOpt.Value))
                            {
                                ansOpt.IsChecked = true;
                            }
                        }
                }
            }


            resData.Questions = questions;

            return new OperationResult(OperationResultType.Success, string.Empty, resData);
        }


    }
}
